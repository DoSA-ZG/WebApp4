using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class KarticaProjekta
{
    public int? ProjektId { get; set; }

    public int Id { get; set; }

    public virtual Projekt Projekt { get; set; }

    public virtual ICollection<Transakcija> TransakcijaKarticaProjekta { get; set; } = new List<Transakcija>();

    public virtual ICollection<Transakcija> TransakcijaKarticaProjektaId1Navigation { get; set; } = new List<Transakcija>();
}
