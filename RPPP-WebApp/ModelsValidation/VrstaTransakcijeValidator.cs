using FluentValidation;
using RPPP_WebApp.Controllers;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    public class VrstaTransakcijeValidator : AbstractValidator<VrstaTransakcije>
    {
        public VrstaTransakcijeValidator()
        {
            RuleFor(d => d.Vrsta)
            .NotEmpty().WithMessage("Vrsta transakcije je obvezno polje.")
            .Length(1).WithMessage("Vrsta transakcije mora biti duljine 1.")
            .Must(JedanOdPUI).WithMessage("Vrsta transakcije mora biti ili 'p' ili 'u' ili 'i'");
        }
        private bool JedanOdPUI(string value)
    {
        return value == "p" || value == "u" || value == "i";
    }
    }
}
