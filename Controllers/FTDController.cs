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
                .OrderByDescending(x => x.FtdNo)
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

            // Get logged-in user's company
            var userCompany = await _userManager
                .GetUserAsync(User);

            if (userCompany == null ||
                string.IsNullOrWhiteSpace(userCompany.Company))
            {
                return Forbid();
            }

            string companyCode = userCompany.Company;

            // Generate FTD No
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string ftdID = $"{companyCode}-FTD{datePart}";

            model.TblFtdMaster.FtdNo = ftdID;

            // Initialize collections
            model.TblFtdFabric = new List<TblFtdFabric>();
            model.TblFtdYarn = new List<TblFtdYarn>();
            model.TblFtdKnit = new List<TblFtdKnit>();
            model.TblFtdDyeingBasic = new List<TblFtdDyeingBasic>();
            model.TblFtdDyeingRecipe = new List<TblFtdDyeingRecipe>();

            // Initialize finish process models
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

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FTDViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ReloadCreateDropdowns();
                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -------------------------------------------------
                // SAVE FTD MASTER
                // -------------------------------------------------

                var master = model.TblFtdMaster;
                master.Trdate = DateOnly.FromDateTime(DateTime.Now);
                _context.TblFtdMasters.Add(master);
                await _context.SaveChangesAsync();

                // Database-generated FTDID
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
                    foreach (var item in model.TblFtdKnit)
                    {
                        item.Ftdid = ftdId;
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
                        // Set Ftdid here if this entity has that property.
                        item.Ftdid = ftdId;

                        _context.TblFtdDyeingBasics.Add(item);
                    }
                }

                // SAVE LAB DETAILS
                if (model.TblLabDetails != null)
                {
                    model.TblLabDetails.Ftdid = ftdId;

                    _context.TblLabDetails.Add(
                        model.TblLabDetails);
                }

                // -------------------------------------------------
                // SAVE DYEING RECIPE DETAILS
                // -------------------------------------------------

                if (model.TblFtdDyeingRecipe != null)
                {
                    foreach (var item in model.TblFtdDyeingRecipe)
                    {
                        // Set Ftdid here if this entity has that property.
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

                    _context.TblSqueezing.Add(
                        model.TblSqueezing);
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
                // SAVE Compacting Data
                // -------------------------------------------------

                if (model.TblCompacting != null)
                {
                    model.TblCompacting.Ftdid = ftdId;
                    _context.TblCompacting.Add(model.TblCompacting);
                }

                // -------------------------------------------------
                // SAVE Compacting Data
                // -------------------------------------------------

                if (model.TblOtherProcess != null)
                {
                    model.TblOtherProcess.Ftdid = ftdId;
                    _context.TblOtherProcess.Add(model.TblOtherProcess);
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
                // 17. ROLLBACK IF TRANSACTION IS ACTIVE
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
                // RETURN ERROR TO VIEW
                // -------------------------------------------------

                ModelState.AddModelError(
                    string.Empty,
                    "Failed to save FTD data. Check the application logs for details.");

                await ReloadCreateDropdowns();

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Get Master
            var master = await _context.TblFtdMasters
                .FirstOrDefaultAsync(x => x.Ftdid == id);

            if (master == null)
            {
                return NotFound();
            }

            // 2. Initialize ViewModel with existing data
            var model = new FTDViewModel
            {
                TblFtdMaster = master,

                TblFtdFabric = await _context.TblFtdFabrics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                TblFtdYarn = await _context.TblFtdYarns
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                TblFtdKnit = await _context.TblFtdKnits
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                TblFtdDyeingBasic = await _context.TblFtdDyeingBasics
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                TblFtdDyeingRecipe = await _context.TblFtdDyeingRecipes
                    .Where(x => x.Ftdid == id)
                    .ToListAsync(),

                TblFinishProcessHeatset =
                    await _context.TblFinishProcessHeatset
                        .FirstOrDefaultAsync(x => x.Ftdid == id)
                    ?? new TblFinishProcessHeatset(),

                TblFinishProcessStenteringAb =
                    await _context.TblFinishProcessStenteringAb
                        .FirstOrDefaultAsync(x => x.Ftdid == id)
                    ?? new TblFinishProcessStenteringAb(),

                TblFinishProcessStenteringBb =
                    await _context.TblFinishProcessStenteringBb
                        .FirstOrDefaultAsync(x => x.Ftdid == id)
                    ?? new TblFinishProcessStenteringBb(),

                TblSingeing = await _context.TblSingeing
                    .FirstOrDefaultAsync(x => x.Ftdid == id)
                    ?? new TblSingeing(),

                TblSqueezing = await _context.TblSqueezing
                    .FirstOrDefaultAsync(x => x.Ftdid == id)
                    ?? new TblSqueezing(),

                TblBrush = await _context.TblBrushe
                    .FirstOrDefaultAsync(x => x.Ftdid == id)
                    ?? new TblBrush()
            };

            // 3. Load Control / Batch dropdown
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
            if (model.TblFtdMaster == null ||
                id != model.TblFtdMaster.Ftdid)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await ReloadCreateDropdowns();
                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Update Master
                var master = await _context.TblFtdMasters
                    .FirstOrDefaultAsync(x => x.Ftdid == id);

                if (master == null)
                {
                    return NotFound();
                }

                CopyNonKeyValues(master, model.TblFtdMaster);

                // 2. Update / Insert Fabric
                if (model.TblFtdFabric != null)
                {
                    foreach (var item in model.TblFtdFabric)
                    {
                        item.Ftdid = id;

                        if (item.Trid == 0)
                        {
                            _context.TblFtdFabrics.Add(item);
                        }
                        else
                        {
                            var existing = await _context.TblFtdFabrics
                                .FirstOrDefaultAsync(x =>
                                    x.Trid == item.Trid && x.Ftdid == id);

                            if (existing == null)
                                throw new InvalidOperationException(
                                    $"Fabric TRID {item.Trid} not found.");

                            CopyNonKeyValues(existing, item);
                        }
                    }
                }

                // 3. Update / Insert Yarn
                if (model.TblFtdYarn != null)
                {
                    foreach (var item in model.TblFtdYarn)
                    {
                        item.Ftdid = id;

                        if (item.Trid == 0)
                        {
                            _context.TblFtdYarns.Add(item);
                        }
                        else
                        {
                            var existing = await _context.TblFtdYarns
                                .FirstOrDefaultAsync(x =>
                                    x.Trid == item.Trid && x.Ftdid == id);

                            if (existing == null)
                                throw new InvalidOperationException(
                                    $"Yarn TRID {item.Trid} not found.");

                            CopyNonKeyValues(existing, item);
                        }
                    }
                }

                // 4. Update / Insert Knit
                if (model.TblFtdKnit != null)
                {
                    foreach (var item in model.TblFtdKnit)
                    {
                        item.Ftdid = id;

                        // GsPhotoContentType is nullable.
                        // Do not force it to an empty string.

                        if (item.Trid == 0)
                        {
                            _context.TblFtdKnits.Add(item);
                        }
                        else
                        {
                            var existing = await _context.TblFtdKnits
                                .FirstOrDefaultAsync(x =>
                                    x.Trid == item.Trid && x.Ftdid == id);

                            if (existing == null)
                                throw new InvalidOperationException(
                                    $"Knit TRID {item.Trid} not found.");

                            CopyNonKeyValues(existing, item);
                        }
                    }
                }

                // 5. Update / Insert Dyeing Basic
                if (model.TblFtdDyeingBasic != null)
                {
                    foreach (var item in model.TblFtdDyeingBasic)
                    {

                        if (item.Trid == 0)
                        {
                            _context.TblFtdDyeingBasics.Add(item);
                        }
                        else
                        {
                            var existing = await _context.TblFtdDyeingBasics
                                .FirstOrDefaultAsync(x =>
                                    x.Trid == item.Trid);

                            if (existing == null)
                                throw new InvalidOperationException(
                                    $"Dyeing Basic TRID {item.Trid} not found.");

                            CopyNonKeyValues(existing, item);
                        }
                    }
                }

                // 6. Update / Insert Dyeing Recipe
                if (model.TblFtdDyeingRecipe != null)
                {
                    foreach (var item in model.TblFtdDyeingRecipe)
                    {
                        if (item.Trid == 0)
                        {
                            _context.TblFtdDyeingRecipes.Add(item);
                        }
                        else
                        {
                            var existing = await _context.TblFtdDyeingRecipes
                                .FirstOrDefaultAsync(x =>
                                    x.Trid == item.Trid);

                            if (existing == null)
                                throw new InvalidOperationException(
                                    $"Dyeing Recipe TRID {item.Trid} not found.");

                            CopyNonKeyValues(existing, item);
                        }
                    }
                }

                // 7. Update / Insert Heatset
                await SaveSingleChild(
                    id,
                    model.TblFinishProcessHeatset,
                    _context.TblFinishProcessHeatset);

                // 8. Update / Insert Stentering A/B
                await SaveSingleChild(
                    id,
                    model.TblFinishProcessStenteringAb,
                    _context.TblFinishProcessStenteringAb);

                // 9. Update / Insert Stentering B/B
                await SaveSingleChild(
                    id,
                    model.TblFinishProcessStenteringBb,
                    _context.TblFinishProcessStenteringBb);

                // 10. Update / Insert Singeing
                await SaveSingleChild(
                    id,
                    model.TblSingeing,
                    _context.TblSingeing);

                // 11. Update / Insert Squeezing
                await SaveSingleChild(
                    id,
                    model.TblSqueezing,
                    _context.TblSqueezing);

                // 12. Update / Insert Brush
                await SaveSingleChild(
                    id,
                    model.TblBrush,
                    _context.TblBrushe);

                // 13. Save all changes
                await _context.SaveChangesAsync();

                // 14. Commit
                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "FTD updated successfully.";

                return RedirectToAction(nameof(FTDList));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Failed to update FTD ID: {FtdId}",
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "Failed to update FTD. Check application logs.");

                await ReloadCreateDropdowns();
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
                        internalRef = master.InternalRef,
                        styleRefNo = master.StyleRefNo,
                        sysReq = master.SysReq,
                        buyerName = master.BuyerName,
                        bookingNo = master.BookingNo,
                        batchNo = master.BatchNo
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
                        brand = x.Brand
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
            public string? CompanyName { get; set; }

            public string? InternalRef { get; set; }

            public string? StyleRefNo { get; set; }

            public string? SysReq { get; set; }

            public string? BuyerName { get; set; }

            public string? BookingNo { get; set; }

            public string? BatchNo { get; set; }
        }


        // =========================================================
        // FABRIC DTO
        // =========================================================

        public class FTDFabricDto
        {
            public string? BookingNo { get; set; }

            public string? FabricDescription { get; set; }

            public string? Construction { get; set; }

            public decimal? ColorTypeId { get; set; }

            public string? ColorName { get; set; }

            public decimal? GsmWeight { get; set; }

            public string? Dia { get; set; }

            public decimal? DiaWidth { get; set; }

            public decimal? DtlsId { get; set; }
        }

        public class FTDYarnDto
        {
            public string? Count { get; set; }
            public string? Lot { get; set; }
            public string? Brand { get; set; }
        }

        public class FTDKnitDto
        {
            public string? PROGRAM_NO { get; set; }

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
    }
}
