using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class TerminInsertValidator : AbstractValidator<TerminInsertRequest>
    {
        public TerminInsertValidator()
        {
            RuleFor(x => x.KlijentId)
                .GreaterThan(0).WithMessage("KlijentId je obavezan.");

            RuleFor(x => x.FrizerId)
                .GreaterThan(0).WithMessage("FrizerId je obavezan.");

            RuleFor(x => x.UslugaId)
                .GreaterThan(0).WithMessage("UslugaId je obavezan.");

            RuleFor(x => x.DatumVrijeme)
                .GreaterThan(DateTime.UtcNow).WithMessage("Termin mora biti zakazan u buducnosti.");

            RuleFor(x => x.Napomena)
                .MaximumLength(500).WithMessage("Napomena ne smije biti duza od 500 karaktera.");
        }
    }
}
