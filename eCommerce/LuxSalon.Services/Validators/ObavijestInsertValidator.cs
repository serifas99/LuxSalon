using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class ObavijestInsertValidator : AbstractValidator<ObavijestInsertRequest>
    {
        public ObavijestInsertValidator()
        {
            RuleFor(x => x.Naslov)
                .NotEmpty().WithMessage("Naslov je obavezan.")
                .MaximumLength(150).WithMessage("Naslov ne smije biti duzi od 150 karaktera.");

            RuleFor(x => x.Tekst)
                .NotEmpty().WithMessage("Tekst je obavezan.")
                .MaximumLength(2000).WithMessage("Tekst ne smije biti duzi od 2000 karaktera.");
        }
    }
}
