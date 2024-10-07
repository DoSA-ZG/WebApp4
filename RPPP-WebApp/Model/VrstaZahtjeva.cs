using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class VrstaZahtjeva
{
    public int Id { get; set; }

    public string NazivVrsteZahtjeva { get; set; }

    public virtual ICollection<ProjektniZahtjev> ProjektniZahtjev { get; set; } = new List<ProjektniZahtjev>();
}
