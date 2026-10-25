//namespace FabricDevelopmentHub.Data
//{
//    public class ApplicationDbContext
//    {
//    }
//}
using FabricDevelopmentHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FabricDevelopmentHub.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================
        // DbSets
        // =========================
        public virtual DbSet<TblUserPermission> TblUserPermission { get; set; }
        public virtual DbSet<TblModule> TblModule { get; set; }
        public virtual DbSet<TblMenu> TblMenu { get; set; }
        public virtual DbSet<TblPermissionAction> TblPermissionAction { get; set; }
        public virtual DbSet<TblCompanyInfo> TblCompanyInfo { get; set; }
        public virtual DbSet<TblFtdFabric> TblFtdFabrics { get; set; }
        public virtual DbSet<TblFtdKnit> TblFtdKnits { get; set; }
        public virtual DbSet<TblFtdMaster> TblFtdMasters { get; set; }
        public virtual DbSet<TblFtdYarn> TblFtdYarns { get; set; }
        public virtual DbSet<TblFtdDyeingBasic> TblFtdDyeingBasics { get; set; }
        public virtual DbSet<TblFtdDyeingRecipe> TblFtdDyeingRecipes { get; set; }
        public virtual DbSet<TblFinishProcessHeatset> TblFinishProcessHeatset { get; set; }
        public virtual DbSet<TblFinishProcessStenteringAb> TblFinishProcessStenteringAb { get; set; }
        public virtual DbSet<TblFinishProcessStenteringBb> TblFinishProcessStenteringBb { get; set; }
        public virtual DbSet<TblSingeing> TblSingeing { get; set; }
        public virtual DbSet<TblSqueezing> TblSqueezing { get; set; }
        public virtual DbSet<TblBrush> TblBrushe { get; set; }
        public virtual DbSet<TblCompacting> TblCompacting { get; set; }
        public virtual DbSet<TblOtherProcess> TblOtherProcess { get; set; }
        public virtual DbSet<TblLabDetails> TblLabDetails { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TblUserPermissions
            // =========================

            modelBuilder.Entity<TblUserPermission>(entity =>
            {
                entity.HasKey(e => e.UserPermissionId)
                    .HasName("PK_UserPermissions");

                entity.ToTable("UserPermissions");

                entity.Property(e => e.UserId)
                    .HasColumnType("nvarchar(50)");

                entity.Property(e => e.IsAllowed)
                    .HasDefaultValue(true);

                // 👇 NEW ADDITIONS (RELATIONS)

                entity.HasOne(x => x.Module)
                    .WithMany()
                    .HasForeignKey(x => x.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Menu)
                    .WithMany()
                    .HasForeignKey(x => x.MenuId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Action)
                    .WithMany()
                    .HasForeignKey(x => x.ActionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =========================
            // Modules
            // =========================

            modelBuilder.Entity<TblModule>(entity =>
            {
                entity.HasKey(e => e.ModuleId);

                entity.ToTable("Modules");

                entity.Property(e => e.ModuleName)
                    .HasMaxLength(100)
                    .IsRequired();
            });


            // =========================
            // Menus
            // =========================

            modelBuilder.Entity<TblMenu>(entity =>
            {
                entity.HasKey(e => e.MenuId);

                entity.ToTable("Menus");

                entity.Property(e => e.MenuName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Url)
                    .HasMaxLength(300);

                entity.HasOne<TblModule>()
                    .WithMany()
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // =========================
            // PermissionActions
            // =========================

            modelBuilder.Entity<TblPermissionAction>(entity =>
            {
                entity.HasKey(e => e.ActionId);

                entity.ToTable("PermissionActions");

                entity.Property(e => e.ActionName)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // =========================
            // TblCompanyInfo
            modelBuilder.Entity<TblCompanyInfo>(entity =>
            {
                entity.HasKey(e => e.Comid);

                entity.ToTable("TBL_COMPANY_INFO");

                entity.Property(e => e.Comid).HasColumnName("COMID");
                entity.Property(e => e.ComAddress)
                    .HasMaxLength(250)
                    .HasColumnName("COM_ADDRESS");
                entity.Property(e => e.ComContact)
                    .HasMaxLength(50)
                    .HasColumnName("COM_CONTACT");
                entity.Property(e => e.ComEmail)
                    .HasMaxLength(50)
                    .HasColumnName("COM_EMAIL");
                entity.Property(e => e.ComName)
                    .HasMaxLength(50)
                    .HasColumnName("COM_NAME");
                entity.Property(e => e.Remarks)
                    .HasMaxLength(50)
                    .HasColumnName("REMARKS");
                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasColumnName("STATUS");
            });

            modelBuilder.Entity<TblFtdFabric>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FTD_FABRIC");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.Color)
                    .HasMaxLength(50)
                    .HasColumnName("COLOR");
                entity.Property(e => e.ColorType)
                    .HasMaxLength(50)
                    .HasColumnName("COLOR_TYPE");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CREATED_BY");
                entity.Property(e => e.DiaType)
                    .HasMaxLength(50)
                    .HasColumnName("DIA_TYPE");
                entity.Property(e => e.Fabrication)
                    .HasMaxLength(250)
                    .HasColumnName("FABRICATION");
                entity.Property(e => e.FinDia).HasColumnName("FIN_DIA");
                entity.Property(e => e.FinDiaType).HasMaxLength(50).HasColumnName("FIN_DIA_TYPE");
                entity.Property(e => e.FinGsm).HasColumnName("FIN_GSM");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.ReqDia).HasMaxLength(50).HasColumnName("REQ_DIA");
                entity.Property(e => e.ReqGsm).HasColumnName("REQ_GSM");
                entity.Property(e => e.Trdate).HasColumnName("TRDATE");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("UPDATED_AT");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("UPDATED_BY");

                entity.HasOne(d => d.Ftd).WithMany(p => p.TblFtdFabrics)
                    .HasForeignKey(d => d.Ftdid)
                    .HasConstraintName("FK_TBL_FTD_FABRIC_TBL_FTD_MASTER");
            });

            modelBuilder.Entity<TblFtdKnit>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FTD_KNIT");

                entity.Property(e => e.Trid).ValueGeneratedOnAdd().HasColumnName("TRID");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CREATED_BY");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.GsPhoto).HasColumnName("GS_PHOTO");
                entity.Property(e => e.GsPhotoContentType)
                    .HasMaxLength(50)
                    .HasColumnName("GS_PHOTO_CONTENT_TYPE");
                entity.Property(e => e.KnitCom)
                    .HasMaxLength(50)
                    .HasColumnName("KNIT_COM");
                entity.Property(e => e.McBrand)
                    .HasMaxLength(50)
                    .HasColumnName("MC_BRAND");
                entity.Property(e => e.McDia).HasMaxLength(50).HasColumnName("MC_DIA");
                entity.Property(e => e.McGauge).HasMaxLength(50).HasColumnName("MC_GAUGE");
                entity.Property(e => e.McNo)
                    .HasMaxLength(50)
                    .HasColumnName("MC_NO");
                entity.Property(e => e.Rpm).HasColumnName("RPM");
                entity.Property(e => e.Sl)
                    .HasMaxLength(50)
                    .HasColumnName("SL");
                entity.Property(e => e.Source)
                    .HasMaxLength(50)
                    .HasColumnName("SOURCE");
                entity.Property(e => e.StripeMeasure).HasMaxLength(50).HasColumnName("STRIPE_MEASURE");
                entity.Property(e => e.ProgramNo).HasMaxLength(50).HasColumnName("PROGRAM_NO");
                entity.Property(e => e.ColorName).HasMaxLength(50).HasColumnName("COLOR_NAME");
                entity.Property(e => e.Measurement).HasMaxLength(50).HasColumnName("MASUREMENT");
                entity.Property(e => e.Uom).HasMaxLength(50).HasColumnName("UOM");
                entity.Property(e => e.FeederNo).HasMaxLength(50).HasColumnName("FEEDER_NO");
                entity.Property(e => e.Trdate).HasColumnName("TRDATE");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("UPDATED_AT");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("UPDATED_BY");
                entity.Property(e => e.YarnPhoto).HasColumnName("YARN_PHOTO");
                entity.Property(e => e.YarnPhotoContentType)
                    .HasMaxLength(50)
                    .HasColumnName("YARN_PHOTO_CONTENT_TYPE");

                entity.HasOne(d => d.Ftd).WithMany(p => p.TblFtdKnits)
                    .HasForeignKey(d => d.Ftdid)
                    .HasConstraintName("FK_TBL_FTD_KNIT_TBL_FTD_MASTER");
            });

            modelBuilder.Entity<TblFtdMaster>(entity =>
            {
                entity.HasKey(e => e.Ftdid);

                entity.ToTable("TBL_FTD_MASTER");

                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.BatchNo)
                    .HasMaxLength(50)
                    .HasColumnName("BATCH_NO");
                entity.Property(e => e.Buyer)
                    .HasMaxLength(50)
                    .HasColumnName("BUYER");
                entity.Property(e => e.Company)
                    .HasMaxLength(50)
                    .HasColumnName("COMPANY");
                entity.Property(e => e.Control)
                    .HasMaxLength(250)
                    .HasColumnName("CONTROL");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CREATED_BY");
                entity.Property(e => e.FtdNo).HasMaxLength(50).HasColumnName("FTD_NO");
                entity.Property(e => e.StyleRef).HasMaxLength(50).HasColumnName("STYLE_REF");
                entity.Property(e => e.SysReq).HasMaxLength(50).HasColumnName("SYS_REQ");
                entity.Property(e => e.Trdate).HasColumnName("TRDATE");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("UPDATED_AT");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("UPDATED_BY");
            });

            modelBuilder.Entity<TblFtdYarn>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FTD_YARN");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.Brand)
                    .HasMaxLength(50)
                    .HasColumnName("BRAND");
                entity.Property(e => e.Count)
                    .HasMaxLength(100)
                    .HasColumnName("COUNT");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CREATED_BY");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.LotNo)
                    .HasMaxLength(50)
                    .HasColumnName("LOT_NO");
                entity.Property(e => e.Remarks).HasMaxLength(50).HasColumnName("REMARKS");
                entity.Property(e => e.Construction).HasMaxLength(50).HasColumnName("CONSTRUCTION");
                entity.Property(e => e.Std)
                    .HasMaxLength(50)
                    .HasColumnName("STD");
                entity.Property(e => e.Tpi)
                    .HasMaxLength(50)
                    .HasColumnName("TPI");
                entity.Property(e => e.Trdate).HasColumnName("TRDATE");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("UPDATED_AT");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("UPDATED_BY");

                entity.HasOne(d => d.Ftd).WithMany(p => p.TblFtdYarns)
                    .HasForeignKey(d => d.Ftdid)
                    .HasConstraintName("FK_TBL_FTD_YARN_TBL_FTD_YARN");
            });

            modelBuilder.Entity<TblFtdDyeingBasic>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FTD_DYEING_BASIC");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.BatchNo)
                    .HasMaxLength(50)
                    .HasColumnName("BATCH_NO");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CREATED_BY");
                entity.Property(e => e.DyeingPart)
                    .HasMaxLength(50)
                    .HasColumnName("DYEING_PART");
                entity.Property(e => e.EnzayemPer).HasColumnName("ENZAYEM_PER");
                entity.Property(e => e.InhouseDyeing).HasColumnName("INHOUSE_DYEING");
                entity.Property(e => e.IsNonRft)
                    .HasMaxLength(50)
                    .HasColumnName("IS_NON_RFT");
                entity.Property(e => e.MachineNo)
                    .HasMaxLength(50)
                    .HasColumnName("MACHINE_NO");
                entity.Property(e => e.ProductName).HasMaxLength(50).HasColumnName("PRODUCT_NAME");
                entity.Property(e => e.Shade)
                    .HasMaxLength(50)
                    .HasColumnName("SHADE");
                entity.Property(e => e.ShadeP).HasColumnName("SHADE_P");
                entity.Property(e => e.Trdate).HasColumnName("TRDATE");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("UPDATED_AT");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("UPDATED_BY");
            });

            modelBuilder.Entity<TblFtdDyeingRecipe>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FTD_DYEING_RECIPE");

                entity.Property(e => e.Trid).ValueGeneratedOnAdd().HasColumnName("TRID");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.BatchNo)
                    .HasMaxLength(50)
                    .HasColumnName("BATCH_NO");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CREATED_BY");
                entity.Property(e => e.RecipeDetails)
                    .HasMaxLength(50)
                    .HasColumnName("RECIPE_DETAILS");
                entity.Property(e => e.Ratio).HasColumnName("RATIO");
                entity.Property(e => e.Trdate).HasColumnName("TRDATE");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("UPDATED_AT");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("UPDATED_BY");
            });

            //TblFinishProcessHeatset
            modelBuilder.Entity<TblFinishProcessHeatset>(entity =>
            {
                entity.HasKey(e => e.Trid);
                entity.ToTable("TBL_FINISH_PROCESS_HEATSET");
                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.AHeatsetDia).HasColumnName("A_HEATSET_DIA");
                entity.Property(e => e.AHeatsetGsm).HasColumnName("A_HEATSET_GSM");
                entity.Property(e => e.Blower).HasColumnName("BLOWER");
                entity.Property(e => e.Comments).HasMaxLength(500).HasColumnName("COMMENTS");
                entity.Property(e => e.DweelTime).HasColumnName("DWEEL_TIME"); 
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.GreyDia).HasColumnName("GREY_DIA");
                entity.Property(e => e.GreyGsm).HasColumnName("GREY_GSM");
                entity.Property(e => e.Noc).HasColumnName("NOC");
                entity.Property(e => e.Nop).HasColumnName("NOP");
                entity.Property(e => e.OFeed).HasColumnName("O_FEED");
                entity.Property(e => e.Pp).HasColumnName("PP");
                entity.Property(e => e.Speed).HasColumnName("SPEED");
                entity.Property(e => e.Stretching).HasColumnName("STRETCHING");
                entity.Property(e => e.Temp).HasColumnName("TEMP");
                entity.Property(e => e.UFeed).HasColumnName("U_FEED");
            });

            modelBuilder.Entity<TblFinishProcessStenteringAb>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FINISH_PROCESS_STENTERING_AB");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.ADia).HasColumnName("A_DIA");
                entity.Property(e => e.AGsm).HasColumnName("A_GSM");
                entity.Property(e => e.AShrinkage).HasColumnName("A_SHRINKAGE");
                entity.Property(e => e.BlowerSpeed).HasColumnName("BLOWER_SPEED");
                entity.Property(e => e.Chemical).HasColumnName("CHEMICAL");
                entity.Property(e => e.Comments).HasMaxLength(50).HasColumnName("COMMENTS");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.Noc).HasColumnName("NOC");
                entity.Property(e => e.OFeed).HasColumnName("O_FEED");
                entity.Property(e => e.PPressure1).HasColumnName("P_PRESSURE_1");
                entity.Property(e => e.PPressure2).HasColumnName("P_PRESSURE_2");
                entity.Property(e => e.Speed).HasColumnName("SPEED");
                entity.Property(e => e.StretchPer).HasColumnName("STRETCH_PER");
                entity.Property(e => e.Temp).HasColumnName("TEMP");
            });

            modelBuilder.Entity<TblFinishProcessStenteringBb>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_FINISH_PROCESS_STENTERING_BB");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.ADia).HasColumnName("A_DIA");
                entity.Property(e => e.AGsm).HasColumnName("A_GSM");
                entity.Property(e => e.AShrinkage).HasColumnName("A_SHRINKAGE");
                entity.Property(e => e.BlowerSpeed).HasColumnName("BLOWER_SPEED");
                entity.Property(e => e.Chemical).HasColumnName("CHEMICAL");
                entity.Property(e => e.Comments).HasMaxLength(50).HasColumnName("COMMENTS");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.Noc).HasColumnName("NOC");
                entity.Property(e => e.OFeed).HasColumnName("O_FEED");
                entity.Property(e => e.PPressure1).HasColumnName("P_PRESSURE_1");
                entity.Property(e => e.PPressure2).HasColumnName("P_PRESSURE_2");
                entity.Property(e => e.Speed).HasColumnName("SPEED");
                entity.Property(e => e.StretchPer).HasColumnName("STRETCH_PER");
                entity.Property(e => e.Temp).HasColumnName("TEMP");
            });
            modelBuilder.Entity<TblSingeing>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_SINGEING");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.ASingeingDia).HasColumnName("A_SINGEING_DIA");
                entity.Property(e => e.ASingeingGsm).HasColumnName("A_SINGEING_GSM");
                entity.Property(e => e.BurnerTemp).HasColumnName("BURNER_TEMP");
                entity.Property(e => e.FlameHeight).HasColumnName("FLAME_HEIGHT");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.Speed).HasColumnName("SPEED");
            });

            modelBuilder.Entity<TblSqueezing>(entity =>
            {
                entity.HasKey(e => e.Trid);

                entity.ToTable("TBL_SQUEEZING");

                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.DiaType)
                    .HasMaxLength(50)
                    .HasColumnName("DIA_TYPE");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.Pp).HasColumnName("PP");
            });
            modelBuilder.Entity<TblBrush>(entity =>
            {
                entity.HasKey(e => e.Trid);
                entity.ToTable("TBL_BRUSH");
                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.ADia).HasColumnName("A_DIA");
                entity.Property(e => e.AGsm).HasColumnName("A_GSM");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.LCp).HasColumnName("L_CP");
                entity.Property(e => e.LDrumSpeed).HasColumnName("L_DRUM_SPEED");
                entity.Property(e => e.LPile).HasColumnName("L_PILE");
                entity.Property(e => e.LTension).HasColumnName("L_TENSION");
                entity.Property(e => e.McSpeed).HasColumnName("MC_SPEED");
                entity.Property(e => e.UCp).HasColumnName("U_CP");
                entity.Property(e => e.UDrumSpeed).HasColumnName("U_DRUM_SPEED");
                entity.Property(e => e.UPile).HasColumnName("U_PILE");
                entity.Property(e => e.UTension).HasColumnName("U_TENSION");
            });

            modelBuilder.Entity<TblCompacting>(entity =>
            {
                entity.HasKey(e => e.Trid);
                entity.ToTable("TBL_COMPACTING");
                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.ADia).HasColumnName("A_DIA");
                entity.Property(e => e.AGsm).HasColumnName("A_GSM");
                entity.Property(e => e.Comments).HasMaxLength(500).HasColumnName("COMMENTS");
                entity.Property(e => e.CompactionPer).HasColumnName("COMPACTION_PER");
                entity.Property(e => e.FGsm).HasColumnName("F_GSM");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.OFeed).HasColumnName("O_FEED");
                entity.Property(e => e.RDia).HasColumnName("R_DIA");
                entity.Property(e => e.RGsm).HasColumnName("R_GSM");
                entity.Property(e => e.Shrinkage).HasColumnName("SHRINKAGE");
                entity.Property(e => e.Speed).HasColumnName("SPEED");
                entity.Property(e => e.Steam).HasColumnName("STEAM");
                entity.Property(e => e.Temp).HasColumnName("TEMP");
            });

            modelBuilder.Entity<TblOtherProcess>(entity =>
            {
                entity.HasKey(e => e.Trid);
                entity.ToTable("TBL_OTHER_PROCESS");
                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.AopRemarks).HasMaxLength(50).HasColumnName("AOP_REMARKS");
                entity.Property(e => e.AopType).HasMaxLength(50).HasColumnName("AOP_TYPE");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.GmtColorAw).HasMaxLength(50).HasColumnName("GMT_COLOR_AW");
                entity.Property(e => e.GmtColorBw).HasMaxLength(50).HasColumnName("GMT_COLOR_BW");
                entity.Property(e => e.ImageP).HasMaxLength(50).HasColumnName("IMAGE_P");
                entity.Property(e => e.ImageW).HasMaxLength(50).HasColumnName("IMAGE_W");
                entity.Property(e => e.NoOfColorP).HasColumnName("NO_OF_COLOR_P");
                entity.Property(e => e.NoOfColorW).HasColumnName("NO_OF_COLOR_W");
                entity.Property(e => e.PrintRemarks).HasMaxLength(50).HasColumnName("PRINT_REMARKS");
                entity.Property(e => e.PrintType).HasMaxLength(50).HasColumnName("PRINT_TYPE");
                entity.Property(e => e.RepeatSizeP).HasMaxLength(50).HasColumnName("REPEAT_SIZE_P");
                entity.Property(e => e.RepeatSizeW).HasMaxLength(50).HasColumnName("REPEAT_SIZE_W");
                entity.Property(e => e.WashRemarks).HasMaxLength(500).HasColumnName("WASH_REMARKS");
                entity.Property(e => e.WashType).HasMaxLength(50).HasColumnName("WASH_TYPE");
            });

            modelBuilder.Entity<TblLabDetails>(entity =>
            {
                entity.HasKey(e => e.Trid);
                entity.ToTable("TBL_LAB_DETAILS");
                entity.Property(e => e.Trid).HasColumnName("TRID");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.LabDipNo).HasMaxLength(50).HasColumnName("LAB_DIP_NO");
                entity.Property(e => e.LabProductName).HasMaxLength(50).HasColumnName("LAB_PRODUCT_NAME");
                entity.Property(e => e.Ratio).HasColumnName("RATIO");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}