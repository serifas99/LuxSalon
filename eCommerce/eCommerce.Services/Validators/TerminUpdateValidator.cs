using eCommerce.Model.Requests;
using FluentValidation;

namespace eCommerce.Services.Validators
{
    public class TerminUpdateValidator : AbstractValidator<TerminUpdateRequest>
    {
        public TerminUpdateValidator()
        {
            RuleFor(x => x.DatumVrijeme)
                .GreaterThan(DateTime.UtcNow).WithMessage("Termin mora biti zakazan u buducnosti.");

            RuleFor(x => x.Napomena)
                .MaximumLength(500).WithMessage("Napomena ne smije biti duza od 500 karaktera.");
        }
    }
}
