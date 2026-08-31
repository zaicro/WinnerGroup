using System.Reflection;

namespace FunEvents.Logging.Domain;

/// <summary>
/// Defines the contract for application logging.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <typeparam name="T">
    /// The type associated with the source of the log message.
    /// </typeparam>
    /// <param name="methodName">
    /// The name of the method that generated the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    void Info<T>(string methodName, string message);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <typeparam name="T">
    /// The type associated with the source of the log message.
    /// </typeparam>
    /// <param name="methodName">
    /// The name of the method that generated the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    void Debug<T>(string methodName, string message);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <typeparam name="T">
    /// The type associated with the source of the log message.
    /// </typeparam>
    /// <param name="methodName">
    /// The name of the method that generated the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    /// <param name="exception">
    /// The exception associated with the error, if any.
    /// </param>
    void Error<T>(
        string methodName,
        string message,
        Exception? exception = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <typeparam name="T">
    /// The type associated with the source of the log message.
    /// </typeparam>
    /// <param name="methodName">
    /// The name of the method that generated the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    /// <param name="exception">
    /// The exception associated with the warning, if any.
    /// </param>
    void Warn<T>(
        string methodName,
        string message,
        Exception? exception = null);

    /// <summary>
    /// Logs a fatal error message.
    /// </summary>
    /// <typeparam name="T">
    /// The type associated with the source of the log message.
    /// </typeparam>
    /// <param name="methodName">
    /// The name of the method that generated the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    /// <param name="exception">
    /// The exception associated with the fatal error, if any.
    /// </param>
    void Fatal<T>(
        string methodName,
        string message,
        Exception? exception = null);

    /// <summary>
    /// Logs an informational message using the specified method metadata.
    /// </summary>
    /// <param name="methodBase">
    /// The method metadata representing the source of the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    void Info(MethodBase methodBase, string message);

    /// <summary>
    /// Logs a debug message using the specified method metadata.
    /// </summary>
    /// <param name="methodBase">
    /// The method metadata representing the source of the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    void Debug(MethodBase methodBase, string message);

    /// <summary>
    /// Logs an error message using the specified method metadata.
    /// </summary>
    /// <param name="methodBase">
    /// The method metadata representing the source of the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    /// <param name="exception">
    /// The exception associated with the error, if any.
    /// </param>
    void Error(
        MethodBase methodBase,
        string message,
        Exception? exception = null);

    /// <summary>
    /// Logs a warning message using the specified method metadata.
    /// </summary>
    /// <param name="methodBase">
    /// The method metadata representing the source of the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    /// <param name="exception">
    /// The exception associated with the warning, if any.
    /// </param>
    void Warn(
        MethodBase methodBase,
        string message,
        Exception? exception = null);

    /// <summary>
    /// Logs a fatal error message using the specified method metadata.
    /// </summary>
    /// <param name="methodBase">
    /// The method metadata representing the source of the log message.
    /// </param>
    /// <param name="message">
    /// The message to log.
    /// </param>
    /// <param name="exception">
    /// The exception associated with the fatal error, if any.
    /// </param>
    void Fatal(
        MethodBase methodBase,
        string message,
        Exception? exception = null);
}