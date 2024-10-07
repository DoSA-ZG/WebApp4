using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation;

public class VrstaUlogeValidator : AbstractValidator<VrstaUloge>
{
    public VrstaUlogeValidator()
    {
        RuleFor(d => d.Vrsta)
            .NotEmpty().WithMessage("Naziv vrste je obavezno polje")
            .MaximumLength(50).WithMessage("Naziv vrste ne može imati više od 50 znakova");
        
    }
}