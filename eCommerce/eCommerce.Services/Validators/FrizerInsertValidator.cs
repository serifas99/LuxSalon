using eCommerce.Model.Requests;
using FluentValidation;

namespace eCommerce.Services.Validators
{
    public class FrizerInsertValidator : AbstractValidator<FrizerInsertRequest>
    {
        public FrizerInsertValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId je obavezan.");

            RuleFor(x => x.Specijalizacija)
                .MaximumLength(200).WithMessage("Specijalizacija ne smije biti duza od 200 karaktera.");

            RuleFor(x => x.Biografija)
                .MaximumLength(1000).WithMessage("Biografija ne smije biti duza od 1000 karaktera.");
        }
    }
}
