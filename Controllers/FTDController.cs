using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;
using FabricDevelopmentHub.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;

namespace FabricDevelopmentHub.Controllers
{
    public class FTDController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<FTDController> _logger;

        public FTDController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<FTDController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> FTDList(int page = 1, string searchString = "")
        {
            int pageSize = 30;

            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            // Menu
            var menuId = await _context.TblMenu
                .Where(x => x.MenuName == "FTD Information")
                .Select(x => x.MenuId)
                .FirstOrDefaultAsync();

            // Permissions
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

            if (!userPermissions.Contains("View"))
                return Forbid();

            // Base Query
            var query = _context.TblFtdMasters.AsNoTracking();

            // Company filter temporarily removed for testing

            // Search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(x =>
                    (x.FtdNo != null && x.FtdNo.Contains(searchString)) ||
                    (x.Control != null && x.Control.Contains(searchString)) ||
                    (x.Company != null && x.Company.Contains(searchString)) ||
                    (x.BatchNo != null && x.BatchNo.Contains(searchString)) ||
                    (x.SysReq != null && x.SysReq.Contains(searchString)) ||
                    (x.Buyer != null && x.Buyer.Contains(searchString)) ||
                    (x.StyleRef != null && x.StyleRef.Contains(searchString))
                );
            }

            // Count
            var totalRecords = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            page = totalPages > 0
                ? Math.Clamp(page, 1, totalPages)
                : 1;

            // Pagination
            var data = await query
                .OrderByDescending(x => x.Trdate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchString = searchString;

            return View(data);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new FTDViewModel();

            await LoadGroupList();

            // -----------------------------------------------------
            // INITIALIZE COLLECTIONS
            // -----------------------------------------------------

            model.TblFtdFabric = new List<TblFtdFabric>();
            model.TblFtdYarn = new List<TblFtdYarn>();
            model.TblFtdKnit = new List<TblFtdKnit>();
            model.TblFtdDyeingBasic = new List<TblFtdDyeingBasic>();
            model.TblFtdDyeingRecipe = new List<TblFtdDyeingRecipe>();

            // -----------------------------------------------------
            // INITIALIZE FINISH PROCESS MODELS
            // -----------------------------------------------------

            model.TblFinishProcessHeatset = new TblFinishProcessHeatset();
            model.TblFinishProcessStenteringAb = new TblFinishProcessStenteringAb();
            model.TblFinishProcessStenteringBb = new TblFinishProcessStenteringBb();
            model.TblSingeing = new TblSingeing();
            model.TblSqueezing = new TblSqueezing();
            model.TblBrush = new TblBrush();
            model.TblCompacting = new TblCompacting();
            model.TblOtherProcess = new TblOtherProcess();

            // -----------------------------------------------------
            // CONTROL / BATCH LIST
            // -----------------------------------------------------

            var controlList = await _context.Database
                .SqlQueryRaw<FTDControlDto>(
                    "EXEC dbo.SP_GET_FTD_CONTROL_LIST"
                )
                .ToListAsync();

            ViewBag.ControlList = controlList
                .Select(x => new SelectListItem
                {
                    Value = $"{x.INTERNAL_REF}|{x.BATCH_NO}",
                    Text = $"{x.INTERNAL_REF} - {x.BATCH_NO}"
                })
                .ToList();

            // -----------------------------------------------------
            // FTD NUMBER WILL BE GENERATED AFTER CONTROL SELECTION
            // -----------------------------------------------------

            model.TblFtdMaster.FtdNo = "";

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FTDViewModel model)
        {
            // -------------------------------------------------
            // MODEL VALIDATION
            // -------------------------------------------------

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError(
                            "ModelState Error: Field={Field}, Error={Error}",
                            state.Key,
                            error.ErrorMessage);
                    }
                }

                TempData["ErrorMessage"] =
                    "Please check the form. Some required fields are missing or invalid.";

                await ReloadCreateDropdowns();
                await LoadGroupList();

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -------------------------------------------------
                // SAVE FTD MASTER
                // -------------------------------------------------
                model.TblFtdMaster.Control = model.TblFtdMaster.Control?.Split('|')[0].Trim();
                var master = model.TblFtdMaster;

                master.Trdate = DateOnly.FromDateTime(DateTime.Now);

                // -------------------------------------------------
                // SAVE DRD PHOTO
                // -------------------------------------------------

                if (model.DrdPhoto != null &&
                    model.DrdPhoto.Length > 0)
                {
                    using var memoryStream = new MemoryStream();

                    await model.DrdPhoto.CopyToAsync(memoryStream);

                    master.DrdPhoto = memoryStream.ToArray();

                    master.DrdPhotoContentType =
                        model.DrdPhoto.ContentType;
                }

                _context.TblFtdMasters.Add(master);

                await _context.SaveChangesAsync();

                int ftdId = master.Ftdid;


                // -------------------------------------------------
                // SAVE FABRIC DETAILS
                // -------------------------------------------------

                if (model.TblFtdFabric != null)
                {
                    foreach (var item in model.TblFtdFabric)
                    {
                        item.Ftdid = ftdId;
                        _context.TblFtdFabrics.Add(item);
                    }
                }


                // -------------------------------------------------
                // SAVE YARN DETAILS
                // -------------------------------------------------

                if (model.TblFtdYarn != null)
                {
                    foreach (var item in model.TblFtdYarn)
                    {
                        item.Ftdid = ftdId;
                        _context.TblFtdYarns.Add(item);
                    }
                }


                // -------------------------------------------------
                // SAVE KNITTING DETAILS
                // -------------------------------------------------

                if (model.TblFtdKnit != null)
                {
                    for (int i = 0; i < model.TblFtdKnit.Count; i++)
                    {
                        var item = model.TblFtdKnit[i];

                        item.Ftdid = ftdId;

                        // ---------------------------------------------
                        // GET CORRESPONDING PHOTO
                        // ---------------------------------------------

                        var photo = Request.Form.Files.GetFile(
                            $"FtdKnitPhotos[{i}]"
                        );

                        // ---------------------------------------------
                        // SAVE PHOTO AS BYTE[]
                        // ---------------------------------------------

                        if (photo != null && photo.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();

                            await photo.CopyToAsync(memoryStream);

                            item.Photo = memoryStream.ToArray();

                            item.PhotoContentType = photo.ContentType;
                        }

                        // ---------------------------------------------
                        // ADD KNIT RECORD
                        // ---------------------------------------------

                        _context.TblFtdKnits.Add(item);
                    }
                }


                // -------------------------------------------------
                // SAVE DYEING BASIC DETAILS
                // -------------------------------------------------

                if (model.TblFtdDyeingBasic != null)
                {
                    foreach (var item in model.TblFtdDyeingBasic)
                    {
                        item.Ftdid = ftdId;
                        _context.TblFtdDyeingBasics.Add(item);
                    }
                }


                // -------------------------------------------------
                // SAVE LAB DETAILS
                // -------------------------------------------------

                if (model.TblLabDetails != null)
                {
                    foreach (var item in model.TblLabDetails)
                    {
                        item.Ftdid = ftdId;
                        _context.TblLabDetails.Add(item);
                    }
                }


                // -------------------------------------------------
                // SAVE DYEING RECIPE DETAILS
                // -------------------------------------------------

                if (model.TblFtdDyeingRecipe != null)
                {
                    foreach (var item in model.TblFtdDyeingRecipe)
                    {
                        item.Ftdid = ftdId;
                        _context.TblFtdDyeingRecipes.Add(item);
                    }
                }


                // -------------------------------------------------
                // SAVE HEATSET
                // -------------------------------------------------

                if (model.TblFinishProcessHeatset != null)
                {
                    model.TblFinishProcessHeatset.Ftdid = ftdId;

                    _context.TblFinishProcessHeatset.Add(
                        model.TblFinishProcessHeatset);
                }


                // -------------------------------------------------
                // SAVE STENTERING A/B
                // -------------------------------------------------

                if (model.TblFinishProcessStenteringAb != null)
                {
                    model.TblFinishProcessStenteringAb.Ftdid = ftdId;

                    _context.TblFinishProcessStenteringAb.Add(
                        model.TblFinishProcessStenteringAb);
                }


                // -------------------------------------------------
                // SAVE STENTERING B/B
                // -------------------------------------------------

                if (model.TblFinishProcessStenteringBb != null)
                {
                    model.TblFinishProcessStenteringBb.Ftdid = ftdId;

                    _context.TblFinishProcessStenteringBb.Add(
                        model.TblFinishProcessStenteringBb);
                }


                // -------------------------------------------------
                // SAVE SINGEING
                // -------------------------------------------------

                if (model.TblSingeing != null)
                {
                    model.TblSingeing.Ftdid = ftdId;

                    _context.TblSingeing.Add(
                        model.TblSingeing);
                }


                // -------------------------------------------------
                // SAVE SQUEEZING
                // -------------------------------------------------

                if (model.TblSqueezing != null)
                {
                    model.TblSqueezing.Ftdid = ftdId;

                    _context.TblSqueezing.Add(model.TblSqueezing);
                }


                // -------------------------------------------------
                // SAVE BRUSH
                // -------------------------------------------------

                if (model.TblBrush != null)
                {
                    model.TblBrush.Ftdid = ftdId;

                    _context.TblBrushe.Add(
                        model.TblBrush);
                }


                // -------------------------------------------------
                // SAVE COMPACTING DATA
                // -------------------------------------------------

                if (model.TblCompacting != null)
                {
                    model.TblCompacting.Ftdid = ftdId;

                    _context.TblCompacting.Add(
                        model.TblCompacting);
                }


                // -------------------------------------------------
                // SAVE OTHER PROCESS DATA
                // -------------------------------------------------

                if (model.TblOtherProcess != null)
                {
                    model.TblOtherProcess.Ftdid = ftdId;

                    _context.TblOtherProcess.Add(
                        model.TblOtherProcess);
                }


                // -------------------------------------------------
                // SAVE LAB TEST RESULTS
                // -------------------------------------------------

                if (model.Results != null && model.Results.Any())
                {
                    // VALIDATE GROUP AND TEST NAME

                    var validTest = await _context.TblLabTestNames
                        .AnyAsync(x =>
                            x.Ltnid == model.Ltnid &&
                            x.Ltgid == model.Ltgid &&
                            x.Status == "Active");

                    if (!validTest)
                    {
                        throw new InvalidOperationException(
                            "Invalid Group or Test Name.");
                    }


                    // VALIDATE METHOD

                    var validMethod = await _context.TblLabTestMethods
                        .AnyAsync(x =>
                            x.Ltmid == model.Ltmid &&
                            x.Ltnid == model.Ltnid &&
                            x.Status == "Active");

                    if (!validMethod)
                    {
                        throw new InvalidOperationException(
                            "Invalid Method selected.");
                    }


                    // CHECK DUPLICATE PARAMETER IDs

                    var parameterIds = model.Results
                        .Select(x => x.ParameterId)
                        .ToList();

                    if (parameterIds.Distinct().Count() != parameterIds.Count)
                    {
                        throw new InvalidOperationException(
                            "Duplicate parameters found.");
                    }


                    // VERIFY PARAMETERS

                    var validParameters = await _context.TblLabTestParameters
                        .Where(x =>
                            x.Ltnid == model.Ltnid &&
                            x.Status == "Active" &&
                            parameterIds.Contains(x.ParameterId))
                        .Select(x => x.ParameterId)
                        .ToListAsync();

                    if (validParameters.Count != parameterIds.Count)
                    {
                        throw new InvalidOperationException(
                            "Invalid Test Parameters.");
                    }


                    // FILTER ENTERED RESULTS

                    var enteredResults = model.Results
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.ResultValue))
                        .ToList();

                    if (!enteredResults.Any())
                    {
                        throw new InvalidOperationException(
                            "Please enter at least one result.");
                    }


                    // PREPARE RESULT LIST

                    var resultList = enteredResults.Select(x =>
                        new TblLabTestResult
                        {
                            Ftdid = ftdId,
                            Ltnid = model.Ltnid,
                            ParameterId = x.ParameterId,
                            ResultValue = x.ResultValue!.Trim(),
                            EntryDate = DateTime.Now,
                            Remarks = model.Remarks
                        })
                        .ToList();


                    // SAVE RESULTS

                    _context.TblLabTestResults.AddRange(resultList);
                }


                // -------------------------------------------------
                // LOG PENDING ENTITIES FOR DEBUGGING
                // -------------------------------------------------

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    if (entry.State == EntityState.Added ||
                        entry.State == EntityState.Modified)
                    {
                        var values = string.Join(
                            ", ",
                            entry.Properties.Select(p =>
                                $"{p.Metadata.Name}={p.CurrentValue ?? "NULL"}"));

                        _logger.LogInformation(
                            "Pending Entity: {EntityName}; State: {State}; Values: {Values}",
                            entry.Metadata.ClrType.Name,
                            entry.State,
                            values);
                    }
                }


                // -------------------------------------------------
                // SAVE ALL CHILD RECORDS
                // -------------------------------------------------

                await _context.SaveChangesAsync();


                // -------------------------------------------------
                // COMMIT TRANSACTION
                // -------------------------------------------------

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "FTD created successfully.";

                return RedirectToAction(nameof(FTDList));
            }
            catch (Exception ex)
            {
                // -------------------------------------------------
                // LOG ERROR DETAILS
                // -------------------------------------------------

                _logger.LogError(
                    ex,
                    "Failed to create FTD. FTD No: {FtdNo}",
                    model.TblFtdMaster?.FtdNo);

                var sqlError = ex;

                while (sqlError.InnerException != null)
                {
                    sqlError = sqlError.InnerException;
                }

                _logger.LogError(
                    "Root exception: {ExceptionMessage}",
                    sqlError.Message);


                // -------------------------------------------------
                // ROLLBACK TRANSACTION
                // -------------------------------------------------

                try
                {
                    if (transaction.GetDbTransaction().Connection != null)
                    {
                        await transaction.RollbackAsync();
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(
                        rollbackEx,
                        "Rollback failed while creating FTD.");
                }


                // -------------------------------------------------
                // ERROR MESSAGE
                // -------------------------------------------------

                TempData["ErrorMessage"] =
                    "Failed to save FTD data. Please check your input and try again.";

                ModelState.AddModelError(
                    string.Empty,
                    "Failed to save FTD data. Check application logs for details.");


                // -------------------------------------------------
                // RELOAD DROPDOWNS
                // -------------------------------------------------

                await ReloadCreateDropdowns();

                await LoadGroupList();


                // -------------------------------------------------
                // RETURN VIEW
                // -------------------------------------------------

                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            await LoadGroupList();

            var master = await _context.TblFtdMasters
                .FirstOrDefaultAsync(x => x.Ftdid == id);

            if (master == null)
            {
                return NotFound();
            }

            var model = new FTDViewModel
            {
                TblFtdMaster = master,

                // -----------------------------------------
                // LOAD FABRIC
                // -----------------------------------------

                TblFtdFabric = await _context.TblFtdFabrics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                // -----------------------------------------
                // LOAD YARN
                // -----------------------------------------

                TblFtdYarn = await _context.TblFtdYarns
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                // -----------------------------------------
                // LOAD KNITTING
                // -----------------------------------------

                TblFtdKnit = await _context.TblFtdKnits
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                // -----------------------------------------
                // LOAD DYEING BASIC
                // -----------------------------------------

                TblFtdDyeingBasic = await _context.TblFtdDyeingBasics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                // -----------------------------------------
                // LOAD DYEING RECIPE
                // -----------------------------------------

                TblFtdDyeingRecipe = await _context.TblFtdDyeingRecipes
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                // -----------------------------------------
                // LOAD LAB DETAILS
                // -----------------------------------------

                TblLabDetails = await _context.TblLabDetails
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                // -----------------------------------------
                // LOAD FINISH PROCESS
                // -----------------------------------------

                TblFinishProcessHeatset =
                    await _context.TblFinishProcessHeatset
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblFinishProcessStenteringAb =
                    await _context.TblFinishProcessStenteringAb
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblFinishProcessStenteringBb =
                    await _context.TblFinishProcessStenteringBb
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblSingeing =
                    await _context.TblSingeing
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblSqueezing =
                    await _context.TblSqueezing
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblBrush =
                    await _context.TblBrushe
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblCompacting =
                    await _context.TblCompacting
                        .FirstOrDefaultAsync(x => x.Ftdid == id),

                TblOtherProcess =
                    await _context.TblOtherProcess
                        .FirstOrDefaultAsync(x => x.Ftdid == id)
            };

            // -----------------------------------------
            // INITIALIZE EMPTY COLLECTIONS
            // -----------------------------------------

            model.TblFtdFabric ??= new List<TblFtdFabric>();
            model.TblFtdYarn ??= new List<TblFtdYarn>();
            model.TblFtdKnit ??= new List<TblFtdKnit>();
            model.TblFtdDyeingBasic ??= new List<TblFtdDyeingBasic>();
            model.TblFtdDyeingRecipe ??= new List<TblFtdDyeingRecipe>();
            model.TblLabDetails ??= new List<TblLabDetails>();

            // -----------------------------------------
            // INITIALIZE FINISH PROCESS MODELS
            // -----------------------------------------

            model.TblFinishProcessHeatset ??= new TblFinishProcessHeatset();
            model.TblFinishProcessStenteringAb ??= new TblFinishProcessStenteringAb();
            model.TblFinishProcessStenteringBb ??= new TblFinishProcessStenteringBb();
            model.TblSingeing ??= new TblSingeing();
            model.TblSqueezing ??= new TblSqueezing();
            model.TblBrush ??= new TblBrush();
            model.TblCompacting ??= new TblCompacting();
            model.TblOtherProcess ??= new TblOtherProcess();

            // -----------------------------------------
            // LOAD LAB TEST RESULTS
            // -----------------------------------------

            var existingResults = await _context.TblLabTestResults
                .Where(x => x.Ftdid == id)
                .ToListAsync();

            // Map your existing result properties to your view model.
            // This section depends on the exact Results model.

            // -----------------------------------------
            // CONTROL LIST
            // -----------------------------------------

            var controlList = await _context.Database
                .SqlQueryRaw<FTDControlDto>(
                    "EXEC dbo.SP_GET_FTD_CONTROL_LIST")
                .ToListAsync();

            ViewBag.ControlList = controlList
                .Select(x => new SelectListItem
                {
                    Value = $"{x.INTERNAL_REF}|{x.BATCH_NO}",
                    Text = $"{x.INTERNAL_REF} - {x.BATCH_NO}"
                })
                .ToList();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FTDViewModel model)
        {
            if (id != model.TblFtdMaster.Ftdid)
            {
                return BadRequest();
            }

            // -----------------------------------------
            // MODEL VALIDATION
            // -----------------------------------------

            if (!ModelState.IsValid)
            {
                await ReloadCreateDropdowns();
                await LoadGroupList();

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -----------------------------------------
                // GET EXISTING MASTER
                // -----------------------------------------

                var master = await _context.TblFtdMasters
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (master == null)
                {
                    return NotFound();
                }

                // -----------------------------------------
                // UPDATE MASTER
                // -----------------------------------------

                var updatedMaster = model.TblFtdMaster;

                _context.Entry(master).CurrentValues.SetValues(updatedMaster);

                // Preserve original creation date if it should not change.
                // Set it explicitly here if your application requires it.

                // -----------------------------------------
                // UPDATE DRD PHOTO
                // -----------------------------------------

                if (model.DrdPhoto != null &&
                    model.DrdPhoto.Length > 0)
                {
                    using var memoryStream = new MemoryStream();

                    await model.DrdPhoto.CopyToAsync(memoryStream);

                    master.DrdPhoto = memoryStream.ToArray();

                    master.DrdPhotoContentType =
                        model.DrdPhoto.ContentType;
                }

                // -----------------------------------------
                // UPDATE FABRIC
                // -----------------------------------------

                var oldFabric = await _context.TblFtdFabrics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdFabrics.RemoveRange(oldFabric);

                if (model.TblFtdFabric != null)
                {
                    foreach (var item in model.TblFtdFabric)
                    {
                        item.Ftdid = id;

                        _context.TblFtdFabrics.Add(item);
                    }
                }

                // -----------------------------------------
                // UPDATE YARN
                // -----------------------------------------

                var oldYarn = await _context.TblFtdYarns
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdYarns.RemoveRange(oldYarn);

                if (model.TblFtdYarn != null)
                {
                    foreach (var item in model.TblFtdYarn)
                    {
                        item.Ftdid = id;

                        _context.TblFtdYarns.Add(item);
                    }
                }

                // -----------------------------------------
                // UPDATE KNITTING
                // -----------------------------------------

                var oldKnit = await _context.TblFtdKnits
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdKnits.RemoveRange(oldKnit);

                if (model.TblFtdKnit != null)
                {
                    for (int i = 0; i < model.TblFtdKnit.Count; i++)
                    {
                        var item = model.TblFtdKnit[i];

                        item.Ftdid = id;

                        var photo = Request.Form.Files.GetFile(
                            $"FtdKnitPhotos[{i}]");

                        if (photo != null && photo.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();

                            await photo.CopyToAsync(memoryStream);

                            item.Photo = memoryStream.ToArray();

                            item.PhotoContentType = photo.ContentType;
                        }

                        _context.TblFtdKnits.Add(item);
                    }
                }

                // -----------------------------------------
                // UPDATE DYEING BASIC
                // -----------------------------------------

                var oldDyeingBasic = await _context.TblFtdDyeingBasics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdDyeingBasics.RemoveRange(oldDyeingBasic);

                if (model.TblFtdDyeingBasic != null)
                {
                    foreach (var item in model.TblFtdDyeingBasic)
                    {
                        item.Ftdid = id;

                        _context.TblFtdDyeingBasics.Add(item);
                    }
                }

                // -----------------------------------------
                // UPDATE LAB DETAILS
                // -----------------------------------------

                var oldLabDetails = await _context.TblLabDetails
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblLabDetails.RemoveRange(oldLabDetails);

                if (model.TblLabDetails != null)
                {
                    foreach (var item in model.TblLabDetails)
                    {
                        item.Ftdid = id;

                        _context.TblLabDetails.Add(item);
                    }
                }

                // -----------------------------------------
                // UPDATE DYEING RECIPE
                // -----------------------------------------

                var oldRecipes = await _context.TblFtdDyeingRecipes
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdDyeingRecipes.RemoveRange(oldRecipes);

                if (model.TblFtdDyeingRecipe != null)
                {
                    foreach (var item in model.TblFtdDyeingRecipe)
                    {
                        item.Ftdid = id;

                        _context.TblFtdDyeingRecipes.Add(item);
                    }
                }

                // -----------------------------------------
                // UPDATE FINISH PROCESS - HEATSET
                // -----------------------------------------

                var heatset = await _context.TblFinishProcessHeatset
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (heatset != null)
                {
                    if (model.TblFinishProcessHeatset != null)
                    {
                        var source = model.TblFinishProcessHeatset;

                        heatset.Noc = source.Noc;
                        heatset.Nop = source.Nop;
                        heatset.DweelTime = source.DweelTime;
                        heatset.GreyDia = source.GreyDia;
                        heatset.GreyGsm = source.GreyGsm;
                        heatset.Temp = source.Temp;
                        heatset.Speed = source.Speed;
                        heatset.OFeed = source.OFeed;
                        heatset.UFeed = source.UFeed;
                        heatset.Blower = source.Blower;
                        heatset.Pp = source.Pp;
                        heatset.SettingDia = source.SettingDia;
                        heatset.ChemicalName = source.ChemicalName;
                        heatset.Ratio = source.Ratio;
                        heatset.AHeatsetGsm = source.AHeatsetGsm;
                        heatset.AHeatsetDia = source.AHeatsetDia;
                        heatset.Comments = source.Comments;
                    }
                }
                else if (model.TblFinishProcessHeatset != null)
                {
                    model.TblFinishProcessHeatset.Ftdid = id;

                    _context.TblFinishProcessHeatset.Add(
                        model.TblFinishProcessHeatset
                    );
                }

                // -----------------------------------------
                // UPDATE STENTERING A/B
                // -----------------------------------------

                var stenterAb = await _context.TblFinishProcessStenteringAb
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (stenterAb != null)
                {
                    if (model.TblFinishProcessStenteringAb != null)
                    {
                        var source = model.TblFinishProcessStenteringAb;

                        var entry = _context.Entry(stenterAb);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterAb.Trid) ||
                                property.Name == nameof(stenterAb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblFinishProcessStenteringAb != null)
                {
                    model.TblFinishProcessStenteringAb.Ftdid = id;

                    _context.TblFinishProcessStenteringAb.Add(
                        model.TblFinishProcessStenteringAb
                    );
                }

                // -----------------------------------------
                // UPDATE STENTERING B/B
                // -----------------------------------------

                var stenterBb = await _context.TblFinishProcessStenteringBb
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (stenterBb != null)
                {
                    if (model.TblFinishProcessStenteringBb != null)
                    {
                        var source = model.TblFinishProcessStenteringBb;

                        var entry = _context.Entry(stenterBb);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterBb.Trid) ||
                                property.Name == nameof(stenterBb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblFinishProcessStenteringBb != null)
                {
                    model.TblFinishProcessStenteringBb.Ftdid = id;

                    _context.TblFinishProcessStenteringBb.Add(
                        model.TblFinishProcessStenteringBb
                    );
                }

                // UPDATE SINGEING
                var singeing = await _context.TblSingeing.FirstOrDefaultAsync(x => x.Ftdid == id);
                if (singeing != null)
                {
                    if (model.TblSingeing != null)
                    {
                        var source = model.TblSingeing;

                        var entry = _context.Entry(singeing);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterAb.Trid) ||
                                property.Name == nameof(stenterAb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblSingeing != null)
                {
                    model.TblSingeing.Ftdid = id;

                    _context.TblSingeing.Add(
                        model.TblSingeing
                    );
                }

                // UPDATE SQUEEZING
                var squeezing = await _context.TblSqueezing.FirstOrDefaultAsync(x => x.Ftdid == id);
                if (squeezing != null)
                {
                    if (model.TblSqueezing != null)
                    {
                        var source = model.TblSqueezing;

                        var entry = _context.Entry(squeezing);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterAb.Trid) ||
                                property.Name == nameof(stenterAb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblSqueezing != null)
                {
                    model.TblSqueezing.Ftdid = id;
                    _context.TblSqueezing.Add(model.TblSqueezing);
                }

                // UPDATE BRUSH
                var brush = await _context.TblBrushe.FirstOrDefaultAsync(x => x.Ftdid == id);
                if (brush != null)
                {
                    if (model.TblBrush != null)
                    {
                        var source = model.TblBrush;

                        var entry = _context.Entry(brush);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterAb.Trid) ||
                                property.Name == nameof(stenterAb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblBrush != null)
                {
                    model.TblBrush.Ftdid = id;
                    _context.TblBrushe.Add(model.TblBrush);
                }

                // UPDATE COMPACTING
                var compacting = await _context.TblCompacting.FirstOrDefaultAsync(x => x.Ftdid == id);
                if (compacting != null)
                {
                    if (model.TblCompacting != null)
                    {
                        var source = model.TblCompacting;

                        var entry = _context.Entry(compacting);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterAb.Trid) ||
                                property.Name == nameof(stenterAb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblCompacting != null)
                {
                    model.TblCompacting.Ftdid = id;
                    _context.TblCompacting.Add(model.TblCompacting);
                }

                // UPDATE OTHER PROCESS

                var otherProcess = await _context.TblOtherProcess.FirstOrDefaultAsync(x => x.Ftdid == id);
                if (otherProcess != null)
                {
                    if (model.TblOtherProcess != null)
                    {
                        var source = model.TblOtherProcess;

                        var entry = _context.Entry(squeezing);

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            // Never update Primary Key or Foreign Key
                            if (property.Name == nameof(stenterAb.Trid) ||
                                property.Name == nameof(stenterAb.Ftdid))
                            {
                                continue;
                            }

                            var sourceProperty = source.GetType()
                                .GetProperty(property.Name);

                            if (sourceProperty != null)
                            {
                                entry.Property(property.Name).CurrentValue =
                                    sourceProperty.GetValue(source);
                            }
                        }
                    }
                }
                else if (model.TblOtherProcess != null)
                {
                    model.TblOtherProcess.Ftdid = id;
                    _context.TblOtherProcess.Add(model.TblOtherProcess);
                }


                // -----------------------------------------
                // UPDATE LAB TEST RESULTS
                // -----------------------------------------

                if (model.Results != null && model.Results.Any())
                {
                    var validTest = await _context.TblLabTestNames
                        .AnyAsync(x =>
                            x.Ltnid == model.Ltnid &&
                            x.Ltgid == model.Ltgid &&
                            x.Status == "Active");

                    if (!validTest)
                    {
                        throw new InvalidOperationException(
                            "Invalid Group or Test Name.");
                    }

                    var validMethod = await _context.TblLabTestMethods
                        .AnyAsync(x =>
                            x.Ltmid == model.Ltmid &&
                            x.Ltnid == model.Ltnid &&
                            x.Status == "Active");

                    if (!validMethod)
                    {
                        throw new InvalidOperationException(
                            "Invalid Method selected.");
                    }

                    var parameterIds = model.Results
                        .Select(x => x.ParameterId)
                        .ToList();

                    if (parameterIds.Distinct().Count() != parameterIds.Count)
                    {
                        throw new InvalidOperationException(
                            "Duplicate parameters found.");
                    }

                    var validParameters = await _context.TblLabTestParameters
                        .Where(x =>
                            x.Ltnid == model.Ltnid &&
                            x.Status == "Active" &&
                            parameterIds.Contains(x.ParameterId))
                        .Select(x => x.ParameterId)
                        .ToListAsync();

                    if (validParameters.Count != parameterIds.Count)
                    {
                        throw new InvalidOperationException(
                            "Invalid Test Parameters.");
                    }

                    var enteredResults = model.Results
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.ResultValue))
                        .ToList();

                    if (!enteredResults.Any())
                    {
                        throw new InvalidOperationException(
                            "Please enter at least one result.");
                    }

                    // Delete old results and insert updated results.

                    var oldResults = await _context.TblLabTestResults
                        .Where(x => x.Ftdid == id)
                        .ToListAsync();

                    _context.TblLabTestResults.RemoveRange(oldResults);

                    var resultList = enteredResults.Select(x =>
                        new TblLabTestResult
                        {
                            Ftdid = id,
                            Ltnid = model.Ltnid,
                            ParameterId = x.ParameterId,
                            ResultValue = x.ResultValue!.Trim(),
                            EntryDate = DateTime.Now,
                            Remarks = model.Remarks
                        })
                        .ToList();

                    _context.TblLabTestResults.AddRange(resultList);
                }
                else
                {
                    var oldResults = await _context.TblLabTestResults
                        .Where(x => x.Ftdid == id)
                        .ToListAsync();

                    _context.TblLabTestResults.RemoveRange(oldResults);
                }

                // -----------------------------------------
                // SAVE ALL CHANGES
                // -----------------------------------------

                await _context.SaveChangesAsync();

                // -----------------------------------------
                // COMMIT
                // -----------------------------------------

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "FTD updated successfully.";

                return RedirectToAction(nameof(FTDList));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update FTD. FTD ID: {FtdId}",
                    id);

                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(
                        rollbackEx,
                        "Rollback failed while updating FTD.");
                }

                TempData["ErrorMessage"] =
                    "Failed to update FTD data. Please check your input.";

                ModelState.AddModelError(
                    string.Empty,
                    "Failed to update FTD data. Check application logs.");

                await ReloadCreateDropdowns();
                await LoadGroupList();

                return View(model);
            }
        }

        // =========================================================
        // GET CONTROL DETAILS / MASTER DATA
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetControlDetails(string internalRef)
        {
            try
            {
                // -------------------------------------------------
                // VALIDATE INTERNAL REF
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(internalRef))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Internal Ref is required."
                    });
                }


                // -------------------------------------------------
                // GET MASTER DATA
                // SP PARAMETER:
                // @INTERNAL_REF
                // -------------------------------------------------

                var result = await _context.Database
                    .SqlQueryRaw<FTDMasterDto>(
                        "EXEC dbo.SP_GET_FTD_SAMPLE_MASTER @INTERNAL_REF = {0}",
                        internalRef
                    )
                    .ToListAsync();


                // -------------------------------------------------
                // GET FIRST RECORD
                // -------------------------------------------------

                var master = result.FirstOrDefault();


                // -------------------------------------------------
                // NO DATA FOUND
                // -------------------------------------------------

                if (master == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "No master data found for Internal Ref: "
                            + internalRef
                    });
                }


                // -------------------------------------------------
                // RETURN JSON
                // -------------------------------------------------

                return Json(new
                {
                    success = true,

                    data = new
                    {
                        companyName = master.CompanyName,
                        workingCompanyName = master.WorkingCompanyName,
                        companyShortName = master.CompanyShortName,
                        internalRef = master.InternalRef,
                        styleRefNo = master.StyleRefNo,
                        sysReq = master.SysReq,
                        buyerName = master.BuyerName,
                        bookingNo = master.BookingNo,
                        batchNo = master.BatchNo,
                        fso = master.Fso,
                    }
                });
            }
            catch (Exception ex)
            {
                // -------------------------------------------------
                // DEBUG ERROR
                // -------------------------------------------------

                Console.WriteLine("======================================");
                Console.WriteLine("GET CONTROL DETAILS ERROR");
                Console.WriteLine("======================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");


                return StatusCode(500, new
                {
                    success = false,

                    message =
                        "An error occurred while loading Control details.",

                    error = ex.Message,

                    innerError =
                        ex.InnerException?.Message
                });
            }
        }



        // =========================================================
        // GET FABRIC DETAILS BY BATCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetFabricDetails(string batchNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Batch No is required."
                    });
                }

                var result = await _context.Database
                    .SqlQueryRaw<FTDFabricDto>(
                        "EXEC dbo.SP_GET_FTD_FABRIC_DETAILS @BATCH_NO = {0}",
                        batchNo
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,

                    data = result.Select(x => new
                    {
                        bookingNo = x.BookingNo,
                        fabricDescription = x.FabricDescription,
                        construction = x.Construction,
                        colorTypeId = x.ColorTypeId,
                        colorName = x.ColorName,
                        gsmWeight = x.GsmWeight,
                        dia = x.Dia,
                        diaWidth = x.DiaWidth,
                        dtlsId = x.DtlsId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("======================================");
                Console.WriteLine("GET FABRIC DETAILS ERROR");
                Console.WriteLine("======================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Fabric details.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET YARN DETAILS BY BATCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetYarnDetails(string batchNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Batch No is required."
                    });
                }

                var result = await _context.Database
                    .SqlQueryRaw<FTDYarnDto>(
                        "EXEC dbo.SP_GET_YARN_DETAILS @BATCH_NO = {0}",
                        batchNo
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,

                    data = result.Select(x => new
                    {
                        count = x.Count,
                        lot = x.Lot,
                        brand = x.Brand,
                        construction = x.Construction
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Yarn details.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetKnitDetails(string batchNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Batch No is required."
                    });
                }

                var result = await _context.Database
                    .SqlQueryRaw<FTDKnitDto>(
                        "EXEC dbo.SP_GET_FTD_KNIT_DETAILS @BATCH_NO = {0}",
                        batchNo
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    count = result.Count,

                    data = result.Select(x => new
                    {
                        programNo = x.PROGRAM_NO,
                        construction = x.CONSTRUCTION,
                        source = x.KNITTING_SOURCE,
                        knitCom = x.COMPANY_NAME,
                        mcNo = x.MACHINE_NO,
                        mcDia = x.MACHINE_DIA,
                        mcGauge = x.MACHINE_GG,
                        sl = x.STITCH_LENGTH,
                        measurement = x.MEASUREMENT,
                        colorName = x.COLOR_NAME,
                        uom = x.UOM,
                        feederNo = x.TOTFIDDER
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Knit details.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        //Dyeing Basic Data By Batch
        [HttpGet]
        public async Task<IActionResult> GetDyeingBasicData(string batchNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Batch No is required."
                    });
                }

                var result = await _context.Database
                    .SqlQueryRaw<FTDDyeingBasicDto>(
                        "EXEC dbo.SP_GET_FTD_DYEING_BASIC_DATA @BATCH_NO = {0}",
                        batchNo
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,

                    data = result.Select(x => new
                    {
                        batchNo = x.BATCH_NO,
                        machineNo = x.MACHINE,
                        inhouseDyeing = x.INHOUSE_DYEING,
                        shadeP = x.SHADE_P,
                        shade = x.SHADE,
                        dyeingPart = x.DYEING_PART,
                        isNonRft = x.IS_NON_RFT,
                        productName = x.PRODUCT_NAME_DETAILS,
                        enzayemPer = x.RATIO
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("======================================");
                Console.WriteLine("GET DYEING BASIC DATA ERROR");
                Console.WriteLine("======================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Dyeing Basic data.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }


        //Dyeing Recipe Data By Batch
        [HttpGet]
        public async Task<IActionResult> GetDyeingRecipeData(string batchNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Batch No is required."
                    });
                }

                var result = await _context.Database
                    .SqlQueryRaw<FTDDyeingRecipeDto>(
                        "EXEC dbo.SP_GET_FTD_DYEING_RECIPE_DATA @BATCH_NO = {0}",
                        batchNo
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,

                    data = result.Select(x => new
                    {
                        batchNo = x.BATCH_NO,
                        recipeDetails = x.RECIPE_DETAILS,
                        ratio = x.RATIO
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("GET DYEING RECIPE DATA ERROR");
                Console.WriteLine("======================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Dyeing Recipe data.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
        // =========================================================
        // GET LAB DIP LIST
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetLabDipList()
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<FTDLabDipListDto>(
                        "EXEC dbo.SP_GET_LAB_DIP_LIST"
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,

                    data = result.Select(x => new
                    {
                        labDipNo = x.LAB_DIP_NO
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("GET LAB DIP LIST ERROR");
                Console.WriteLine("======================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Lab Dip list.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetLabDipDetails(string labDipNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(labDipNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lab Dip No is required."
                    });
                }

                var result = await _context.Database
                    .SqlQueryRaw<FTDLabDipDto>(
                        "EXEC dbo.SP_GET_LAB_DIP_DETAILS @LAB_DIP_NO = {0}",
                        labDipNo
                    )
                    .ToListAsync();

                return Json(new
                {
                    success = true,

                    data = result.Select(x => new
                    {
                        labDipNo = x.LAB_DIP_NO,
                        labProductName = x.LAB_PRODUCT_NAME,
                        labRatio = x.LAB_RATIO
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("GET LAB DIP DETAILS ERROR");
                Console.WriteLine("======================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while loading Lab Dip details.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // =========================================================
        // CONTROL DTO
        // =========================================================

        public class FTDControlDto
        {
            public string? INTERNAL_REF { get; set; }

            public string? BATCH_NO { get; set; }
        }


        // =========================================================
        // MASTER DTO
        // =========================================================

        public class FTDMasterDto
        {
            public string? CompanyName { get; set; }//LC Company
            public string? CompanyShortName { get; set; }
            public string? WorkingCompanyName { get; set; }

            public string? InternalRef { get; set; }

            public string? StyleRefNo { get; set; }

            public string? SysReq { get; set; }

            public string? BuyerName { get; set; }

            public string? BookingNo { get; set; }

            public string? BatchNo { get; set; }
            public string? Fso { get; set; }
        }


        // =========================================================
        // FABRIC DTO
        // =========================================================

        public class FTDFabricDto
        {
            public string? BookingNo { get; set; }

            public string? FabricDescription { get; set; }

            public string? Construction { get; set; }

            public string? ColorTypeId { get; set; }

            public string? ColorName { get; set; }

            public decimal? GsmWeight { get; set; }

            public string? Dia { get; set; }

            public string? DiaWidth { get; set; }

            public decimal? DtlsId { get; set; }
        }

        public class FTDYarnDto
        {
            public string? Count { get; set; }
            public string? Lot { get; set; }
            public string? Brand { get; set; }
            public string? Construction { get; set; }
        }

        public class FTDKnitDto
        {
            public string? PROGRAM_NO { get; set; }

            public string? CONSTRUCTION { get; set; }
            public string? KNITTING_SOURCE { get; set; }

            public string? COMPANY_NAME { get; set; }

            public string? MACHINE_NO { get; set; }

            public string? MACHINE_DIA { get; set; }

            public string? MACHINE_GG { get; set; }

            public string? STITCH_LENGTH { get; set; }

            public string? COLOR_NAME { get; set; }

            public string? MEASUREMENT { get; set; }

            public string? UOM { get; set; }

            public string? TOTFIDDER { get; set; }
        }

        public class FTDDyeingBasicDto
        {
            public string? BATCH_NO { get; set; }
            public string? MACHINE { get; set; }
            public double? INHOUSE_DYEING { get; set; }
            public double? SHADE_P { get; set; }
            public string? SHADE { get; set; }
            public string? DYEING_PART { get; set; }
            public string? IS_NON_RFT { get; set; }
            public string? PRODUCT_NAME_DETAILS { get; set; }
            public double? RATIO { get; set; }
        }

        public class FTDDyeingRecipeDto
        {
            public string? BATCH_NO { get; set; }
            public double? RATIO { get; set; }
            public string? RECIPE_DETAILS { get; set; }
        }

        public class FTDLabDipListDto
        {
            public string? LAB_DIP_NO { get; set; }
        }
        public class FTDLabDipDto
        {
            public string? LAB_DIP_NO { get; set; }

            public string? LAB_PRODUCT_NAME { get; set; }

            public double? LAB_RATIO { get; set; }
        }
        private async Task ReloadCreateDropdowns()
        {
            var controlList = await _context.Database
                .SqlQueryRaw<FTDControlDto>(
                    "EXEC dbo.SP_GET_FTD_CONTROL_LIST")
                .ToListAsync();

            ViewBag.ControlList = controlList
                .Select(x => new SelectListItem
                {
                    Value = $"{x.INTERNAL_REF}|{x.BATCH_NO}",
                    Text = $"{x.INTERNAL_REF} - {x.BATCH_NO}"
                })
                .ToList();
        }


        private void CopyNonKeyValues<TEntity>(
            TEntity target,
            TEntity source)
            where TEntity : class
        {
            var entry = _context.Entry(target);

            foreach (var property in entry.Metadata.GetProperties())
            {
                // Never overwrite the primary key
                if (property.IsPrimaryKey())
                    continue;

                var clrProperty = typeof(TEntity)
                    .GetProperty(property.Name);

                if (clrProperty == null)
                    continue;

                var value = clrProperty.GetValue(source);

                entry.Property(property.Name).CurrentValue = value;
            }
        }


        private async Task SaveSingleChild<TEntity>(
            int ftdId,
            TEntity? incoming,
            DbSet<TEntity> dbSet)
            where TEntity : class
        {
            if (incoming == null)
                return;

            // Set FTDID on incoming entity
            var ftdidProperty = typeof(TEntity)
                .GetProperty("Ftdid");

            if (ftdidProperty == null)
            {
                throw new InvalidOperationException(
                    $"{typeof(TEntity).Name} has no Ftdid property.");
            }

            ftdidProperty.SetValue(incoming, ftdId);

            // Find existing record using FTDID
            var existing = await dbSet.FirstOrDefaultAsync(
                x => EF.Property<int>(x, "Ftdid") == ftdId);

            if (existing == null)
            {
                dbSet.Add(incoming);
            }
            else
            {
                CopyNonKeyValues(existing, incoming);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Step 1: Load FTD Master
            var master = await _context.TblFtdMasters
                .FirstOrDefaultAsync(x => x.Ftdid == id);

            if (master == null)
            {
                return NotFound();
            }

            // Step 2: Load Fabric Details
            var fabricList = await _context.TblFtdFabrics
                .Where(x => x.Ftdid == master.Ftdid)
                .ToListAsync();

            // Step 3: Load Yarn Details
            var yarnList = await _context.TblFtdYarns
                .Where(x => x.Ftdid == master.Ftdid)
                .ToListAsync();

            // Step 4: Load Knit Details
            var knitList = await _context.TblFtdKnits
                .Where(x => x.Ftdid == master.Ftdid)
                .ToListAsync();

            // Step 5: Load Dyeing Basic Details
            var dyeingBasicList = await _context.TblFtdDyeingBasics
                .Where(x => x.Ftdid == master.Ftdid)
                .ToListAsync();

            // Step 6: Load Dyeing Recipe Details
            var dyeingRecipeList = await _context.TblFtdDyeingRecipes
                .Where(x => x.Ftdid == master.Ftdid)
                .ToListAsync();

            // Step 7: Load Heatset Details
            var heatset = await _context.TblFinishProcessHeatset
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 8: Load Stentering A/B Details
            var stenteringAb = await _context.TblFinishProcessStenteringAb
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 9: Load Stentering B/B Details
            var stenteringBb = await _context.TblFinishProcessStenteringBb
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 10: Load Singeing Details
            var singeing = await _context.TblSingeing
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 11: Load Squeezing Details
            var squeezing = await _context.TblSqueezing
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 12: Load Brush Details
            var brush = await _context.TblBrushe
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 13: Load Compacting Details
            var compacting = await _context.TblCompacting
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 14: Load Other Process Details
            var otherProcess = await _context.TblOtherProcess
                .FirstOrDefaultAsync(x => x.Ftdid == master.Ftdid);

            // Step 15: Prepare ViewModel
            var model = new FTDViewModel
            {
                TblFtdMaster = master,
                TblFtdFabric = fabricList,
                TblFtdYarn = yarnList,
                TblFtdKnit = knitList,
                TblFtdDyeingBasic = dyeingBasicList,
                TblFtdDyeingRecipe = dyeingRecipeList,

                TblFinishProcessHeatset = heatset,
                TblFinishProcessStenteringAb = stenteringAb,
                TblFinishProcessStenteringBb = stenteringBb,

                TblSingeing = singeing,
                TblSqueezing = squeezing,
                TblBrush = brush,
                TblCompacting = compacting,
                TblOtherProcess = otherProcess
            };

            return View(model);
        }

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
        [HttpGet]
        public async Task<IActionResult> GetNextFtdNumber(string prefix)
        {
            var lastNumber = await _context.TblFtdMasters
                .AsNoTracking()
                .Where(x => x.FtdNo.StartsWith(prefix))
                .Select(x => (int?)Convert.ToInt32(
                    x.FtdNo.Substring(x.FtdNo.Length - 5)))
                .MaxAsync() ?? 0;

            string ftdNumber = $"{prefix}{(lastNumber + 1):D5}";

            return Json(new
            {
                ftdNo = ftdNumber
            });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // -------------------------------------------------
                // FIND FTD MASTER
                // -------------------------------------------------

                var master = await _context.TblFtdMasters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (master == null)
                {
                    TempData["ErrorMessage"] = "FTD record not found.";
                    return RedirectToAction(nameof(FTDList));
                }

                // -------------------------------------------------
                // LOAD VIEW MODEL
                // -------------------------------------------------

                var model = new FTDViewModel
                {
                    TblFtdMaster = master
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading FTD Delete page. FTD ID: {FtdId}",
                    id);

                TempData["ErrorMessage"] =
                    "Failed to load FTD details.";

                return RedirectToAction(nameof(FTDList));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -------------------------------------------------
                // FIND FTD MASTER
                // -------------------------------------------------

                var master = await _context.TblFtdMasters
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (master == null)
                {
                    TempData["ErrorMessage"] =
                        "FTD record not found.";

                    return RedirectToAction(nameof(FTDList));
                }


                // -------------------------------------------------
                // DELETE LAB TEST RESULTS
                // -------------------------------------------------

                var labResults = await _context.TblLabTestResults
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                if (labResults.Any())
                {
                    _context.TblLabTestResults.RemoveRange(labResults);
                }


                // -------------------------------------------------
                // DELETE FABRIC DETAILS
                // -------------------------------------------------

                var fabrics = await _context.TblFtdFabrics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdFabrics.RemoveRange(fabrics);


                // -------------------------------------------------
                // DELETE YARN DETAILS
                // -------------------------------------------------

                var yarns = await _context.TblFtdYarns
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdYarns.RemoveRange(yarns);


                // -------------------------------------------------
                // DELETE KNITTING DETAILS
                // -------------------------------------------------

                var knits = await _context.TblFtdKnits
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdKnits.RemoveRange(knits);


                // -------------------------------------------------
                // DELETE DYEING BASIC DETAILS
                // -------------------------------------------------

                var dyeingBasics = await _context.TblFtdDyeingBasics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdDyeingBasics.RemoveRange(dyeingBasics);


                // -------------------------------------------------
                // DELETE LAB DETAILS
                // -------------------------------------------------

                var labDetails = await _context.TblLabDetails
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblLabDetails.RemoveRange(labDetails);


                // -------------------------------------------------
                // DELETE DYEING RECIPE DETAILS
                // -------------------------------------------------

                var dyeingRecipes = await _context.TblFtdDyeingRecipes
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFtdDyeingRecipes.RemoveRange(dyeingRecipes);


                // -------------------------------------------------
                // DELETE HEATSET
                // -------------------------------------------------

                var heatset = await _context.TblFinishProcessHeatset
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFinishProcessHeatset.RemoveRange(heatset);


                // -------------------------------------------------
                // DELETE STENTERING A/B
                // -------------------------------------------------

                var stenteringAb = await _context.TblFinishProcessStenteringAb
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFinishProcessStenteringAb.RemoveRange(stenteringAb);


                // -------------------------------------------------
                // DELETE STENTERING B/B
                // -------------------------------------------------

                var stenteringBb = await _context.TblFinishProcessStenteringBb
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblFinishProcessStenteringBb.RemoveRange(stenteringBb);


                // -------------------------------------------------
                // DELETE SINGEING
                // -------------------------------------------------

                var singeing = await _context.TblSingeing
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblSingeing.RemoveRange(singeing);


                // -------------------------------------------------
                // DELETE SQUEEZING
                // -------------------------------------------------

                var squeezing = await _context.TblSqueezing
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblSqueezing.RemoveRange(squeezing);


                // -------------------------------------------------
                // DELETE BRUSH
                // -------------------------------------------------

                var brush = await _context.TblBrushe
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblBrushe.RemoveRange(brush);


                // -------------------------------------------------
                // DELETE COMPACTING
                // -------------------------------------------------

                var compacting = await _context.TblCompacting
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblCompacting.RemoveRange(compacting);


                // -------------------------------------------------
                // DELETE OTHER PROCESS
                // -------------------------------------------------

                var otherProcess = await _context.TblOtherProcess
                    .Where(x => x.Ftdid == id)
                    .ToListAsync();

                _context.TblOtherProcess.RemoveRange(otherProcess);


                // -------------------------------------------------
                // SAVE CHILD RECORD DELETIONS
                // -------------------------------------------------

                await _context.SaveChangesAsync();


                // -------------------------------------------------
                // DELETE FTD MASTER
                // -------------------------------------------------

                _context.TblFtdMasters.Remove(master);

                await _context.SaveChangesAsync();


                // -------------------------------------------------
                // COMMIT TRANSACTION
                // -------------------------------------------------

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "FTD deleted successfully.";

                return RedirectToAction(nameof(FTDList));
            }
            catch (Exception ex)
            {
                // -------------------------------------------------
                // LOG ERROR
                // -------------------------------------------------

                _logger.LogError(
                    ex,
                    "Failed to delete FTD. FTD ID: {FtdId}",
                    id);


                // -------------------------------------------------
                // GET ROOT EXCEPTION
                // -------------------------------------------------

                var sqlError = ex;

                while (sqlError.InnerException != null)
                {
                    sqlError = sqlError.InnerException;
                }

                _logger.LogError(
                    "Root exception: {ExceptionMessage}",
                    sqlError.Message);


                // -------------------------------------------------
                // ROLLBACK TRANSACTION
                // -------------------------------------------------

                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(
                        rollbackEx,
                        "Rollback failed while deleting FTD.");
                }


                // -------------------------------------------------
                // ERROR MESSAGE
                // -------------------------------------------------

                TempData["ErrorMessage"] =
                    "Failed to delete FTD data. Please try again.";

                return RedirectToAction(nameof(FTDList));
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckBatchAlreadyExists(string batchNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNo))
                {
                    return Json(new
                    {
                        success = false,
                        exists = false,
                        message = "Batch No. is required."
                    });
                }

                batchNo = batchNo.Trim();

                bool exists = await _context.TblFtdMasters
                    .AnyAsync(x => x.BatchNo == batchNo);

                return Json(new
                {
                    success = true,
                    exists = exists,
                    message = exists
                        ? "This Batch No. already exists in FTD."
                        : "Batch No. is available."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    exists = false,
                    message = ex.Message
                });
            }
        }
    }

}
