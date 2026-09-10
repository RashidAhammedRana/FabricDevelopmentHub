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
                entity.Property(e => e.FinDiaType)
                    .HasMaxLength(50)
                    .HasColumnName("FIN_DIA_TYPE");
                entity.Property(e => e.FinGsm).HasColumnName("FIN_GSM");
                entity.Property(e => e.Ftdid).HasColumnName("FTDID");
                entity.Property(e => e.ReqDia).HasColumnName("REQ_DIA");
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

                entity.Property(e => e.Trid)
                    .ValueGeneratedNever()
                    .HasColumnName("TRID");
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
                entity.Property(e => e.McDia).HasColumnName("MC_DIA");
                entity.Property(e => e.McGauge).HasColumnName("MC_GAUGE");
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
                entity.Property(e => e.StripeMeasure)
                    .HasMaxLength(50)
                    .HasColumnName("STRIPE_MEASURE");
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
                entity.Property(e => e.FtdNo)
                    .HasMaxLength(50)
                    .HasColumnName("FTD_NO");
                entity.Property(e => e.StyleRef)
                    .HasMaxLength(50)
                    .HasColumnName("STYLE_REF");
                entity.Property(e => e.SysReq).HasColumnName("SYS_REQ");
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
                    .HasMaxLength(50)
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
                entity.Property(e => e.Remarks)
                    .HasMaxLength(50)
                    .HasColumnName("REMARKS");
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}