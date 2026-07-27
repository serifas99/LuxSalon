using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class UslugaKategorijaUpdateValidator : AbstractValidator<UslugaKategorijaUpdateRequest>
    {
        public UslugaKategorijaUpdateValidator()
        {
            RuleFor(x => x.Naziv)
                .NotEmpty().WithMessage("Naziv je obavezan.")
                .MaximumLength(100).WithMessage("Naziv ne smije biti duzi od 100 karaktera.");

            RuleFor(x => x.Opis)
                .MaximumLength(500).WithMessage("Opis ne smije biti duzi od 500 karaktera.");
        }
    }
}
