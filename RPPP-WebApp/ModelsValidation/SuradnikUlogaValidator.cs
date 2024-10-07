using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation;

public class SuradnikUlogaValidator : AbstractValidator<SuradnikUloga>
{
    public SuradnikUlogaValidator()
    {
        RuleFor(d => d.DatumPocetak)
            .NotEmpty().WithMessage("Datum početka je obavezno polje");
            
        RuleFor(d => d.DatumKraj)
            .NotEmpty().WithMessage("Datum kraja je obavezno polje");

        RuleFor(d => d.ProjektId)
            .NotEmpty().WithMessage("Projekt je obavezno polje");

        RuleFor(d => d.SuradnikId)
            .NotEmpty().WithMessage("Suradnik je obavezno polje");

        RuleFor(d => d.VrstaUlogeId)
            .NotEmpty().WithMessage("Vrsta uloge je obavezno polje");

    }
}