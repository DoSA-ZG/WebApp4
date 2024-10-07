using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class SuradnikUloga
{
    public int Id { get; set; }

    public DateTime DatumPocetak { get; set; }

    public DateTime DatumKraj { get; set; }

    public int ProjektId { get; set; }

    public int SuradnikId { get; set; }

    public int VrstaUlogeId { get; set; }

    public virtual Projekt Projekt { get; set; }

    public virtual Suradnik Suradnik { get; set; }

    public virtual VrstaUloge VrstaUloge { get; set; }
}
