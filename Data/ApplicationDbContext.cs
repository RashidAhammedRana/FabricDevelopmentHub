//namespace FabricDevelopmentHub.Data
//{
//    public class ApplicationDbContext
//    {
//    }
//}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Models;

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

        public virtual DbSet<TblCompanyInfo> TblCompanyInfo { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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



            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}