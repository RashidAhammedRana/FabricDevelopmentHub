
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;

public class LabTestParameterController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabTestParameterController(ApplicationDbContext context)
    {
        _context = context;
    }

    // CREATE GET
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadGroupList();
        return View();
    }

    // LOAD GROUP DROPDOWN
    private async Task LoadGroupList()
    {
        ViewBag.GroupList = new SelectList(
            await _context.TblLabTestGroups
                .Where(x => x.Status == "Active")
                .OrderBy(x => x.GroupName)
                .ToListAsync(),
            "Ltgid",
            "GroupName"
        );
    }

    // GET TEST NAME BY GROUP
    [HttpGet]
    public async Task<IActionResult> GetTestNamesByGroup(int lTgid)
    {
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

    // GET METHOD BY TEST NAME
    [HttpGet]
    public async Task<IActionResult> GetMethodsByTestName(int ltnid)
    {
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

    // CREATE POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int Ltgid,
        TblLabTestParameter model)
    {
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

        // VERIFY METHOD BELONGS TO TEST NAME
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
                "This Parameter already exists for this Method."
            );

            return View(model);
        }

        // SAVE
        _context.TblLabTestParameters.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Lab Test Parameter created successfully.";

        return RedirectToAction(nameof(Create));
    }

    [HttpGet]
    public async Task<IActionResult> GetNextDisplayOrder(int ltnid)
    {
        var lastOrder = await _context.TblLabTestParameters
            .Where(x => x.Ltnid == ltnid)
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync();

        return Json(new
        {
            nextOrder = (lastOrder ?? 0) + 1
        });
    }
}
