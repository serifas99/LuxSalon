using eCommerce.Model.Requests;
using FluentValidation;

namespace eCommerce.Services.Validators
{
    public class NotifikacijaUpdateValidator : AbstractValidator<NotifikacijaUpdateRequest>
    {
        public NotifikacijaUpdateValidator()
        {
            // Nema posebnih pravila - Procitano je bool.
        }
    }
}
