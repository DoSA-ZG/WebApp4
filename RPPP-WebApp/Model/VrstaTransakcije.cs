using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class VrstaTransakcije
{
    public string Vrsta { get; set; }

    public int Id { get; set; }

    public virtual ICollection<Transakcija> Transakcija { get; set; } = new List<Transakcija>();
}
