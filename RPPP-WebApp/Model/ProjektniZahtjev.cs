using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ProjektniZahtjev
{
    public int Id { get; set; }

    public string NazivZahtjeva { get; set; }

    public string OpisZahtjeva { get; set; }

    public int PrioritetZahtjevaId { get; set; }

    public int VrstaZahtjevaId { get; set; }

    public int ProjektId { get; set; }

    public virtual PrioritetZahtjeva PrioritetZahtjeva { get; set; }

    public virtual Projekt Projekt { get; set; }

    public virtual VrstaZahtjeva VrstaZahtjeva { get; set; }

    public virtual ICollection<Zadatak> Zadatak { get; set; } = new List<Zadatak>();
}
