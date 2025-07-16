using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;


public static class FileUtil
{
	public static Encoding encoding = Encoding.UTF8;

	public static bool IsExistsFile(string path)
	{
		if (string.IsNullOrEmpty(path)) return false;

		return File.Exists(path);
	}

	public static bool IsExistsDirectory(string path)
	{
		if (string.IsNullOrEmpty(path)) return false;

		return Directory.Exists(path);
	}

	/// <summary>
	/// 复制文件夹
	/// </summary>
	/// <param name="sourceDirectory"></param>
	/// <param name="destDirectory"></param>
	/// <param name="exclude">排除的文件类型</param>
	/// <param name="deepExclude">是否每一层都排除</param>
	/// <exception cref="Exception"></exception>
	public static void CopyDirectory(string sourceDirectory, string destDirectory, string? exclude = ".exe|.bat", bool deepExclude = false)
	{
		if (string.IsNullOrEmpty(sourceDirectory)) return;
		if (string.IsNullOrEmpty(destDirectory)) return;
		if (!Directory.Exists(sourceDirectory)) return;

		try
		{

			if (!Directory.Exists(destDirectory))
			{
				CreateDirectory(destDirectory);
			}

			//拷贝文件
			CopyDirectoryFiles(sourceDirectory, destDirectory, exclude);
			//拷贝子目录       
			//获取所有子目录名称
			string[] directionName = Directory.GetDirectories(sourceDirectory);
			foreach (string directionPath in directionName)
			{
				//根据每个子目录名称生成对应的目标子目录名称
				string directionPathTemp = Path.Combine(destDirectory, directionPath.Substring(sourceDirectory.Length + 1));// destDirectory + "\\" + directionPath.Substring(sourceDirectory.Length + 1);
																															//递归下去
				CopyDirectory(directionPath, directionPathTemp, deepExclude ? exclude : null);
			}

		}
		catch (Exception ex)
		{
			throw new Exception("CopyDirectory error:" + ex.Message);
		}

	}

	/// <summary>
	/// 复制一批文件到
	/// </summary>
	/// <param name="path"></param>
	/// <param name="toPath"></param>
	/// <param name="copyFileNames"></param>
	public static void CopyDirectory(string path, string toPath, List<string> copyFileNames)
	{
		if (copyFileNames == null || copyFileNames.Count == 0) return;
		if (string.IsNullOrEmpty(path)) return;
		if (string.IsNullOrEmpty(toPath)) return;
		if (toPath.Contains(path)) return;
		if (!Directory.Exists(path)) return;

		string[] files = Directory.GetFiles(path);
		if (files.Length == 0) return;
		if (!Directory.Exists(toPath))
		{
			Directory.CreateDirectory(toPath);
		}
		string filePath, toFilePath;
		//string lowerFileName;
		for (int i = 0; i < copyFileNames.Count; ++i)
		{
			toFilePath = copyFileNames[i];
			filePath = string.Format("{0}/{1}", path, copyFileNames[i]);
			if (!File.Exists(filePath))
			{
				continue;
			}
			//lowerFileName = toFilePath.ToLower();
			//if (lowerFileName.EndsWith("png") || lowerFileName.EndsWith("jpg"))
			//{
				toFilePath = string.Format("{0}/{1}", toPath, toFilePath);
				//Debug.LogError(string.Format("CopyFile: {0} to {1}", filePath, toFilePath));
				CopyFile(filePath, toFilePath);
			//}

		}
	}

	/// <summary>
	/// 复制目录下所有文件到
	/// </summary>
	/// <param name="sourceDirectory"></param>
	/// <param name="destDirectory"></param>
	/// <param name="exclude"></param>
	/// <exception cref="Exception"></exception>
	public static void CopyDirectoryFiles(string sourceDirectory, string destDirectory, string? exclude = ".exe|.bat")
	{
		try
		{
			List<string> excludeSuffix = new();
			if (!string.IsNullOrEmpty(exclude)) excludeSuffix = exclude.ToStringList("|");

			//获取所有文件名称
			string[] fileName = Directory.GetFiles(sourceDirectory);
			foreach (string filePath in fileName)
			{
				//根据每个文件名称生成对应的目标文件名称
				string filePathTemp = Path.Combine(destDirectory, filePath.Substring(sourceDirectory.Length + 1));// destDirectory + "\\" + filePath.Substring(sourceDirectory.Length + 1);

				//剔除部分文件类型
				string suffix = Path.GetExtension(filePath);
				if (excludeSuffix.Contains(suffix)) continue;

				//若不存在，直接复制文件；若存在，覆盖复制
				if (File.Exists(filePathTemp))
				{
					CopyFile(filePath, filePathTemp, true);
				}
				else
				{
					CopyFile(filePath, filePathTemp);
				}
			}

		}
		catch (Exception ex)
		{
			throw new Exception("CopyDirectoryFiles error:" + ex.Message);
		}
	}

