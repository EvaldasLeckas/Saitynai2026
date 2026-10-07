using FluentValidation;
using Saitynai.DTO;

namespace Saitynai.Validators;

public class CreateGameDtoValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();


        RuleFor(x => x.Difficulty).NotEmpty();
        RuleFor(x => x.GameLength).NotEmpty();
        RuleFor(x => x.PlayerCount).InclusiveBetween(1, 10);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SessionId).GreaterThanOrEqualTo(0);
    }
}