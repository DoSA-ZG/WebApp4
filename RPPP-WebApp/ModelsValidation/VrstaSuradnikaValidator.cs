using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation;

public class VrstaSuradnikaValidator : AbstractValidator<VrstaSuradnika>
{
    public VrstaSuradnikaValidator()
    {
        RuleFor(d => d.Vrsta)
            .NotEmpty().WithMessage("Naziv vrste je obavezno polje")
            .MaximumLength(30).WithMessage("Naziv vrste ne može imati više od 30 znakova");
    }
}