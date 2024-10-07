using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers
{
  /// <summary>
  /// Kontroler za početnu stranicu
  /// </summary>
  public class HomeController : Controller
  {
    private readonly Rppp04Context ctx;
    private ILogger<HomeController> logger;

    public HomeController(Rppp04Context ctx, ILogger<HomeController> logger)
    {
      this.ctx = ctx;
      this.logger = logger;
    }
    
    public IActionResult Index()
    {
      return View();
    }
  }
}
