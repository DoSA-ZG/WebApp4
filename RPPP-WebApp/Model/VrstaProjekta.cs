using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class VrstaProjekta
{
    public int Id { get; set; }

    [Display(Name = "Vrsta projekta")]
    [Required(ErrorMessage = "Potrebno je unijeti vrstu projekta")]
    public string Vrsta { get; set; }

    public virtual ICollection<Projekt> Projekt { get; set; } = new List<Projekt>();
}
