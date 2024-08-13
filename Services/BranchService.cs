using Dapper;
using EmployeeContract.DTO.Response;
using EmployeeContract.Models;
using EmployeeContract.Repository;
using OfficeOpenXml;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;


namespace EmployeeContract.Services
{
    public interface IBranchService
    {
        Task<List<BranchResponse>> ImportBranchesAsync(Stream fileStream);
        Task<List<BranchResponse>> getAllBranches();
        Task<IActionResult> deleteBranchById(int id);
    }
    public class BranchService : IBranchService
    {
        private readonly ApplicationsDBContext _dbContext;

        public BranchService(ApplicationsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<BranchResponse>> ImportBranchesAsync(Stream fileStream)
        {
            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                var branches = new List<BranchModel>();

                for (int row = 2; row <= rowCount; row++)
                {
                    var branchCode = worksheet.Cells[row, 1].Value.ToString();
                    var branchName = worksheet.Cells[row, 2].Value.ToString();

                    var branch = new BranchModel
                    {
                        BranchCode = branchCode,
                        BranchName = branchName,
                        IsActive = true
                    };

                    branches.Add(branch);
                }

                

                _dbContext.Branches.AddRange(branches);
                await _dbContext.SaveChangesAsync();

                var responseList = branches.Select(b => new BranchResponse
                {
                    BranchId = b.BranchId,
                    BranchName = b.BranchName,
                    BranchCode = b.BranchCode,
                    IsActive = b.IsActive
                }).ToList();

                return responseList;
            }

        }

        public async Task<List<BranchResponse>> getAllBranches()
        {
            using var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            
            var result = await connection.QueryAsync<BranchResponse>(
                "getAllBranches",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<IActionResult> deleteBranchById(int id)
        {
            var branch = await _dbContext.Branches.FindAsync(id);

            if (branch == null)
            {
                return new NotFoundResult();
            }

            branch.IsActive = false;
            _dbContext.Branches.Update(branch);
            await _dbContext.SaveChangesAsync();

            return new NoContentResult();
        }
    }


}
