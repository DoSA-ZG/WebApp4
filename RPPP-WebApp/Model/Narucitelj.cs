using RPPP_WebApp.ModelsValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace RPPP_WebApp.Model;

public partial class Narucitelj
{
    public int Id { get; set; }

    [Display(Name = "Naziv naručitelja")]
    [Required(ErrorMessage = "Potrebno je unijeti naziv")]
    public string NazivNarucitelj { get; set; }

    [Required(ErrorMessage = "Potrebno je unijeti OIB")]
    [Display(Name = "OIB naručitelja")]
    [RegularExpression("[0-9]{11}")]
    public string Oib { get; set; }

    [Display(Name = "IBAN naručitelja")]
    [Required(ErrorMessage = "Potrebno je unijeti IBAN")]
    [IBANValidator(ErrorMessage = "Pogrešan format IBAN-a. XX...")]
    public string Iban { get; set; }

    [Display(Name = "Adresa naručitelja")]
    [Required(ErrorMessage = "Potrebno je unijeti adresu")]
    public string Adresa { get; set; }

    [Display(Name = "E-mail naručitelja")]
    [Required(ErrorMessage = "Potrebno je unijeti e-mail")]
    [EMAILValidator(ErrorMessage = "Pogrešan format e-maila")]
    public string Email { get; set; }

    public virtual ICollection<Projekt> Projekt { get; set; } = new List<Projekt>();
}
