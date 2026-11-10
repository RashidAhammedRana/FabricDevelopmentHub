using FabricDevelopmentHub.Models;

namespace FabricDevelopmentHub.ViewModels
{
    public class FTDViewModel
    {
        public int Ltgid { get; set; }
        public int Ltnid { get; set; }
        public int Ltmid { get; set; }

        public string? Remarks { get; set; }

        public List<LabTestResultEntry> Results { get; set; }= new List<LabTestResultEntry>();

        public TblFtdMaster TblFtdMaster { get; set; } = new();
        public TblFinishProcessHeatset TblFinishProcessHeatset { get; set; } = new();
        public TblFinishProcessStenteringAb TblFinishProcessStenteringAb { get; set; } = new();
        public TblFinishProcessStenteringBb TblFinishProcessStenteringBb { get; set; } = new();
        public TblSingeing TblSingeing { get; set; } = new();
        public TblSqueezing TblSqueezing { get; set; } = new();
        public TblBrush TblBrush { get; set; } = new();
        public TblCompacting TblCompacting { get; set; } = new();
        public TblOtherProcess TblOtherProcess { get; set; } = new();
        public TblLabTestGroup TblLabTestGroup { get; set; } = new();
        public TblLabTestMethod TblLabTestMethod { get; set; } = new();
        public TblLabTestName TblLabTestName { get; set; } = new();
        //public TblLabTestParameter TblLabTestParameter { get; set; } = new();
        //public TblLabTestResult TblLabTestResult { get; set; } = new();



        //public TblLabDetails TblLabDetails { get; set; } = new();
        public DateTime Date { get; set; } = DateTime.Today;
        public List<IFormFile?> FtdKnitPhotos { get; set; } = new();


        public List<TblFtdFabric> TblFtdFabric { get; set; } = new();
        public List<TblFtdKnit> TblFtdKnit { get; set; } = new();

        public List<TblFtdYarn> TblFtdYarn { get; set; } = new();
        public List<TblFtdDyeingBasic> TblFtdDyeingBasic { get; set; } = new();
        public List<TblFtdDyeingRecipe> TblFtdDyeingRecipe { get; set; } = new();
        public List<TblLabDetails> TblLabDetails { get; set; } = new();



    }
}