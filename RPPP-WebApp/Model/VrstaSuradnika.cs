using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class VrstaSuradnika
{
    public int Id { get; set; }

    public string Vrsta { get; set; }

    public virtual ICollection<Suradnik> Suradnik { get; set; } = new List<Suradnik>();
}
