using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class PrioritetZahtjeva
{
    public int Id { get; set; }

    public string NazivPrioritetaZahtjeva { get; set; }

    public int StupanjPrioriteta { get; set; }

    public virtual ICollection<ProjektniZahtjev> ProjektniZahtjev { get; set; } = new List<ProjektniZahtjev>();
}
