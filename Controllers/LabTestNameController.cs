using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;

public class LabTestNameController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabTestNameController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LabTestNameList(int page = 1, string searchString = "")
    {
        int pageSize = 15;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var menuId = await _context.TblMenu
            .Where(x => x.MenuName == "Lab Test Name")
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

        var query = _context.TblLabTestNames.Include(x=>x.Ltg)
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();

            query = query.Where(x =>
                x.TestName.Contains(searchString) ||
                x.Status.Contains(searchString) ||
                x.Remarks.Contains(searchString)
            );
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(x => x.TestName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        ViewBag.SearchString = searchString;
        ViewBag.totalNames = totalRecords;
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadGroupList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblLabTestName labTestName)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields.";
                await LoadGroupList(labTestName.Ltgid);
                return View(labTestName);
            }

            // Duplicate Test Name Check
            var isExist = await _context.TblLabTestNames
                .AnyAsync(x => x.TestName == labTestName.TestName);

            if (isExist)
            {
                ModelState.AddModelError(
                    "TestName",
                    "Test Name already exists!"
                );

                await LoadGroupList(labTestName.Ltgid);
                return View(labTestName);
            }

            labTestName.Date = DateOnly.FromDateTime(DateTime.Now);

            _context.TblLabTestNames.Add(labTestName);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Test Name Created successfully.";

            return RedirectToAction(nameof(LabTestNameList));
        }
        catch (Exception)
        {
            await LoadGroupList(labTestName.Ltgid);

            TempData["ErrorMessage"] = "Failed to create Test Name.";

            return View(labTestName);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var testNameInfo = await _context.TblLabTestNames
            .FirstOrDefaultAsync(x => x.Ltnid == id);

        if (testNameInfo == null)
        {
            return NotFound();
        }

        // Load Group Dropdown with selected Group ID
        await LoadGroupList(testNameInfo.Ltgid);

        return View(testNameInfo);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblLabTestName labTestName)
    {
        if (!ModelState.IsValid)
        {
            await LoadGroupList(labTestName.Ltgid);
            return View(labTestName);
        }

        var data = await _context.TblLabTestNames
            .FirstOrDefaultAsync(x => x.Ltnid == labTestName.Ltnid);

        if (data == null)
        {
            TempData["ErrorMessage"] = "Test Name not found.";
            return RedirectToAction(nameof(LabTestNameList));
        }

        // Update Information
        data.Ltgid = labTestName.Ltgid;
        data.TestName = labTestName.TestName;
        data.Status = labTestName.Status;
        data.Remarks = labTestName.Remarks;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Test Name Updated successfully.";

        return RedirectToAction(nameof(LabTestNameList));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var data = await _context.TblLabTestNames.FindAsync(id);

        if (data == null)
            return NotFound();

        return View(data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var data = await _context.TblLabTestNames
            .FirstOrDefaultAsync(x => x.Ltnid == id);

        if (data == null)
        {
            TempData["ErrorMessage"] = "Test Name not found.";
            return RedirectToAction(nameof(LabTestNameList));
        }

        _context.TblLabTestNames.Remove(data);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Test Name deleted successfully.";

        return RedirectToAction(nameof(LabTestNameList));
    }

    private async Task LoadGroupList(int? selectedId = null)
    {
        ViewBag.GroupList = new SelectList(
            await _context.TblLabTestGroups
                .Where(x => x.Status == "Active")
                .OrderBy(x => x.GroupName)
                .ToListAsync(),
            "Ltgid",
            "GroupName",
            selectedId
        );
    }
}



