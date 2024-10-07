using FluentValidation;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Klasa za validaciju zadatka
    /// </summary>
    public class ZadatakValidator : AbstractValidator<Zadatak>
    {
        public ZadatakValidator()
        {
            RuleFor(d => d.OpisZadatak)
            .NotEmpty().WithMessage("Opis zadatka je obvezno polje")
            .MaximumLength(255);

            RuleFor(d => d.PlanPocetak).NotEmpty()
            .WithMessage("Planirani početak je obvezno polje");

            RuleFor(d => d.PlanKraj).NotEmpty()
            .WithMessage("Planirani kraj je obvezno polje")
            .GreaterThan(d => d.PlanPocetak).WithMessage("Planirani početak mora biti prije planiranog kraja");

            RuleFor(d => d.StvarniKraj)
            .GreaterThan(d => d.StvarniPocetak)
            .WithMessage("Stvarni početak mora biti prije stvarnog kraja");

            RuleFor(d => d.SuradnikId)
            .NotEmpty()
            .WithMessage("Suradnik je obvezno polje");

            RuleFor(d => d.PrioritetZadatkaId)
            .NotEmpty()
            .WithMessage("Prioritet je obvezno polje");

            RuleFor(d => d.StatusZadatkaId)
            .NotEmpty()
            .WithMessage("Status zadatka je obvezno polje");

            RuleFor(d => d.ProjektniZahtjevId)
            .NotEmpty()
            .WithMessage("Projektni zahtjev je obvezno polje");
        }
        
    }
}
