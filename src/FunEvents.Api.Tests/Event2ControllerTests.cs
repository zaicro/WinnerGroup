using Moq;
using FunEvents.Api.Controllers.v2;
using FunEvents.Application.Features.Event.Services;
using FunEvents.Application.Features.Event.Model.Commands;
using Microsoft.AspNetCore.Mvc;
using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Api.Tests;

[TestFixture]
public class Event2ControllerTests
{
    private Mock<ICreateEventService> _createEventServiceMock;
    private Mock<IUpdateEventService> _updateEventServiceMock;
    private Mock<IGetEventService> _getEventServiceMock;

    private Event2Controller _controller;

    [SetUp]
    public void SetUp()
    {
        _createEventServiceMock = new Mock<ICreateEventService>();
        _updateEventServiceMock = new Mock<IUpdateEventService>();
        _getEventServiceMock = new Mock<IGetEventService>();

        _controller = new Event2Controller(
            _createEventServiceMock.Object,
            _updateEventServiceMock.Object,
            _getEventServiceMock.Object);
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsResult_ShouldReturnOk()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Code = "EVT001",
            Name = "Sample Event",
            EventDate = DateTime.UtcNow.AddDays(1),
            Capacity = 10
        };

        var expectedResult = new EventDto
        {
            Code = "EVT001"
        };

        _createEventServiceMock.Setup(x => x.CreateEventAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateAsync(command, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));

        _createEventServiceMock.Verify(x => x.CreateEventAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}