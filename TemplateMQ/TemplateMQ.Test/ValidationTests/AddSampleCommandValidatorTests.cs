namespace TemplateMQ.Test.ValidationTests;

[TestFixture]
public class AddSampleCommandValidatorTests
{
    private AddSampleCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new AddSampleCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        AddSampleCommand command = new(name: "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is Required.");
    }

    [Test]
    public void Should_Not_Have_Error_When_Name_Is_Provided()
    {
        // Arrange
        AddSampleCommand command = new( name :"Valid Name" );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
