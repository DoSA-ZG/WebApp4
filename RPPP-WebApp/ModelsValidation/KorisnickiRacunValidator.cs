using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation;

public class KorisnickiRacunValidator : AbstractValidator<KorisnickiRacun>
{
    public KorisnickiRacunValidator()
    {
        RuleFor(d => d.StupanjPrava)
            .NotEmpty().WithMessage("Stupanj prava je obavezno polje");
        
    }
}