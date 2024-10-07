using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class EvidencijaRada
{
    public int Id { get; set; }

    public int BrojSati { get; set; }

    public string OpisRada { get; set; }

    public int? ZadatakId { get; set; }

    public int VrstaRadaId { get; set; }

    public int SuradnikId { get; set; }

    public virtual Suradnik Suradnik { get; set; }

    public virtual VrstaRada VrstaRada { get; set; }

    public virtual Zadatak Zadatak { get; set; }
}
