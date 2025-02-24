
namespace TemplateMQ.Test.MiddlewareTests;

public class ExceptionHandlerTests
{
    private Mock<RequestDelegate> _nextMock;
    private Mock<ILogger<ExceptionHandler>> _loggerMock;
    private ExceptionHandler _exceptionHandler;

    [SetUp]
    public void SetUp()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandler>>();
        _exceptionHandler = new ExceptionHandler(_nextMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Invoke_NoException_ShouldCallNext()
    {
        // Arrange
        DefaultHttpContext context = new();

        // Act
        await _exceptionHandler.Invoke(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Test]
    public async Task Invoke_ValidationException_ShouldHandleValidationException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        List<ValidationFailure> validationFailures =
        [
            new("Property1", "Error1"),
            new("Property2", "Error2")
        ];

        ValidationException validationException = new(validationFailures);

        _nextMock.Setup(next => next(context)).ThrowsAsync(validationException);

        // Act
        await _exceptionHandler.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        StreamReader reader = new(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(errorResponse?.Title, Is.EqualTo("Validation Failed"));
            Assert.That(errorResponse?.Errors.Count, Is.EqualTo(2));
            Assert.That(errorResponse?.Errors.ContainsKey("Property1"), Is.True);
            Assert.That(errorResponse?.Errors.ContainsKey("Property2"), Is.True);
        });
    }

    [Test]
    public async Task Invoke_ArgumentException_ShouldHandleArgumentException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        var argumentException = new ArgumentException("Invalid argument");

        _nextMock.Setup(next => next(context)).ThrowsAsync(argumentException);

        // Act
        await _exceptionHandler.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        StreamReader reader = new(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(errorResponse?.Title, Is.EqualTo("Validation Failed"));
            Assert.That(errorResponse?.Errors.Count, Is.EqualTo(1));
            Assert.That(errorResponse?.Errors.ContainsKey("General"), Is.True);
            Assert.That(errorResponse?.Errors["General"][0], Is.EqualTo("Invalid argument"));
        });
    }

    [Test]
    public async Task Invoke_GenericException_ShouldHandleGenericException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        Exception exception = new("Unexpected error");

        _nextMock.Setup(next => next(context)).ThrowsAsync(exception);

        // Act
        await _exceptionHandler.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        StreamReader reader = new(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(errorResponse?.Title, Is.EqualTo("An unexpected error occurred."));
            Assert.That(errorResponse?.Errors.Count, Is.EqualTo(1));
            Assert.That(errorResponse?.Errors.ContainsKey("General"), Is.True);
            Assert.That(errorResponse?.Errors["General"][0], Is.EqualTo("An unexpected error occurred. Please try again later."));
        });
       
    }
}
