using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class StatusDokumenta
{
    public int Id { get; set; }

    [Display(Name = "Status dokumenta")]
    [Required(ErrorMessage = "Potrebno je unijeti status dokumenta")]
    public string StatusDok { get; set; }

    public virtual ICollection<Dokument> Dokument { get; set; } = new List<Dokument>();
}
