using FluentValidation;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Klasa za validaciju evidencije
    /// </summary>
    public class EvidencijaValidator : AbstractValidator<EvidencijaRada>
    {
        public EvidencijaValidator()
        {
            RuleFor(d => d.BrojSati)
            .NotEmpty()
            .WithMessage("Broj sati je obvezno polje");

            RuleFor(d => d.OpisRada).NotEmpty()
            .WithMessage("Opis rada je obvezno polje");

            RuleFor(d => d.ZadatakId)
            .NotEmpty()
            .WithMessage("Zadatak je obvezno polje");
        
            RuleFor(d => d.VrstaRadaId)
            .NotEmpty()
            .WithMessage("Vrsta rada je obvezno polje");

            RuleFor(d => d.SuradnikId)
            .NotEmpty()
            .WithMessage("Suradnik je obvezno polje");

        }

    }
}

