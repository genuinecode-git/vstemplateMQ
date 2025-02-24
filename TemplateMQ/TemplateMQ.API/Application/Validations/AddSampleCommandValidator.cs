namespace TemplateMQ.API.Application.Validations;

public class AddSampleCommandValidator : AbstractValidator<AddSampleCommand>
{
    public AddSampleCommandValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty().WithMessage("Name is Required.");
    }
}
