using FluentValidation;
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ModelsValidation
{
    public class TransakcijaValidator : AbstractValidator<Transakcija>
    {
        public TransakcijaValidator()
        {
            RuleFor(d => d.Iznos)
            .NotEmpty().WithMessage("Iznos transakcije je obvezno polje");

            RuleFor(d => d.Iban).NotEmpty()
            .WithMessage("Iban je obvezno polje");

            RuleFor(d => d.DatumVrijeme).NotEmpty()
            .WithMessage("Datum i Vrijeme je obvezno polje");

            RuleFor(d => d.KarticaProjektaId)
            .NotEmpty()
            .WithMessage("Id prve kartice projekta je obavezno polje");

            RuleFor(d => d.KarticaProjektaId1)
            .NotEmpty()
            .WithMessage("Id druge kartice projekta je obvezno polje");
        
            RuleFor(d => d.VrstaTransakcije)
            .NotEmpty()
            .WithMessage("Vrsta transakcije je obvezno polje");
        }
        
    }
}
