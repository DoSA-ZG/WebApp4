using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Transakcija
{
    public int Id { get; set; }

    public int? Iznos { get; set; }

    public string Iban { get; set; }

    public DateTime? DatumVrijeme { get; set; }

    public int? KarticaProjektaId { get; set; }

    public int VrstaTransakcijeId { get; set; }

    public int? KarticaProjektaId1 { get; set; }

    public virtual KarticaProjekta KarticaProjekta { get; set; }

    public virtual KarticaProjekta KarticaProjektaId1Navigation { get; set; }

    public virtual VrstaTransakcije VrstaTransakcije { get; set; }
}
