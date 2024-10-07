using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class VrstaDokumenta
{
    public int Id { get; set; }

    [Display(Name = "Vrsta dokumenta")]
    [Required(ErrorMessage = "Potrebno je unijeti vrstu dokumenta")]
    public string VrstaDok { get; set; }

    public virtual ICollection<Dokument> Dokument { get; set; } = new List<Dokument>();
}
