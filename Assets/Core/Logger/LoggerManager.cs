using System;
using System.IO;
using UnityEngine;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Unity;
using ZLogger.Providers;

/// <summary>
/// 日志输出到控制台和文件的管理
/// 正确使用ZLogger可零GC高性能
/// 示例： var logger = LoggerManager.Get(); logger.ZLogDebug($"LogDebug{name}");
/// </summary>
public class LoggerManager : MonoBehaviour
{

	private static ILogger<LoggerManager> logger_global = default!;


	// 单例模式
	public static LoggerManager Instance { get; private set; } = default!;

	/// <summary>
	/// 获取全局logger
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	public static ILogger<LoggerManager> Get()
	{
		return logger_global ?? throw new NullReferenceException("logger null error");
	}

	private ILoggerFactory _loggerFactory_global = default!;
	private ILoggerFactory _loggerFactory_file_info = default!;
	private ILoggerFactory _loggerFactory_file_warning = default!;
	private ILoggerFactory _loggerFactory_file_error = default!;
	private ILogger<LoggerManager> logger_file_info = default!;
	private ILogger<LoggerManager> logger_file_warning = default!;
	private ILogger<LoggerManager> logger_file_error = default!;

	void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
		InitializeLogging();
	}

	/// <summary>
	/// 滚动文件检查
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	string RollingFile(string path)
	{
		if (string.IsNullOrWhiteSpace(path)) return path;
		string roll = path.Replace(".log", "_prev.log"); 
		if (File.Exists(roll)) File.Delete(roll);;
		if (File.Exists(path)) File.Move(path, roll);
		return path;
	}

	void InitializeLogging()
	{
		// 创建全局控制台日志工厂
		_loggerFactory_global = LoggerFactory.Create(builder =>
		{
			builder
			.ClearProviders()
			.SetMinimumLevel(LogLevel.Trace)
			.AddZLoggerUnityDebug(options =>
			{
				//options.PrettyStacktrace = true;
				// options.UsePlainTextFormatter(formatter =>
				// {
				// 	formatter.SetPrefixFormatter($"[ZLog]", (in MessageTemplate template, in LogInfo info) =>
				// 	{
				// 		template.Format();
				// 	});
				// });	
			})
			;
		});


		// 创建文件输出日志工厂
#if UNITY_EDITOR
		string logDirectory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs/Player");	
#else
		string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
#endif
		
		if (Directory.Exists(logDirectory) == false) Directory.CreateDirectory(logDirectory);
		string path_info = RollingFile(Path.Combine(logDirectory, "Info.log"));
		string path_warning = RollingFile(Path.Combine(logDirectory, "Warning.log"));
		string path_error = RollingFile(Path.Combine(logDirectory, "Error.log"));
 
		
		//配置ZLoggerFileOptions
		Action<ZLoggerFileOptions> configure = options =>
		{
			options.FileShared = false;
			//options.FullMode = BackgroundBufferFullMode.Grow;
			options.UsePlainTextFormatter(formatter =>
			{
				// 2023-12-19 02:46:14.289 ......
				formatter.SetPrefixFormatter($"[{0:local-longdate}] ", (in MessageTemplate template, in LogInfo info) =>
				{
					template.Format(info.Timestamp);
				});
			});

		};
      
		_loggerFactory_file_info = LoggerFactory.Create(builder =>
		{
			builder.ClearProviders().AddFilter(null, (LogLevel level) => { return level <= LogLevel.Information; }).AddZLoggerFile(path_info, configure);
		});

		_loggerFactory_file_warning = LoggerFactory.Create(builder =>
		{
			builder.ClearProviders().AddFilter(null, (LogLevel level) => { return level == LogLevel.Warning; }).AddZLoggerFile(path_warning, configure);
		});

		_loggerFactory_file_error = LoggerFactory.Create(builder =>
		{
			builder.ClearProviders().AddFilter(null, (LogLevel level) => { return level >= LogLevel.Error; }).AddZLoggerFile(path_error, configure);
		});

		logger_global = _loggerFactory_global.CreateLogger<LoggerManager>();
		logger_global.LogInformation("Logging system initialized", this);

		logger_file_info = _loggerFactory_file_info.CreateLogger<LoggerManager>();
		logger_file_warning = _loggerFactory_file_warning.CreateLogger<LoggerManager>();
		logger_file_error = _loggerFactory_file_error.CreateLogger<LoggerManager>();
		logger_file_info.LogInformation("Logging Start");
		logger_file_warning.LogWarning("Logging Start");
		logger_file_error.LogCritical("Logging Start");

		Application.logMessageReceived += HandleUnityLog;

	}

	/// <summary>
	/// 接管消息并将日志写入到对应级别文件
	/// </summary>
	/// <param name="logString"></param>
	/// <param name="stackTrace"></param>
	/// <param name="type"></param>
	void HandleUnityLog(string logString, string stackTrace, LogType type)
	{
		switch (type)
		{
			case LogType.Error:
			case LogType.Exception:
				logger_file_error.ZLogError($"{logString}\n{stackTrace}");
				break;
			case LogType.Warning:
				logger_file_warning.ZLogWarning($"{logString}");
				break;
			case LogType.Log:
				logger_file_info.ZLogInformation($"{logString}");
				break;
			case LogType.Assert:
				logger_file_error.ZLogCritical($"{logString}");
				break;
		}
	}


	void OnDestroy()
	{
		logger_global.LogInformation("Logging system shutdown");
		logger_file_info.LogInformation("Logging End\n--------------------------------------------");
		logger_file_warning.LogWarning("Logging End\n--------------------------------------------");
		logger_file_error.LogCritical("Logging End\n--------------------------------------------");

		Application.logMessageReceived -= HandleUnityLog;

		_loggerFactory_global.Dispose();
		_loggerFactory_file_info.Dispose();
		_loggerFactory_file_warning.Dispose();
		_loggerFactory_file_error.Dispose();
	}
}