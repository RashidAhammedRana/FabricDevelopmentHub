
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.ViewModels;

namespace FabricDevelopmentHub.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var data = await (
                from up in _context.TblUserPermission
                where up.UserId == userId

                join m in _context.TblModule on up.ModuleId equals m.ModuleId
                join me in _context.TblMenu on up.MenuId equals me.MenuId

                select new SidebarViewModel
                {
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    MenuId = me.MenuId,
                    MenuName = me.MenuName,
                    Url = me.Url
                }
            )
            .Distinct()
            .ToListAsync();

            return View(data);
        }
    }
}