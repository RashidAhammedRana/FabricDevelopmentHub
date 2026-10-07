using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;
using FabricDevelopmentHub.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FabricDevelopmentHub.Controllers
{
    public class FTDController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FTDController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public IActionResult Index()
        {
            return View();
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new FTDViewModel();

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


        


    }
}
