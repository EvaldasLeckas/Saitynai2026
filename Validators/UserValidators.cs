using FluentValidation;
using Saitynai.DTO;

namespace Saitynai.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Bio).MaximumLength(1000);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Today)
            .WithMessage("BirthDate must be in the past.")
            .GreaterThan(new DateTime(1900, 1, 1))
            .WithMessage("BirthDate is not valid.");
    }
}