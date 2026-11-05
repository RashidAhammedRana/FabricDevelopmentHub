using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;

public class LabTestMethodController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabTestMethodController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LabTestMethodList(int page = 1, string searchString = "")
    {
        int pageSize = 15;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var menuId = await _context.TblMenu
            .Where(x => x.MenuName == "Lab Test Method")
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

        var query = _context.TblLabTestMethods.Include(x => x.Ltn)
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();

            query = query.Where(x =>
                x.MethodName.Contains(searchString) ||
                x.Status.Contains(searchString) ||
                x.Remarks.Contains(searchString)
            );
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(x => x.MethodName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        ViewBag.SearchString = searchString;
        ViewBag.totalMethods = totalRecords;
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadTestNameList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblLabTestMethod labTestMethod)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields.";
                await LoadTestNameList(labTestMethod.Ltmid);
                return View(labTestMethod);
            }

            // Duplicate Test Name Check
            var isExist = await _context.TblLabTestMethods
                .AnyAsync(x => x.MethodName == labTestMethod.MethodName);

            if (isExist)
            {
                ModelState.AddModelError(
                    "MethodName",
                    "Method Name already exists!"
                );

                await LoadTestNameList(labTestMethod.Ltmid);
                return View(labTestMethod);
            }

            labTestMethod.Date = DateOnly.FromDateTime(DateTime.Now);

            _context.TblLabTestMethods.Add(labTestMethod);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Method Created successfully.";

            return RedirectToAction(nameof(LabTestMethodList));
        }
        catch (Exception)
        {
            await LoadTestNameList(labTestMethod.Ltmid);

            TempData["ErrorMessage"] = "Failed to create Method.";

            return View(labTestMethod);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var testmethodInfo = await _context.TblLabTestMethods
            .FirstOrDefaultAsync(x => x.Ltmid == id);

        if (testmethodInfo == null)
        {
            return NotFound();
        }

        // Load Method Dropdown with selected Test Name ID
        await LoadTestNameList(testmethodInfo.Ltmid);

        return View(testmethodInfo);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblLabTestMethod labTestMethod)
    {
        if (!ModelState.IsValid)
        {
            await LoadTestNameList(labTestMethod.Ltmid);
            return View(labTestMethod);
        }

        var data = await _context.TblLabTestMethods
            .FirstOrDefaultAsync(x => x.Ltnid == labTestMethod.Ltmid);

        if (data == null)
        {
            TempData["ErrorMessage"] = "Test Method not found.";
            return RedirectToAction(nameof(LabTestMethodList));
        }

        // Update Information
        data.Ltnid = labTestMethod.Ltnid;
        data.MethodName = labTestMethod.MethodName;
        data.Status = labTestMethod.Status;
        data.Remarks = labTestMethod.Remarks;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Method Updated successfully.";

        return RedirectToAction(nameof(LabTestMethodList));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var data = await _context.TblLabTestMethods.FindAsync(id);

        if (data == null)
            return NotFound();

        return View(data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var data = await _context.TblLabTestMethods
            .FirstOrDefaultAsync(x => x.Ltmid == id);

        if (data == null)
        {
            TempData["ErrorMessage"] = "Method not found.";
            return RedirectToAction(nameof(LabTestMethodList));
        }

        _context.TblLabTestMethods.Remove(data);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Method deleted successfully.";

        return RedirectToAction(nameof(LabTestMethodList));
    }

    private async Task LoadTestNameList(int? selectedId = null)
    {
        ViewBag.TestNameList = new SelectList(
            await _context.TblLabTestNames
                .Where(x => x.Status == "Active")
                .OrderBy(x => x.TestName)
                .ToListAsync(),
            "Ltnid",
            "TestName",
            selectedId
        );
    }
}




