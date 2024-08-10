using EmployeeContract.DTO.Response;
using Dapper;
using EmployeeContract.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        public virtual DbSet<FileListModel> FileLists { get; set; }
        public virtual DbSet<DetailEmployeeModel> DetailEmployees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetailEmployeeModel>()
               .HasOne(de => de.Position)
               .WithMany(p => p.DetailEmployees)
               .HasForeignKey(de => de.PositionId);

            modelBuilder.Entity<DetailEmployeeModel>()
                .HasOne(de => de.Branch)
                .WithMany(b => b.DetailEmployees)
                .HasForeignKey(de => de.BranchId);

            modelBuilder.Entity<DetailEmployeeModel>()
                .HasOne(de => de.Employee)
                .WithMany(e => e.DetailEmployees)
                .HasForeignKey(de => de.EmployeeId);
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public async Task<List<DetailEmployeeResponse>> GetAllEmployeesWithDetailsAsync()
        {
            using var connection = Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var result = await connection.QueryAsync<DetailEmployeeResponse>(
                "GetAllEmployeesWithDetails",
                commandType: CommandType.StoredProcedure
            );


            return result.ToList();
        }
    }
}
