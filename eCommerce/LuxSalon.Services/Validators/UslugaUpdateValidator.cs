using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class UslugaUpdateValidator : AbstractValidator<UslugaUpdateRequest>
    {
        public UslugaUpdateValidator()
        {
            RuleFor(x => x.Naziv)
                .NotEmpty().WithMessage("Naziv je obavezan.")
                .MaximumLength(150).WithMessage("Naziv ne smije biti duzi od 150 karaktera.");

            RuleFor(x => x.Cijena)
                .GreaterThan(0).WithMessage("Cijena mora biti veca od 0.");

            RuleFor(x => x.TrajanjeMinuta)
                .GreaterThan(0).WithMessage("Trajanje mora biti vece od 0 minuta.")
                .LessThanOrEqualTo(480).WithMessage("Trajanje ne moze biti duze od 8 sati.");

            RuleFor(x => x.Opis)
                .MaximumLength(1000).WithMessage("Opis ne smije biti duzi od 1000 karaktera.");
        }
    }
}
