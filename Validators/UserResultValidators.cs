using FluentValidation;
using Saitynai.DTO;

namespace Saitynai.Validators;

public class CreateUserResultDtoValidator : AbstractValidator<CreateUserResultDto>
{
    public CreateUserResultDtoValidator()
    {
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Placement).GreaterThan(0);
        RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}