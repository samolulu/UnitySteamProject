using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics; // 新增引用

// 主线程调度器（确保UI操作在主线程执行）
public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    private static readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();
    private static int _mainThreadId;

	public static MainThreadDispatcher Instance
		{
			get
			{
				// 先查找场景中是否已存在实例
				if (_instance == null)
				{
					_instance = FindObjectOfType<MainThreadDispatcher>();
					
					// 如果场景中没有，创建新的GameObject
					if (_instance == null)
					{
						GameObject obj = new GameObject("MainThreadDispatcher");
						_instance = obj.AddComponent<MainThreadDispatcher>();
						
						// 仅在运行时设置DontDestroyOnLoad
						if (Application.isPlaying)
							DontDestroyOnLoad(obj);
						
						_mainThreadId = Thread.CurrentThread.ManagedThreadId;
					}
					else
					{
						// 如果找到实例，初始化主线程ID（如果尚未初始化）
						if (_mainThreadId == 0)
							_mainThreadId = Thread.CurrentThread.ManagedThreadId;
					}
				}
				return _instance;
			}
		}

    public bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

    private void Update()
    {
        // 每帧处理主线程任务队列
        while (_actionQueue.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    // 在主线程执行Action（异步）
    public Task ExecuteOnMainThreadAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();
        if (IsMainThread)
        {
            action?.Invoke();
            tcs.SetResult(true);
        }
        else
        {
            _actionQueue.Enqueue(() =>
            {
                try
                {
                    action?.Invoke();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
        }
        return tcs.Task;
    }

    // 在主线程执行Func（异步，带返回值）
    public Task<T> ExecuteOnMainThreadAsync<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        if (IsMainThread)
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }
        else
        {
            _actionQueue.Enqueue(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
        }
        return tcs.Task;
    }
}


// 文件夹选择对话框结构体（与Windows API严格对齐）
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class OpenDialogDir
{
	public IntPtr hwndOwner = IntPtr.Zero;
	public IntPtr pidlRoot = IntPtr.Zero;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string pszDisplayName = null;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpszTitle = null;
	public uint ulFlags = 0;
	public IntPtr lpfn = IntPtr.Zero;
	public IntPtr lParam = IntPtr.Zero;
	public int iImage = 0;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string initialDir = null;


	/// <summary>
	/// 打开文件夹选择窗口（确保在主线程调用）
	/// </summary>
	public static async Task<string> OpenWinDialogToGetFolderAsync(string title = "选择路径", string defaultPath = null)
	{
		return await MainThreadDispatcher.Instance.ExecuteOnMainThreadAsync(() =>
		{
			try
			{
				var ofn2 = new OpenDialogDir();
				ofn2.pszDisplayName = new string('\0', 2048); // 固定缓冲区
				ofn2.lpszTitle = title;
				ofn2.initialDir = defaultPath;
				ofn2.ulFlags = 0x00001000 | 0x00000800 | 0x00000040 | 0x00000002 | 0x00000001;

				IntPtr pidlPtr = OpenFileDialog.SHBrowseForFolder(ofn2);
				if (pidlPtr == IntPtr.Zero) return string.Empty;

				char[] charArray = new char[2048];
				Array.Fill(charArray, '\0');
				if (OpenFileDialog.SHGetPathFromIDList(pidlPtr, charArray))
				{
					return new string(charArray).TrimEnd('\0');
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"文件夹选择失败: {e.Message}");
			}
			return string.Empty;
		});
	}

	/// <summary>
	/// 打开Windows文件夹（支持选中文件）
	/// </summary>
	public static void OpenWinFolder(string path, string selectFile = null)
	{
		if (!Directory.Exists(path))
		{
			UnityEngine.Debug.LogError($"文件夹不存在: {path}");
			return;
		}

		try
		{
			if (selectFile == null)
			{
				// 直接使用System.Diagnostics.Process
				Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
			}
			else
			{
				string target = Path.Combine(path, selectFile);
				if (Directory.Exists(target) || File.Exists(target))
				{
					// 直接使用System.Diagnostics.Process
					Process.Start(new ProcessStartInfo("explorer.exe", $"/e,/select,\"{target}\"") { UseShellExecute = true });
				}
				else
				{
					UnityEngine.Debug.LogError($"目标文件不存在: {target}");
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError($"打开文件夹失败: {ex.Message}");
		}
	}
}

// 保存文件对话框结构体（与Windows OPENFILENAME严格对齐，无继承）
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class SaveFileDialogStruct
{
	public int lStructSize;
	public IntPtr hwndOwner;
	public IntPtr hInstance;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrFilter;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrCustomFilter;
	public int nMaxCustFilter;
	public int nFilterIndex;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrFile; // 文件名缓冲区
	public int nMaxFile; // 缓冲区最大长度
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrFileTitle;
	public int nMaxFileTitle;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrInitialDir;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrTitle;
	public int Flags;
	public short nFileOffset;
	public short nFileExtension;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrDefExt;
	public IntPtr lCustData;
	public IntPtr lpfnHook;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpTemplateName;
	public IntPtr lpReserved;
	public int dwReserved;
	public int FlagsEx;
}

// 打开文件对话框结构体（与Windows OPENFILENAME严格对齐，无继承）
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class OpenFileDialogStruct
{
	public int lStructSize;
	public IntPtr hwndOwner;
	public IntPtr hInstance;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrFilter;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrCustomFilter;
	public int nMaxCustFilter;
	public int nFilterIndex;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrFile; // 文件名缓冲区
	public int nMaxFile; // 缓冲区最大长度
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrFileTitle;
	public int nMaxFileTitle;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrInitialDir;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrTitle;
	public int Flags;
	public short nFileOffset;
	public short nFileExtension;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpstrDefExt;
	public IntPtr lCustData;
	public IntPtr lpfnHook;
	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpTemplateName;
	public IntPtr lpReserved;
	public int dwReserved;
	public int FlagsEx;
}

public static class FileDialog
{
	/// <summary>
	/// 打开保存文件对话框（异步，自动切换主线程）
	/// </summary>
	public static async Task<string> OpenFileDialogToSaveAsync(string fileName, string filter, string title, string defExt)
	{
		return await MainThreadDispatcher.Instance.ExecuteOnMainThreadAsync(() =>
		{
			try
			{
				var ofn = new SaveFileDialogStruct();
				ofn.lStructSize = Marshal.SizeOf(ofn);
				ofn.hwndOwner = OpenFileDialog.GetForegroundWindow(); // 置顶当前窗口
				ofn.lpstrFilter = filter;
				ofn.lpstrTitle = title;
				ofn.lpstrDefExt = defExt;
				ofn.lpstrInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

				// 初始化文件名缓冲区（固定1024字符，避免长度异常）
				char[] fileBuffer = new char[1024];
				Array.Fill(fileBuffer, '\0');
				if (!string.IsNullOrEmpty(fileName))
				{
					Array.Copy(fileName.ToCharArray(), fileBuffer, Math.Min(fileName.Length, 1023));
				}
				ofn.lpstrFile = new string(fileBuffer);
				ofn.nMaxFile = fileBuffer.Length;

				// 设置对话框标志（确保路径存在、不改变当前目录）
				ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

				if (OpenFileDialog.GetSaveFileName(ofn))
				{
					return ofn.lpstrFile.TrimEnd('\0'); // 去除空字符
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"保存对话框异常: {e.Message}");
			}
			return string.Empty;
		});
	}

	/// <summary>
	/// 打开文件选择对话框（异步，自动切换主线程）
	/// </summary>
	public static async Task<string> OpenFileAsync(string filter, string title, string defExt)
	{
		return await MainThreadDispatcher.Instance.ExecuteOnMainThreadAsync(() =>
		{
			try
			{
				var ofn = new OpenFileDialogStruct();
				ofn.lStructSize = Marshal.SizeOf(ofn);
				ofn.hwndOwner = OpenFileDialog.GetForegroundWindow();
				ofn.lpstrFilter = filter;
				ofn.lpstrTitle = title;
				ofn.lpstrDefExt = defExt;
				ofn.lpstrInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

				// 初始化缓冲区
				ofn.lpstrFile = new string('\0', 1024);
				ofn.nMaxFile = 1024;
				ofn.lpstrFileTitle = new string('\0', 256);
				ofn.nMaxFileTitle = 256;

				ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

				if (OpenFileDialog.GetOpenFileName(ofn))
				{
					return ofn.lpstrFile.TrimEnd('\0');
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"文件选择失败: {e.Message}");
			}
			return string.Empty;
		});
	}
}

internal static class OpenFileDialog
{
	// 移除ThrowOnUnmappableChar，避免字符映射异常
	[DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool GetOpenFileName([In, Out] OpenFileDialogStruct ofn);

	[DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool GetSaveFileName([In, Out] SaveFileDialogStruct ofn);

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

	[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [Out] char[] fileName);
}
