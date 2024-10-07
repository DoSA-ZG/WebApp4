using FluentValidation;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    public class KarticaProjektaValidator : AbstractValidator<KarticaProjekta>
    {
        public KarticaProjektaValidator()
        {
            RuleFor(d => d.ProjektId)
            .NotEmpty().WithMessage("Id projekta je obvezno polje");
        }
        
    }
}
