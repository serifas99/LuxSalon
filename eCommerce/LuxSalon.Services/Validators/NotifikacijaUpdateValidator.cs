using LuxSalon.Model.Requests;
using FluentValidation;

namespace LuxSalon.Services.Validators
{
    public class NotifikacijaUpdateValidator : AbstractValidator<NotifikacijaUpdateRequest>
    {
        public NotifikacijaUpdateValidator()
        {
            // Nema posebnih pravila - Procitano je bool.
        }
    }
}
