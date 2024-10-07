using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ViewModels.JTable;

namespace RPPP_WebApp.Controllers.JTable
{
    [Route("jtable/narucitelj/[action]")]
    public class NaruciteljJTableController : JTableController<NaruciteljController, int, NaruciteljViewModel>
    {
        public NaruciteljJTableController(NaruciteljController controller) : base(controller)
        {

        }

        [HttpPost]
        public async Task<JTableAjaxResult> Update([FromForm] NaruciteljViewModel model)
        {
            return await base.UpdateItem(model.Id, model);
        }

        [HttpPost]
        public async Task<JTableAjaxResult> Delete([FromForm] int Id)
        {
            return await base.DeleteItem(Id);
        }
    }
}
