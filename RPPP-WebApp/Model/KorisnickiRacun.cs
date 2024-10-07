using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class KorisnickiRacun
{
    public int Id { get; set; }

    public int StupanjPrava { get; set; }

    public virtual ICollection<Suradnik> Suradnik { get; set; } = new List<Suradnik>();
}
