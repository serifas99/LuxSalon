using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class RadnoVrijemeInsertValidator : AbstractValidator<RadnoVrijemeInsertRequest>
    {
        public RadnoVrijemeInsertValidator()
        {
            RuleFor(x => x.FrizerId)
                .GreaterThan(0).WithMessage("FrizerId je obavezan.");

            RuleFor(x => x.DanUSedmici)
                .InclusiveBetween(0, 6).WithMessage("DanUSedmici mora biti izmedju 0 (Nedjelja) i 6 (Subota).");

            RuleFor(x => x.PocetakRada)
                .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Pocetak rada mora biti u formatu HH:mm, npr. 08:00.");

            RuleFor(x => x.KrajRada)
                .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Kraj rada mora biti u formatu HH:mm, npr. 17:00.");

            RuleFor(x => x)
                .Must(x => x.NeRadi || string.Compare(x.PocetakRada, x.KrajRada, StringComparison.Ordinal) < 0)
                .WithMessage("Pocetak rada mora biti prije kraja rada.")
                .WithName("PocetakRada");
        }
    }
}
