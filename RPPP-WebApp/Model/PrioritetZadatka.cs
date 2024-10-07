using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class PrioritetZadatka
{
    public string NazivPrioriteta { get; set; }

    public int StupanjPrioriteta { get; set; }

    public int Id { get; set; }

    public virtual ICollection<Zadatak> Zadatak { get; set; } = new List<Zadatak>();
}
