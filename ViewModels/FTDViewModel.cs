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

        public List<TblFtdFabric> TblFtdFabrics { get; set; } = new();


        // ==========================
        // KNIT
        // ==========================

        public List<TblFtdKnit> TblFtdKnits { get; set; } = new();


        // ==========================
        // YARN
        // ==========================

        public List<TblFtdYarn> TblFtdYarns { get; set; } = new();
    }
}