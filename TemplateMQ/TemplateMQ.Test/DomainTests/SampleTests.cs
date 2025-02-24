namespace TemplateMQ.Test.DomainTests;

[TestFixture]
public class SampleTests
{
    [Test]
    public void Constructor_ValidName_ShouldCreateInstance()
    {
        // Arrange
        string validName = "Test Sample";

        // Act
        Sample sample = new(validName);

        // Assert
        Assert.That(sample.Name, Is.EqualTo(validName));
    }

    [Test]
    public void Constructor_NullName_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidName = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Sample(invalidName));
        Assert.That(ex.Message, Does.Contain("name is required"));
    }

    [Test]
    public void Constructor_EmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidName = "";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Sample(invalidName));
        Assert.That(ex.Message, Does.Contain("name is required"));
    }
}
