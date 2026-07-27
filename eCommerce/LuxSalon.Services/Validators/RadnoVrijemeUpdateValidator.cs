using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class RadnoVrijemeUpdateValidator : AbstractValidator<RadnoVrijemeUpdateRequest>
    {
        public RadnoVrijemeUpdateValidator()
        {
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
