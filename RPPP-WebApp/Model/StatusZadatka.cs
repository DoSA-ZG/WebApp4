using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class StatusZadatka
{
    public int Id { get; set; }

    public string Status { get; set; }

    public virtual ICollection<Zadatak> Zadatak { get; set; } = new List<Zadatak>();
}
