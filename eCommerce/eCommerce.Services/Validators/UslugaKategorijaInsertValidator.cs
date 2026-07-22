using eCommerce.Model.Requests;
using FluentValidation;

namespace eCommerce.Services.Validators
{
    public class UslugaKategorijaInsertValidator : AbstractValidator<UslugaKategorijaInsertRequest>
    {
        public UslugaKategorijaInsertValidator()
        {
            RuleFor(x => x.Naziv)
                .NotEmpty().WithMessage("Naziv je obavezan.")
                .MaximumLength(100).WithMessage("Naziv ne smije biti duzi od 100 karaktera.");

            RuleFor(x => x.Opis)
                .MaximumLength(500).WithMessage("Opis ne smije biti duzi od 500 karaktera.");
        }
    }
}
