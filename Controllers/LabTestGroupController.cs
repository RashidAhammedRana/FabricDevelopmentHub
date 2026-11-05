using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;

public class LabTestGroupController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabTestGroupController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LabTestGroupInfoList(int page = 1, string searchString = "")
    {
        int pageSize = 15;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var menuId = await _context.TblMenu
            .Where(x => x.MenuName == "Lab Test Group")
            .Select(x => x.MenuId)
            .FirstOrDefaultAsync();

        var userPermissions = await (
            from up in _context.TblUserPermission
            join pa in _context.TblPermissionAction
                on up.ActionId equals pa.ActionId
            where up.UserId == userId
                  && up.MenuId == menuId
                  && up.IsAllowed
            select pa.ActionName
        ).ToListAsync();

        ViewBag.CanView = userPermissions.Contains("View");
        ViewBag.CanCreate = userPermissions.Contains("Create");
        ViewBag.CanEdit = userPermissions.Contains("Edit");
        ViewBag.CanDelete = userPermissions.Contains("Delete");

        var query = _context.TblLabTestGroups
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();

            query = query.Where(x =>
                x.GroupName.Contains(searchString) ||
                x.Status.Contains(searchString) ||
                x.Remarks.Contains(searchString)
            );
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(x => x.GroupName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        ViewBag.SearchString = searchString;
        ViewBag.totalGroups = totalRecords;
        return View(data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblLabTestGroup labTestGroup)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields.";
                return View(labTestGroup);
            }

            // ✅ CHECK DUPLICATE SL NO
            var isExist = await _context.TblLabTestGroups
                .AnyAsync(x => x.GroupName == labTestGroup.GroupName);

            if (isExist)
            {
                ModelState.AddModelError("GroupName", "Group already exists!");
                return View(labTestGroup);
            }

            labTestGroup.Date = DateOnly.FromDateTime(DateTime.Now);
            _context.TblLabTestGroups.Add(labTestGroup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lab Test Group Created successfully.";

            return RedirectToAction(nameof(LabTestGroupInfoList));
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to create Lab Test Group.";
            return View(labTestGroup);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var groupInfo = await _context.TblLabTestGroups.FindAsync(id);
        if (groupInfo == null)
        {
            return NotFound();
        }
        return View(groupInfo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblLabTestGroup labTestGroup)
    {
        if (!ModelState.IsValid)
            return View(labTestGroup);

        var data = await _context.TblLabTestGroups
            .FirstOrDefaultAsync(x => x.Ltgid == labTestGroup.Ltgid);

        if (data == null)
        {
            TempData["ErrorMessage"] = "Lab Test Group not found.";
            return RedirectToAction(nameof(LabTestGroupInfoList));
        }

        data.GroupName = labTestGroup.GroupName;
        data.Status = labTestGroup.Status;
        data.Remarks = labTestGroup.Remarks;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lab Test Group Updated successfully.";

        return RedirectToAction(nameof(LabTestGroupInfoList));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var data = await _context.TblLabTestGroups.FindAsync(id);

        if (data == null)
            return NotFound();

        return View(data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var data = await _context.TblLabTestGroups
            .FirstOrDefaultAsync(x => x.Ltgid == id);

        if (data == null)
        {
            TempData["ErrorMessage"] = "Lab Test Group not found.";
            return RedirectToAction(nameof(LabTestGroupInfoList));
        }

        _context.TblLabTestGroups.Remove(data);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lab Test Group deleted successfully.";

        return RedirectToAction(nameof(LabTestGroupInfoList));
    }

}



