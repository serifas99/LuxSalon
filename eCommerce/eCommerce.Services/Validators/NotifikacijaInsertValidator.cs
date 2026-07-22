using eCommerce.Model.Requests;
using FluentValidation;

namespace eCommerce.Services.Validators
{
    public class NotifikacijaInsertValidator : AbstractValidator<NotifikacijaInsertRequest>
    {
        public NotifikacijaInsertValidator()
        {
            RuleFor(x => x.KorisnikId)
                .GreaterThan(0).WithMessage("KorisnikId je obavezan.");

            RuleFor(x => x.Naslov)
                .NotEmpty().WithMessage("Naslov je obavezan.")
                .MaximumLength(150).WithMessage("Naslov ne smije biti duzi od 150 karaktera.");

            RuleFor(x => x.Poruka)
                .NotEmpty().WithMessage("Poruka je obavezna.")
                .MaximumLength(1000).WithMessage("Poruka ne smije biti duza od 1000 karaktera.");
        }
    }
}
