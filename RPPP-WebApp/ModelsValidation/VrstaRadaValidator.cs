using FluentValidation;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    public class VrstaRadaValidator : AbstractValidator<VrstaRada>
    {
        public VrstaRadaValidator()
        {
            RuleFor(d => d.VrstaRada1)
            .NotEmpty().WithMessage("Vrsta rada je obvezno polje")
            .MaximumLength(255);


        }

    }
}
