using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class VrstaUloge
{
    public int Id { get; set; }

    public string Vrsta { get; set; }

    public virtual ICollection<SuradnikUloga> SuradnikUloga { get; set; } = new List<SuradnikUloga>();
}
