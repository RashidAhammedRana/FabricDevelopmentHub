
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;

public class LabTestParameterController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabTestParameterController(ApplicationDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------
    // CHECK USER PERMISSION
    // -------------------------------------------------

    private async Task<bool> HasPermission(params string[] actionNames)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return false;

        var menuId = await _context.TblMenu
            .Where(x => x.MenuName == "Lab Test Parameter")
            .Select(x => x.MenuId)
            .FirstOrDefaultAsync();

        if (menuId == 0)
            return false;

        return await (
            from up in _context.TblUserPermission
            join pa in _context.TblPermissionAction
                on up.ActionId equals pa.ActionId
            where up.UserId == userId
                  && up.MenuId == menuId
                  && up.IsAllowed
                  && actionNames.Contains(pa.ActionName)
            select up
        ).AnyAsync();
    }

    // -------------------------------------------------
    // LOAD USER PERMISSIONS FOR VIEW
    // -------------------------------------------------

    private async Task LoadPermissions()
    {
        ViewBag.CanView = await HasPermission("View");
        ViewBag.CanCreate = await HasPermission("Create");
        ViewBag.CanEdit = await HasPermission("Edit");
        ViewBag.CanDelete = await HasPermission("Delete");
    }

    // -------------------------------------------------
    // LOAD GROUP DROPDOWN
    // -------------------------------------------------

    private async Task LoadGroupList(int? selectedGroupId = null)
    {
        var groups = await _context.TblLabTestGroups
            .Where(x => x.Status == "Active")
            .OrderBy(x => x.GroupName)
            .ToListAsync();

        ViewBag.GroupList = new SelectList(
            groups,
            "Ltgid",
            "GroupName",
            selectedGroupId
        );
    }

    // -------------------------------------------------
    // PARAMETER LIST
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> LabTestParameterList(
        int page = 1,
        string searchString = "")
    {
        if (!await HasPermission("View"))
            return Forbid();

        await LoadPermissions();

        int pageSize = 15;

        var query = _context.TblLabTestParameters
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();

            query = query.Where(x =>
                x.ParameterGroup.Contains(searchString) ||
                x.ParameterName.Contains(searchString) ||
                x.Status.Contains(searchString)
            );
        }

        // TOTAL PARAMETERS
        int totalRecords = await query.CountAsync();

        // TOTAL PAGES
        int totalPages = (int)Math.Ceiling(
            totalRecords / (double)pageSize
        );

        // VALIDATE PAGE
        page = Math.Max(1, page);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        // GET PAGED DATA
        var parameters = await query
            .Include(x => x.TblLabTestResults)
            .OrderBy(x => x.Ltnid)
            .ThenBy(x => x.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // VIEWBAG
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.SearchString = searchString;
        ViewBag.TotalParameters = totalRecords;

        return View(parameters);
    }

    // -------------------------------------------------
    // GET TEST NAME BY GROUP
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> GetTestNamesByGroup(int lTgid)
    {
        if (!await HasPermission("Create", "Edit"))
            return Forbid();

        var data = await _context.TblLabTestNames
            .Where(x =>
                x.Ltgid == lTgid &&
                x.Status == "Active")
            .OrderBy(x => x.TestName)
            .Select(x => new
            {
                ltnid = x.Ltnid,
                testName = x.TestName
            })
            .ToListAsync();

        return Json(data);
    }

    // -------------------------------------------------
    // GET METHOD BY TEST NAME
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> GetMethodsByTestName(int ltnid)
    {
        if (!await HasPermission("Create", "Edit"))
            return Forbid();

        var data = await _context.TblLabTestMethods
            .Where(x =>
                x.Ltnid == ltnid &&
                x.Status == "Active")
            .OrderBy(x => x.MethodName)
            .Select(x => new
            {
                ltmid = x.Ltmid,
                methodName = x.MethodName
            })
            .ToListAsync();

        return Json(data);
    }

    // -------------------------------------------------
    // GET NEXT DISPLAY ORDER
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> GetNextDisplayOrder(int ltnid)
    {
        if (!await HasPermission("Create", "Edit"))
            return Forbid();

        var lastOrder = await _context.TblLabTestParameters
            .Where(x => x.Ltnid == ltnid)
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync();

        return Json(new
        {
            nextOrder = (lastOrder ?? 0) + 1
        });
    }

    // -------------------------------------------------
    // CREATE GET
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await HasPermission("Create"))
            return Forbid();

        await LoadGroupList();

        return View();
    }

    // -------------------------------------------------
    // CREATE POST
    // -------------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int Ltgid,
        TblLabTestParameter model)
    {
        if (!await HasPermission("Create"))
            return Forbid();

        await LoadGroupList();

        if (!ModelState.IsValid)
            return View(model);

        // VERIFY TEST NAME AND GROUP

        var validTest = await _context.TblLabTestNames
            .AnyAsync(x =>
                x.Ltnid == model.Ltnid &&
                x.Ltgid == Ltgid &&
                x.Status == "Active");

        if (!validTest)
        {
            ModelState.AddModelError(
                "",
                "Invalid Test Name selected."
            );

            return View(model);
        }

        // VERIFY METHOD

        var validMethod = await _context.TblLabTestMethods
            .AnyAsync(x =>
                x.Ltnid == model.Ltnid &&
                x.Status == "Active");

        if (!validMethod)
        {
            ModelState.AddModelError(
                "",
                "Invalid Method selected."
            );

            return View(model);
        }

        // VERIFY PARAMETER GROUP

        if (string.IsNullOrWhiteSpace(model.ParameterGroup))
        {
            ModelState.AddModelError(
                "ParameterGroup",
                "Parameter Group is required."
            );

            return View(model);
        }

        // VERIFY PARAMETER NAME

        if (string.IsNullOrWhiteSpace(model.ParameterName))
        {
            ModelState.AddModelError(
                "ParameterName",
                "Parameter Name is required."
            );

            return View(model);
        }

        // DUPLICATE CHECK

        var isExist = await _context.TblLabTestParameters
            .AnyAsync(x =>
                x.Ltnid == model.Ltnid &&
                x.ParameterGroup == model.ParameterGroup &&
                x.ParameterName == model.ParameterName);

        if (isExist)
        {
            ModelState.AddModelError(
                "ParameterName",
                "This Parameter already exists for this Test."
            );

            return View(model);
        }

        // SAVE

        _context.TblLabTestParameters.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Lab Test Parameter created successfully.";

        return RedirectToAction(nameof(LabTestParameterList));
    }

    // -------------------------------------------------
    // EDIT GET
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await HasPermission("Edit"))
            return Forbid();

        var parameter = await _context.TblLabTestParameters
            .FirstOrDefaultAsync(x => x.ParameterId == id);

        if (parameter == null)
            return NotFound();

        var groupId = await _context.TblLabTestNames
            .Where(x => x.Ltnid == parameter.Ltnid)
            .Select(x => x.Ltgid)
            .FirstOrDefaultAsync();

        ViewBag.SelectedGroupId = groupId;

        await LoadGroupList(groupId);

        return View(parameter);
    }

    // -------------------------------------------------
    // EDIT POST
    // -------------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        int Ltgid,
        TblLabTestParameter model)
    {
        if (!await HasPermission("Edit"))
            return Forbid();

        if (id != model.ParameterId)
            return BadRequest();

        var existing = await _context.TblLabTestParameters
            .FirstOrDefaultAsync(x => x.ParameterId == id);

        if (existing == null)
            return NotFound();

        // VERIFY TEST NAME AND GROUP

        var validTest = await _context.TblLabTestNames
            .AnyAsync(x =>
                x.Ltnid == model.Ltnid &&
                x.Ltgid == Ltgid &&
                x.Status == "Active");

        if (!validTest)
        {
            ModelState.AddModelError(
                "",
                "Invalid Test Name selected."
            );
        }

        // VERIFY METHOD

        var validMethod = await _context.TblLabTestMethods
            .AnyAsync(x =>
                x.Ltnid == model.Ltnid &&
                x.Status == "Active");

        if (!validMethod)
        {
            ModelState.AddModelError(
                "",
                "Invalid Method selected."
            );
        }

        // VERIFY PARAMETER GROUP

        if (string.IsNullOrWhiteSpace(model.ParameterGroup))
        {
            ModelState.AddModelError(
                "ParameterGroup",
                "Parameter Group is required."
            );
        }

        // VERIFY PARAMETER NAME

        if (string.IsNullOrWhiteSpace(model.ParameterName))
        {
            ModelState.AddModelError(
                "ParameterName",
                "Parameter Name is required."
            );
        }

        // DUPLICATE CHECK

        var isExist = await _context.TblLabTestParameters
            .AnyAsync(x =>
                x.ParameterId != id &&
                x.Ltnid == model.Ltnid &&
                x.ParameterGroup == model.ParameterGroup &&
                x.ParameterName == model.ParameterName);

        if (isExist)
        {
            ModelState.AddModelError(
                "ParameterName",
                "This Parameter already exists for this Test."
            );
        }

        // VALIDATION FAILED

        if (!ModelState.IsValid)
        {
            ViewBag.SelectedGroupId = Ltgid;

            await LoadGroupList(Ltgid);

            return View(model);
        }

        // UPDATE

        existing.Ltnid = model.Ltnid;
        existing.ParameterGroup = model.ParameterGroup;
        existing.ParameterName = model.ParameterName;
        existing.DisplayOrder = model.DisplayOrder;
        existing.Status = model.Status;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Lab Test Parameter updated successfully.";

        return RedirectToAction(nameof(LabTestParameterList));
    }

    // -------------------------------------------------
    // DELETE GET
    // -------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await HasPermission("Delete"))
            return Forbid();

        var parameter = await _context.TblLabTestParameters
            .FirstOrDefaultAsync(x => x.ParameterId == id);

        if (parameter == null)
            return NotFound();

        return View(parameter);
    }

    // -------------------------------------------------
    // DELETE POST
    // -------------------------------------------------

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await HasPermission("Delete"))
            return Forbid();

        var parameter = await _context.TblLabTestParameters
            .FirstOrDefaultAsync(x => x.ParameterId == id);

        if (parameter == null)
        {
            TempData["ErrorMessage"] =
                "Lab Test Parameter not found.";

            return RedirectToAction(nameof(LabTestParameterList));
        }

        try
        {
            _context.TblLabTestParameters.Remove(parameter);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Lab Test Parameter deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                "Unable to delete parameter. It may be linked with existing test results.";

        }

        return RedirectToAction(nameof(LabTestParameterList));
    }
}
