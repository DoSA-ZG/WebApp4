using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ProjektniDokumenti
{
    public int Id { get; set; }

    public int ProjektId { get; set; }

    public int DokumentId { get; set; }

    public virtual Dokument Dokument { get; set; }

    public virtual Projekt Projekt { get; set; }
}
