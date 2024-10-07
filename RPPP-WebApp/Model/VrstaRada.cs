using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class VrstaRada
{
    public int Id { get; set; }

    public string VrstaRada1 { get; set; }

    public virtual ICollection<EvidencijaRada> EvidencijaRada { get; set; } = new List<EvidencijaRada>();
}
