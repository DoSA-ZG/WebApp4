using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Dokument
{
    public int Id { get; set; }

    [Display(Name = "Naziv dokumenta")]
    [Required(ErrorMessage = "Potrebno je unijeti naziv dokumenta")]
    public string NazivDok { get; set; }

    [Display(Name = "Vrsta dokumenta")]
    public int? VrstaDokumentaId { get; set; }

    [Display(Name = "Status dokumenta")]
    public int? StatusDokumentaId { get; set; }

    [Display(Name = "Ekstenzija dokumenta")]

    public string EkstenzijaDokumenta { get; set; }

    [Display(Name = "Datum zadnje izmjene dokumenta")]

    public DateTime? DatumZadIzmj { get; set; }

    [Display(Name = "Datoteka")]
    public byte[] Datoteka { get; set; }

    [Display(Name = "Vrijeme prijenosa dokumenta")]

    public DateTime? VrPrijenos { get; set; }

    [Display(Name = "Projekt")]
    public int? ProjektId { get; set; }

    public virtual StatusDokumenta IdStatusDokNavigation { get; set; }

    public virtual VrstaDokumenta IdVrstaDokNavigation { get; set; }

    public virtual Projekt IdProjektNavigation { get; set; }

}