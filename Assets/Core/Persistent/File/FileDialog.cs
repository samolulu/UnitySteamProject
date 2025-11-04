using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenDialogDir
{
	public IntPtr hwndOwner = IntPtr.Zero;
	public IntPtr pidlRoot = IntPtr.Zero;
	public String pszDisplayName = null;
	public String lpszTitle = null;
	public UInt32 ulFlags = 0;
	public IntPtr lpfn = IntPtr.Zero;
	public IntPtr lParam = IntPtr.Zero;
	public int iImage = 0;
	public string initialDir = null;



	/// <summary>
	/// 打开win路径选择窗口
	/// </summary>
	/// <param name="title"></param>
	/// <param name="defaultPath"></param>
	/// <returns></returns>
	public static string OpenWinDialogToGetFolder(string title = "Select Path",  string defaultPath = null)
	{
		try
		{
			OpenDialogDir ofn2 = new OpenDialogDir();
			ofn2.pszDisplayName = new string(new char[2048]); // 存放目录路径缓冲区  
			ofn2.lpszTitle = title; // 标题  
			ofn2.initialDir  = defaultPath; 
			ofn2.ulFlags =  0x00001000 | 0x00000800 | 0x00000040 | 0x00000002 | 0x00000001;  //https://learn.microsoft.com/zh-cn/windows/win32/api/shlobj_core/ns-shlobj_core-browseinfoa
			IntPtr pidlPtr = OpenFileDialog.SHBrowseForFolder(ofn2);

			char[] charArray = new char[2048];

			for (int i = 0; i < 2048; i++)
			{
				charArray[i] = '\0';
			}

			OpenFileDialog.SHGetPathFromIDList(pidlPtr, charArray);
			string res = new string(charArray);
			res = res.Substring(0, res.IndexOf('\0'));
			return res;
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}

		return string.Empty; 

	}

	/// <summary>
	/// 打开windows文件夹
	/// </summary>
	/// <param name="path"></param>
	public static void OpenWinFolder(string path, string selectFile = null)
	{
		if(!FileUtil.IsExistsDirectory(path)) 
		{
			Debug.Log($"文件夹不存在 {path}");
			return;
		}
		//OpenFileDialog.ShellExecute(IntPtr.Zero, "open", path, "", "", 1);
		

		try
		{
			if(selectFile == null)
			{
				System.Diagnostics.Process.Start(path);
			}
			else
			{
				var target =  Path.Combine(path, selectFile);
				// 检查路径是否存在
				if (Directory.Exists(target) || File.Exists(target))
				{
					// 启动资源管理器并选中指定的文件或文件夹
					System.Diagnostics.Process.Start("explorer.exe", $"/e,/select,\"{target}\"");
				}
				else
				{
					Debug.Log("指定的路径不存在。");
				}
			}

		}
		catch (Exception ex)
		{
			Debug.Log($"发生错误: {ex.Message}");
		}       
		
		
	}

}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class FileDialog
{
	public int structSize = 0;
	public IntPtr dlgOwner = IntPtr.Zero;
	public IntPtr instance = IntPtr.Zero;
	public String filter = null;
	public String customFilter = null;
	public int maxCustFilter = 0;
	public int filterIndex = 0;
	public String file = null;
	public int maxFile = 0;
	public String fileTitle = null;
	public int maxFileTitle = 0;
	public String initialDir = null;
	public String title = null;
	public int flags = 0;
	public short fileOffset = 0;
	public short fileExtension = 0;
	public String defExt = null;
	public IntPtr custData = IntPtr.Zero;
	public IntPtr hook = IntPtr.Zero;
	public String templateName = null;
	public IntPtr reservedPtr = IntPtr.Zero;
	public int reservedInt = 0;
	public int flagsEx = 0;
	//public String FileName = null;


	public static string OpenFileDialogToSave(string fileName, string filter, string title, string defExt)
	{

		SaveFileDlg sfn = new SaveFileDlg();
		sfn.structSize = Marshal.SizeOf(sfn);
		// sfn.filter = "文件(*.xls)\0*.xls\0";
		//sfn.filter = "图片文件(*.jpg;*.png)\0*.jpg;*.png\0";
		//sfn.filter = "地图文件(*.mData)\0*.mData\0";
		//
		//sfn.title = "保存";
		sfn.filter = filter;

		sfn.file = new string(new char[1024]);
		sfn.maxFile = sfn.file.Length;

		sfn.fileTitle = new string(new char[256]);


		sfn.maxFileTitle = sfn.fileTitle.Length;
		// sfn.initialDir = Application.dataPath;
		//sfn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		
		sfn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		sfn.file = fileName;
		sfn.title = title;
		//sfn.defExt = "xls";
		sfn.defExt = defExt;

		sfn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
		sfn.dlgOwner = LocalDialog.GetForegroundWindow(); //这一步将文件选择窗口置顶。

		try
		{
			if (SaveFileDialog.GetSaveFileName(sfn))
			{

				return sfn.file;
			}
		}
		catch (Exception e)
		{

			Debug.LogError($" e.Message:{ e.Message}");
		}
		
		return string.Empty;
	}


	public static string OpenFile(string filter, string title, string defExt)
	{

		OpenFileDlg sfn = new OpenFileDlg();
		sfn.structSize = Marshal.SizeOf(sfn);
		//sfn.filter = "文件(*.xls)\0*.xls\0";
		//sfn.filter = "地图文件(*.mData)\0*.mData\0";
		// sfn.filter = "exe files\0*.exe\0All Files\0*.*\0\0";
		//sfn.filter = "*.png\0*.png\0*.jpg\0*.jpg*\0\0";
		//// sfn.title = "打开地图文件";
		// sfn.defExt = "*.mData";
		sfn.filter = filter;
		sfn.file = new string(new char[1024]);
		sfn.maxFile = sfn.file.Length;
		// sfn.file = name;
		sfn.fileTitle = new string(new char[256]);
		sfn.maxFileTitle = sfn.fileTitle.Length;
		// sfn.initialDir = Application.dataPath;
		//sfn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		sfn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
		// sfn.title = "打开地图文件";

		

		sfn.title = title;
		sfn.defExt = defExt;
		sfn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
		// sfn.defExt = "mData";
		sfn.dlgOwner = LocalDialog.GetForegroundWindow(); //这一步将文件选择窗口置顶。
		try
		{
			if (OpenFileDialog.GetOpenFileName(sfn))
			{
				return sfn.file;
			}
		}
		catch (Exception e)
		{

			Debug.LogError($" e.Message:{ e.Message}");
		}
		
		return string.Empty;
	}

}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileDlg : FileDialog
{

}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class SaveFileDlg : FileDialog
{


}

public class OpenFileDialog
{

	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool GetOpenFileName([In, Out] OpenFileDlg ofd); //声明该函数。

	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	private static extern bool GetSaveFileName([In, Out] OpenFileName ofn); 

	//窗口置顶
	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

	[DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);

	[DllImport("shell32.dll", CharSet=CharSet.Unicode)]
	public static extern int ShellExecute(IntPtr hwnd, string lpszOp, string lpszFile, string lpszParams, string lpszDir, int FsShowCmd);

}

public class SaveFileDialog
{

	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool GetSaveFileName([In, Out] SaveFileDlg ofd); //声明该函数。

	//窗口置顶
	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();
}



