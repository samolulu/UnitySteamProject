using System;
using System.IO;
using UnityEngine;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Unity;
//using ILogger = Microsoft.Extensions.Logging.ILogger;
public class LogManager : MonoBehaviour
{
 
    private ILoggerFactory _loggerFactory;
 	private ILogger<LogManager> logger;
	
    // 单例模式
	public static LogManager Instance { get; private set; }

	public static ILogger<T> CreateLogger<T>()
	{
		return Instance._loggerFactory.CreateLogger<T>();
	}
	
	public static ILogger<LogManager> Get()
	{
		return Instance.logger;
	}

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

    void InitializeLogging()
    {
        string logDirectory = Path.Combine(Application.persistentDataPath, "logs");
        //Directory.CreateDirectory(logDirectory);
        
        // ✅ 创建日志工厂
        _loggerFactory = LoggerFactory.Create(builder =>
        {
			builder
				.ClearProviders()
                .SetMinimumLevel(LogLevel.Trace)
                .AddZLoggerUnityDebug(
					// options =>
                    // {
					// 	options.FullMode = BackgroundBufferFullMode.Grow;   
					// 	options.IncludeScopes = true;   
                    // }
				)
                .AddZLoggerFile(
                    Path.Combine(logDirectory, "app.log"),
                    options =>
                    {
						//options.CaptureThreadInfo
                    }
				);
        });

        logger = CreateLogger<LogManager>();
        logger.LogInformation("Logging system initialized");
        Application.logMessageReceived += HandleUnityLog;
    }

	void HandleUnityLog(string logString, string stackTrace, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Exception:
					logger.LogError(new Exception(logString), "Unity Error: {StackTrace}", stackTrace);  
					break;
				case LogType.Warning:
					logger.LogWarning("Unity Warning: {LogString}", logString);  
					break;
				case LogType.Log:
					logger.LogInformation("Unity Log: {LogString}", logString);  
					break;
				case LogType.Assert:
					logger.LogCritical("Unity Assert: {LogString}", logString);  
					break;
			}
		}

    void OnDestroy()
    {
        logger.LogInformation("Logging system shutdown");  
        _loggerFactory.Dispose(); 
        Application.logMessageReceived -= HandleUnityLog;
    }
}