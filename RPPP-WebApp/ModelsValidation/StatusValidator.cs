using FluentValidation;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    public class StatusValidator : AbstractValidator<StatusZadatka>
    {
        public StatusValidator()
        {
            RuleFor(d => d.Status)
            .NotEmpty().WithMessage("Status je obvezno polje")
            .MaximumLength(255);

           
        }

    }
}
