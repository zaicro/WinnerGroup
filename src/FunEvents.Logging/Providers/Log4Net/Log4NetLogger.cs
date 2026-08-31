using FunEvents.Logging.Domain;
using log4net;
using log4net.Config;
using System.Reflection;
using System.Xml;

namespace FunEvents.Logging.Providers.Log4Net;

/// <summary>
/// Provides logging functionality using log4net.
/// </summary>
public sealed class Log4NetLogger : ILogger
{
    private readonly Func<string, bool>? _messageFilterFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetLogger"/> class
    /// using the default log4net configuration.
    /// </summary>
    public Log4NetLogger()
    {
        //XmlConfigurator.Configure();
        var configFile = Path.Combine(AppContext.BaseDirectory, "Config", "log4net.config");

        XmlConfigurator.Configure(
            LogManager.GetRepository(Assembly.GetEntryAssembly()!),
            new FileInfo(configFile));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetLogger"/> class
    /// using the specified XML configuration and message filter.
    /// </summary>
    /// <param name="xmlElement">The XML element containing the log4net configuration.</param>
    /// <param name="messageFilterFunc">An optional function used to filter messages.</param>
    public Log4NetLogger(XmlElement xmlElement, Func<string, bool>? messageFilterFunc)
    {
        ArgumentNullException.ThrowIfNull(xmlElement);
        _messageFilterFunc = messageFilterFunc;
        XmlConfigurator.Configure(xmlElement);
    }

    /// <inheritdoc />
    public void Info<T>(string methodName, string message)
    {
        Log(
            LogManager.GetLogger(typeof(T)),
            LogLevel.Info,
            methodName,
            message);
    }

    /// <inheritdoc />
    public void Debug<T>(string methodName, string message)
    {
        Log(
            LogManager.GetLogger(typeof(T)),
            LogLevel.Debug,
            methodName,
            message);
    }

    /// <inheritdoc />
    public void Error<T>(
        string methodName,
        string message,
        Exception? exception = null)
    {
        Log(
            LogManager.GetLogger(typeof(T)),
            LogLevel.Error,
            methodName,
            message,
            exception);
    }

    /// <inheritdoc />
    public void Warn<T>(
        string methodName,
        string message,
        Exception? exception = null)
    {
        Log(
            LogManager.GetLogger(typeof(T)),
            LogLevel.Warn,
            methodName,
            message,
            exception);
    }

    /// <inheritdoc />
    public void Fatal<T>(
        string methodName,
        string message,
        Exception? exception = null)
    {
        Log(
            LogManager.GetLogger(typeof(T)),
            LogLevel.Fatal,
            methodName,
            message,
            exception);
    }

    /// <inheritdoc />
    public void Info(MethodBase methodBase, string message)
    {
        LogMethod(
            methodBase,
            LogLevel.Info,
            message);
    }

    /// <inheritdoc />
    public void Debug(MethodBase methodBase, string message)
    {
        LogMethod(
            methodBase,
            LogLevel.Debug,
            message);
    }

    /// <inheritdoc />
    public void Error(
        MethodBase methodBase,
        string message,
        Exception? exception = null)
    {
        LogMethod(
            methodBase,
            LogLevel.Error,
            message,
            exception);
    }

    /// <inheritdoc />
    public void Warn(
        MethodBase methodBase,
        string message,
        Exception? exception = null)
    {
        LogMethod(
            methodBase,
            LogLevel.Warn,
            message,
            exception);
    }

    /// <inheritdoc />
    public void Fatal(
        MethodBase methodBase,
        string message,
        Exception? exception = null)
    {
        LogMethod(
            methodBase,
            LogLevel.Fatal,
            message,
            exception);
    }

    /// <summary>
    /// Logs a message using the declaring type and method represented by
    /// the specified <see cref="MethodBase"/>.
    /// </summary>
    private void LogMethod(
        MethodBase methodBase,
        LogLevel logLevel,
        string message,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(methodBase);

        var declaringType = GetDeclaringType(methodBase);
        var methodName = GetMethodName(methodBase);

        Log(
            LogManager.GetLogger(declaringType),
            logLevel,
            methodName,
            message,
            exception);
    }

    /// <summary>
    /// Logs a message at the specified log level.
    /// </summary>
    private void Log(
        ILog log,
        LogLevel logLevel,
        string methodName,
        string message,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(log);

        if (!ShouldLogMessage(message, exception)) return;

        ThreadContext.Properties["MethodName"] = string.IsNullOrWhiteSpace(methodName) ? "?" : methodName;

        try
        {
            switch (logLevel)
            {
                case LogLevel.Fatal when log.IsFatalEnabled:
                    log.Fatal(message, exception);
                    break;

                case LogLevel.Error when log.IsErrorEnabled:
                    log.Error(message, exception);
                    break;

                case LogLevel.Warn when log.IsWarnEnabled:
                    log.Warn(message, exception);
                    break;

                case LogLevel.Info when log.IsInfoEnabled:
                    log.Info(message, exception);
                    break;

                case LogLevel.Debug when log.IsDebugEnabled:
                    log.Debug(message, exception);
                    break;

                default:
                    break;
            }
        }
        finally
        {
            ThreadContext.Properties.Remove("MethodName");
        }
    }

    /// <summary>
    /// Determines whether the specified message should be logged.
    /// </summary>
    private bool ShouldLogMessage(
        string message,
        Exception? exception)
    {
        if (_messageFilterFunc?.Invoke(message) == true) return false;

        if (string.IsNullOrWhiteSpace(message) && exception is null) return false;

        return true;
    }

    /// <summary>
    /// Gets the type that declares the specified method.
    /// </summary>
    public static Type GetDeclaringType(MethodBase methodBase)
    {
        ArgumentNullException.ThrowIfNull(methodBase);

        return methodBase.DeclaringType ?? throw new ArgumentException("MethodBase does not have a valid declaring type.", nameof(methodBase));
    }

    /// <summary>
    /// Gets the name of the specified method.
    /// </summary>
    public static string GetMethodName(MethodBase methodBase)
    {
        ArgumentNullException.ThrowIfNull(methodBase);

        return methodBase.Name;
    }
}