
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;

public class LabTestResultController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabTestResultController(ApplicationDbContext context)
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

    // LOAD GROUP LIST
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
                methodId = x.Ltmid,
                methodName = x.MethodName
            })
            .ToListAsync();

        return Json(data);
    }

    // GET PARAMETERS BY TEST NAME AND METHOD
    [HttpGet]
    public async Task<IActionResult> GetParameters(int ltnid, int ltmid)
    {
        // VERIFY METHOD BELONGS TO TEST NAME
        var validMethod = await _context.TblLabTestMethods
            .AnyAsync(x =>
                x.Ltmid == ltmid &&
                x.Ltnid == ltnid &&
                x.Status == "Active");

        if (!validMethod)
            return BadRequest("Invalid Method.");

        var data = await _context.TblLabTestParameters
            .Where(x =>
                x.Ltnid == ltnid &&
                x.Status == "Active")
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new
            {
                parameterId = x.ParameterId,
                parameterGroup = x.ParameterGroup,
                parameterName = x.ParameterName,
                unit = x.Unit,
                normalRange = x.NormalRange,
                dataType = x.DataType
            })
            .ToListAsync();

        return Json(data);
    }

    // CREATE POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int Ltgid,
        int Ltnid,
        int Ltmid,
        string? Remarks,
        List<LabTestResultEntry> Results)
    {
        await LoadGroupList();

        // VALIDATE GROUP AND TEST NAME
        var validTest = await _context.TblLabTestNames
            .AnyAsync(x =>
                x.Ltnid == Ltnid &&
                x.Ltgid == Ltgid &&
                x.Status == "Active");

        if (!validTest)
        {
            ModelState.AddModelError(
                "",
                "Invalid Group or Test Name."
            );

            return View();
        }

        // VALIDATE METHOD
        var validMethod = await _context.TblLabTestMethods
            .AnyAsync(x =>
                x.Ltmid == Ltmid &&
                x.Ltnid == Ltnid &&
                x.Status == "Active");

        if (!validMethod)
        {
            ModelState.AddModelError(
                "",
                "Invalid Method selected."
            );

            return View();
        }

        // CHECK RESULTS
        if (Results == null || !Results.Any())
        {
            ModelState.AddModelError(
                "",
                "No result parameters found."
            );

            return View();
        }

        // CHECK DUPLICATE PARAMETER IDs
        var parameterIds = Results
            .Select(x => x.ParameterId)
            .ToList();

        if (parameterIds.Distinct().Count() != parameterIds.Count)
        {
            ModelState.AddModelError(
                "",
                "Duplicate parameters found."
            );

            return View();
        }

        // VERIFY PARAMETERS BELONG TO SELECTED TEST AND METHOD
        var validParameters = await _context.TblLabTestParameters
            .Where(x =>
                x.Ltnid == Ltnid &&
                x.Status == "Active" &&
                parameterIds.Contains(x.ParameterId))
            .Select(x => x.ParameterId)
            .ToListAsync();

        if (validParameters.Count != parameterIds.Count)
        {
            ModelState.AddModelError(
                "",
                "Invalid Test Parameters."
            );

            return View();
        }

        // ONLY ENTERED RESULTS WILL BE SAVED
        var resultList = Results
            .Where(x => !string.IsNullOrWhiteSpace(x.ResultValue))
            .Select(x => new TblLabTestResult
            {
                Ltnid = Ltnid,
                ParameterId = x.ParameterId,
                ResultValue = x.ResultValue!.Trim(),
                EntryDate = DateTime.Now,
                Remarks = Remarks
            })
            .ToList();

        if (!resultList.Any())
        {
            ModelState.AddModelError(
                "",
                "Please enter at least one result."
            );

            return View();
        }

        // SAVE RESULTS
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            _context.TblLabTestResults.AddRange(resultList);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Lab Test Results saved successfully.";

            return RedirectToAction(nameof(Create));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "Failed to save Lab Test Results."
            );

            return View();
        }
    }
}


// RESULT ENTRY VIEW MODEL
public class LabTestResultEntry
{
    public int ParameterId { get; set; }

    public string? ResultValue { get; set; }
}
