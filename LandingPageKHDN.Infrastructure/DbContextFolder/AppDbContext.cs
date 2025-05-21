using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandingPageKHDN.Domain.Entities;

namespace LandingPageKHDN.Infrastructure.DbContextFolder
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActionLog> ActionLogs { get; set; }

        public virtual DbSet<AdminAccount> AdminAccounts { get; set; }

        public virtual DbSet<CompanyRegistration> CompanyRegistrations { get; set; }

        //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //        => optionsBuilder.UseSqlServer("Server=KienLongBankDB.mssql.somee.com;Database=KienLongBankDB;User Id=daigiakien02_SQLLogin_1;Password=2yvjlvxh2t;TrustServerCertificate=True;");
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActionLog>(entity =>
            {
                entity.ToTable("ActionLog");

                entity.Property(e => e.Action).HasMaxLength(500);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Admin).WithMany(p => p.ActionLogs)
                    .HasForeignKey(d => d.AdminId)
                    .HasConstraintName("FK_ActionLog_Admin");
            });

            modelBuilder.Entity<AdminAccount>(entity =>
            {
                entity.ToTable("AdminAccount");

                entity.HasIndex(e => e.Username, "UQ_AdminAccount_Username").IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Username).HasMaxLength(50);
            });

            modelBuilder.Entity<CompanyRegistration>(entity =>
            {
                entity.ToTable("CompanyRegistration");

                entity.HasIndex(e => e.TaxCode, "UQ_CompanyRegistration_TaxCode").IsUnique();

                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.BusinessLicenseFilePath).HasMaxLength(500);
                entity.Property(e => e.CompanyName).HasMaxLength(255);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.LegalRepId)
                    .HasMaxLength(50)
                    .HasColumnName("LegalRepID");
                entity.Property(e => e.LegalRepIdfilePath)
                    .HasMaxLength(500)
                    .HasColumnName("LegalRepIDFilePath");
                entity.Property(e => e.LegalRepName).HasMaxLength(100);
                entity.Property(e => e.LegalRepPosition).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.TaxCode).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
