using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Suradnik
{
    public int Id { get; set; }

    public string Organizacija { get; set; }

    public string Email { get; set; }

    public string Ime { get; set; }

    public string Prezime { get; set; }

    public string BrojTelefona { get; set; }

    public int? KorisnickiRacunId { get; set; }

    public int VrstaSuradnikaId { get; set; }

    public virtual ICollection<EvidencijaRada> EvidencijaRada { get; set; } = new List<EvidencijaRada>();

    public virtual KorisnickiRacun KorisnickiRacun { get; set; }

    public virtual ICollection<SuradnikUloga> SuradnikUloga { get; set; } = new List<SuradnikUloga>();

    public virtual VrstaSuradnika VrstaSuradnika { get; set; }

    public virtual ICollection<Zadatak> Zadatak { get; set; } = new List<Zadatak>();
}
