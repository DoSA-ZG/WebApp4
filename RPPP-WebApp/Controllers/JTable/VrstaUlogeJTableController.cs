using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ViewModels.JTable;

namespace RPPP_WebApp.Controllers.JTable;

[Route("jtable/vrsta-uloge/[action]")]
public class VrstaUlogeJTableController : JTableController<VrstaUlogeApiController, int, VrstaUlogeViewModel>
{
    public VrstaUlogeJTableController(VrstaUlogeApiController controller) : base(controller)
    {
    }

    [HttpPost]
    public async Task<JTableAjaxResult> Update([FromForm] VrstaUlogeViewModel model)
    {
        return await base.UpdateItem(model.Id, model);
    }

    [HttpPost]
    public async Task<JTableAjaxResult> Delete([FromForm] int Id)
    {
        return await base.DeleteItem(Id);
    }
}