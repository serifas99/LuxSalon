using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class FrizerOcjenaUpdateValidator : AbstractValidator<FrizerOcjenaUpdateRequest>
    {
        public FrizerOcjenaUpdateValidator()
        {
            RuleFor(x => x.Ocjena)
                .InclusiveBetween(1, 5).WithMessage("Ocjena mora biti izmedju 1 i 5.");

            RuleFor(x => x.Komentar)
                .MaximumLength(500).WithMessage("Komentar ne smije biti duzi od 500 karaktera.");
        }
    }
}