	/// <summary>
	/// 复制文件到
	/// </summary>
	/// <param name="path"></param>
	/// <param name="toPath"></param>
	/// <param name="overwrite"></param>
	/// <exception cref="Exception"></exception>
	public static void CopyFile(string path, string toPath, bool overwrite = false)
	{
		if (string.IsNullOrEmpty(path)) return;
		if (string.IsNullOrEmpty(toPath)) return;
		if (toPath.Equals(path, System.StringComparison.CurrentCultureIgnoreCase)) return;
		if (overwrite == false && File.Exists(toPath))
		{
			// Debug.LogError($"toPath:{toPath} path:{path}");
			//File.Delete(toPath);
			return;
		}

		try
		{
			File.Copy(path, toPath, overwrite);
		}
		catch (Exception ex)
		{
			throw new Exception("文件复制失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 删除文件
	/// </summary>
	/// <param name="filePath"></param>
	/// <exception cref="Exception"></exception>
	public static void DeleteFile(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath)) return;
		if (File.Exists(filePath))
		{
			try
			{
				File.Delete(filePath);
			}
			catch (Exception ex)
			{
				throw new Exception("文件删除失败：" + ex.Message);
			}

		}
	}

	//删除文件夹
	public static void DeleteDirectory(string path)
	{
		if (string.IsNullOrEmpty(path)) return;

		if (Directory.Exists(path))
		{
			try
			{
				Directory.Delete(path, true);
			}
			catch (Exception ex)
			{
				throw new Exception("删除目标目录失败：" + ex.Message);
			}
		}
	}

	/// <summary>
	/// 删除目录下的文件和文件夹（所有层级）
	/// </summary>
	/// <param name="path"></param>
	/// <exception cref="Exception"></exception>
	public static void DeleteDirectoryAndFiles(string path)
	{
		if (string.IsNullOrEmpty(path)) return;

		if (Directory.Exists(path))
		{
			try
			{

				foreach (string file in Directory.GetFiles(path))
				{
					DeleteFile(file);
				}
				foreach (string dir in Directory.GetDirectories(path))
				{
					DeleteDirectory(path);
				}
			}
			catch (Exception ex)
			{
				//UIManager.Instance.ShowGetItemAddToList(null, "删除目标目录文件失败,请先关闭对应的资源管理器界面", 0);
				throw new Exception("删除目标目录文件失败：" + ex.Message);
			}
		}

	}

	/// <summary>
	/// 删除目录下的文件（所有层级）
	/// </summary>
	/// <param name="directory"></param>
	/// <param name="extension">排除的文件类型</param>
	public static void DeleteAllDirectoryFiles(string directory, string extension)
	{
		if (Directory.Exists(directory))
		{
			string[] files = Directory.GetFiles(directory, "*" + extension, SearchOption.AllDirectories);

			foreach (string file in files)
			{
				try
				{
					File.Delete(file);
					Console.WriteLine($"已删除文件: {file}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"删除文件 {file} 时出现错误: {ex.Message}");
				}
			}
		}
		else
		{
			Console.WriteLine($"指定的目录 {directory} 不存在。");
		}
	}

	/// <summary>
	/// 创建文件夹
	/// </summary>
	/// <param name="path"></param>
	/// <exception cref="Exception"></exception>
	public static void CreateDirectory(string path)
	{
		if (string.IsNullOrEmpty(path)) return;
		if (!Directory.Exists(path))
		{
			try
			{
				Directory.CreateDirectory(path);
			}
			catch (Exception ex)
			{
				throw new Exception("创建目标目录失败：" + ex.Message);
			}
		}
	}

	/// <summary>
	/// 文件改名/移动
	/// </summary>
	/// <param name="path"></param>
	/// <param name="newPath"></param>
	/// <exception cref="Exception"></exception>
	public static void ReNameFile(string path, string newPath)
	{
		if (IsExistsFile(path) == false) return;

		try
		{
			File.Move(path, newPath);
		}
		catch (Exception ex)
		{
			throw new Exception("ReNameFile error:" + ex.Message);
		}
	}

	/// <summary>
	/// 文件夹改名
	/// </summary>
	/// <param name="path"></param>
	/// <param name="newFolderName"></param>
	public static void ReNameFolder(string path, string newFolderName)
	{
		try
		{
			// 检查原始文件夹是否存在
			if (Directory.Exists(path))
			{
				// 获取原始文件夹的父目录
				string parentDirectory = Directory.GetParent(path).FullName;
				// 构建新的文件夹路径
				string newFolderPath = Path.Combine(parentDirectory, newFolderName);

				// 检查新文件夹名是否已被使用
				if (!Directory.Exists(newFolderPath))
				{
					Directory.Move(path, newFolderPath);
				}
				else
				{
					UnityEngine.Debug.LogError($"新的文件夹名 {newFolderName} 已被使用，请选择其他名称。");
				}
			}
			else
			{
				UnityEngine.Debug.LogError($"原始文件夹 {path} 不存在。");
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError($"发生错误: {ex.Message}");
		}
	}

	/// <summary>
	/// 获取文件大小
	/// </summary>
	/// <param name="sFullName"></param>
	/// <returns></returns>
	public static long GetFileSize(string sFullName)
	{
		long lSize = 0;
		if (File.Exists(sFullName))
			lSize = new FileInfo(sFullName).Length;
		return lSize;
	} 
	
 
    /// <summary>
	/// 计算文件的MD5值
	/// </summary>
	public static string GetFileMD5(string file)
	{
		try
		{
			FileStream fs = new FileStream(file, FileMode.Open);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(fs);
			fs.Close();

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		}
		catch (Exception ex)
		{
			throw new Exception("md5file() fail, error:" + ex.Message);
		}
	}

}
