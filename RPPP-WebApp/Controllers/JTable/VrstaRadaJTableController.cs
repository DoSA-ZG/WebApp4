using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ViewModels.JTable;

namespace RPPP_WebApp.Controllers.JTable;

[Route("jtable/vrsta-rada/[action]")]
public class VrstaRadaJTableController : JTableController<VrstaRadaApiController, int, VrstaRadaViewModel>
{
    public VrstaRadaJTableController(VrstaRadaApiController controller) : base(controller)
    {
    }

    [HttpPost]
    public async Task<JTableAjaxResult> Update([FromForm] VrstaRadaViewModel model)
    {
        return await base.UpdateItem(model.Id, model);
    }

    [HttpPost]
    public async Task<JTableAjaxResult> Delete([FromForm] int Id)
    {
        return await base.DeleteItem(Id);
    }
}