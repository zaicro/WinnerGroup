using FunEvents.Logging.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FunEvents.Logging.Tests;

[TestFixture]
public class LoggerTests
{
    private ServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddLoggingServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public void ShouldResolveLogger()
    {
        // Act
        var logger = _serviceProvider.GetRequiredService<ILogger>();

        // Assert
        Assert.That(logger, Is.Not.Null);
    }

    [Test]
    public void Info_ShouldWriteLog()
    {
        // Arrange
        var logger = _serviceProvider.GetRequiredService<ILogger>();
        var message = "Test Info Message";

        // Act
        logger.Info<LoggerTests>(
            nameof(Info_ShouldWriteLog),
            message);

        // Assert
        Assert.DoesNotThrow(() =>
            logger.Info<LoggerTests>(
                nameof(Info_ShouldWriteLog),
                message));
    }

    [Test]
    public void Error_ShouldWriteLog()
    {
        // Arrange
        var logger = _serviceProvider.GetRequiredService<ILogger>();
        var message = "Test Error Message";
        var exception = new Exception("Test Exception");

        // Act
        Assert.DoesNotThrow(() =>
            logger.Error<LoggerTests>(
                nameof(Error_ShouldWriteLog),
                message,
                exception));
    }
}