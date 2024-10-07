using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Zadatak
{
    public int Id { get; set; }
    [DataType(DataType.Date)]
    public DateTime PlanPocetak { get; set; }
    [DataType(DataType.Date)]
    public DateTime PlanKraj { get; set; }
    [DataType(DataType.Date)]
    public DateTime? StvarniPocetak { get; set; }
    [DataType(DataType.Date)]
    public DateTime? StvarniKraj { get; set; }

    public string OpisZadatak { get; set; }

    public int PrioritetZadatkaId { get; set; }

    public int StatusZadatkaId { get; set; }

    public int ProjektniZahtjevId { get; set; }

    public int SuradnikId { get; set; }

    public virtual ICollection<EvidencijaRada> EvidencijaRada { get; set; } = new List<EvidencijaRada>();

    public virtual PrioritetZadatka PrioritetZadatka { get; set; }

    public virtual ProjektniZahtjev ProjektniZahtjev { get; set; }

    public virtual StatusZadatka StatusZadatka { get; set; }

    public virtual Suradnik Suradnik { get; set; }
}
