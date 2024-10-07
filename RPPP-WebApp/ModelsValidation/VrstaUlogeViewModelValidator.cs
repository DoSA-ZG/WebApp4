using FluentValidation;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.ModelsValidation;

public class VrstaUlogeViewModelValidator : AbstractValidator<VrstaUlogeViewModel>
{
    public VrstaUlogeViewModelValidator()
    {
        RuleFor(d => d.Vrsta)
            .NotEmpty().WithMessage("Naziv vrste je obavezno polje")
            .MaximumLength(50).WithMessage("Naziv vrste ne može imati više od 50 znakova");
    }
}