using FabricDevelopmentHub.Models;

namespace FabricDevelopmentHub.ViewModels
{
    public class FTDViewModel
    {
        // ==========================
        // MASTER
        // ==========================

        public TblFtdMaster TblFtdMaster { get; set; } = new();


        // ==========================
        // TRANSACTION DATE
        // ==========================

        public DateTime Date { get; set; } = DateTime.Today;


        // ==========================
        // FABRIC
        // ==========================

        public List<TblFtdFabric> TblFtdFabric { get; set; } = new();
        // FABRIC
        //public TblFtdFabric TblFtdFabric { get; set; } = new();


        // ==========================
        // KNIT
        // ==========================

        public List<TblFtdKnit> TblFtdKnit { get; set; } = new();


        // ==========================
        // YARN
        // ==========================

        public List<TblFtdYarn> TblFtdYarn { get; set; } = new();
        public List<TblFtdDyeingBasic> TblFtdDyeingBasic { get; set; } = new();
        public List<TblFtdDyeingRecipe> TblFtdDyeingRecipe { get; set; } = new();

    }
}