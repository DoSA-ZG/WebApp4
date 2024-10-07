using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public partial class Projekt
{
    public int Id { get; set; }

    public string NazivProjekt { get; set; }

    public string KraticaProjekt { get; set; }

    public DateTime? DatumIsporukaPr { get; set; }

    public int? NaruciteljId { get; set; }

    public int? VrstaProjektaId { get; set; }

    public virtual ICollection<Dokument> Dokument { get; set; } = new List<Dokument>();

    public virtual ICollection<KarticaProjekta> KarticaProjekta { get; set; } = new List<KarticaProjekta>();

    public virtual Narucitelj Narucitelj { get; set; }

    public virtual ICollection<ProjektniZahtjev> ProjektniZahtjev { get; set; } = new List<ProjektniZahtjev>();

    public virtual ICollection<SuradnikUloga> SuradnikUloga { get; set; } = new List<SuradnikUloga>();

    public virtual VrstaProjekta VrstaProjekta { get; set; }
}