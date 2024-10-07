using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ModelsPartial;

public class SuradnikUlogaDenorm {
    
    // za suradnika
    
    public int SuradnikId { get; set; }
    
    public string Organizacija { get; set; }

    public string Email { get; set; }

    public string Ime { get; set; }

    public string Prezime { get; set; }

    public string BrojTelefona { get; set; }
    
    public string VrstaSuradnika { get; set; }
    
    public int? StupanjPrava { get; set; }


    // za suradnik ulogu

    [DataType(DataType.Date)]
    public DateTime DatumPocetak { get; set; }

    [DataType(DataType.Date)]
    public DateTime DatumKraj { get; set; }
    
    public string NazivProjekt { get; set; }
    
    public string VrstaUloge { get; set; }

    
    [NotMapped]
    public string UrlSuradnika { get; set;}
}