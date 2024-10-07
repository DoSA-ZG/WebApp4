
namespace RPPP_WebApp.ViewModels
{
    public class EvidencijaRadaViewModel
    {
        public int Id { get; set; }
        public int BrojSati { get; set; }
        public string OpisRada { get; set; }
        public string OpisZadatak { get; set; }
        public string VrstaRada { get; set; }
        public string Email { get; set; }

        
        public int? ZadatakId { get; set; }

        public int VrstaRadaId { get; set; }

        public int SuradnikId { get; set; }

    }
}
