using EmployeeContract.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContract.Repository
{
    public partial class ApplicationsDBContext : DbContext
    {
        public ApplicationsDBContext(DbContextOptions
        <ApplicationsDBContext> options)
            : base(options)
        {
        }
        public virtual DbSet<PositionsModel> Positions { get; set; }
        public virtual DbSet<BranchModel> Branches { get; set; }
        public virtual DbSet<EmployeeModel> Employees { get; set; }
        public virtual DbSet<DetailEmployee> DetailEmployees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetailEmployee>()
               .HasOne(de => de.Position)
               .WithMany(p => p.DetailEmployees)
               .HasForeignKey(de => de.PositionId);

            modelBuilder.Entity<DetailEmployee>()
                .HasOne(de => de.Branch)
                .WithMany(b => b.DetailEmployees)
                .HasForeignKey(de => de.BranchId);

            modelBuilder.Entity<DetailEmployee>()
                .HasOne(de => de.Employee)
                .WithMany(e => e.DetailEmployees)
                .HasForeignKey(de => de.EmployeeId);
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
