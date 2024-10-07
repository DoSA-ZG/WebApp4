using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation;

public class SuradnikValidator : AbstractValidator<Suradnik>
{
    public SuradnikValidator()
    {
        RuleFor(d => d.Ime)
            .NotEmpty().WithMessage("Ime je obavezno polje");
            
        RuleFor(d => d.Prezime)
            .NotEmpty().WithMessage("Prezime je obavezno polje");

        RuleFor(d => d.BrojTelefona)
            .NotEmpty().WithMessage("Broj telefona je obavezno polje")
            .Matches(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$")
            .WithMessage("Broj telefona nije u valjanom formatu")
            .MaximumLength(20).WithMessage("Broj telefona ne može imati toliko znamenki");

        RuleFor(d => d.Email)
            .NotEmpty().WithMessage("Email je obavezno polje")
            .MaximumLength(255).WithMessage("Email adresa ne može imati toliko znakova")
            .EmailAddress().WithMessage("Email adresa nije valjana");

        RuleFor(d => d.Organizacija)
            .NotEmpty().WithMessage("Organizacija je obavezno polje")
            .MaximumLength(50).WithMessage("Ime organizacije ne može imati više od 50 znakova");

        RuleFor(d => d.VrstaSuradnikaId)
            .NotEmpty().WithMessage("Vrsta suradnika je obavezno polje");
    }
}