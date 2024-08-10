using EmployeeContract.Models;
using EmployeeContract.Repository;
using OfficeOpenXml;

namespace EmployeeContract.Services
{
    public interface IBranchService
    {
        Task ImportBranchesAsync(Stream fileStream);
    }

    public class BranchService : IBranchService
    {
        private readonly ApplicationsDBContext _dbContext;

        public BranchService(ApplicationsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ImportBranchesAsync(Stream fileStream)
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
                        BranchName = branchName
                    };

                    branches.Add(branch);
                }

                _dbContext.Branches.AddRange(branches);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
