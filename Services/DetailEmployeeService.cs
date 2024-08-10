using EmployeeContract.Models;
using EmployeeContract.Repository;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace EmployeeContract.Services
{
    public interface IDetailEmployeeService
    {
        Task ImportDetailEmployeesAsync(Stream fileStream);
    }

    public class DetailEmployeeService : IDetailEmployeeService
    {
        private readonly ApplicationsDBContext _applicationsDBContext;
        private readonly ILogger<DetailEmployeeService> _logger;

        public DetailEmployeeService(ApplicationsDBContext applicationsDBContext, ILogger<DetailEmployeeService> logger)
        {
            _applicationsDBContext = applicationsDBContext;
            _logger = logger;
        }

        public async Task ImportDetailEmployeesAsync(Stream fileStream)
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            var employees = new List<EmployeeModel>();
            var detailEmployees = new List<DetailEmployee>();

            var positionCache = new Dictionary<string, PositionsModel>();
            var branchCache = new Dictionary<string, BranchModel>();

            for (int row = 2; row <= rowCount; row++)
            {
                var employeeName = worksheet.Cells[row, 1].Value.ToString();
                var birthDate = DateTime.Parse(worksheet.Cells[row, 2].Value.ToString());
                var contractTime = int.Parse(worksheet.Cells[row, 3].Value.ToString() ?? "0");
                var startDate = DateTime.Parse(worksheet.Cells[row, 4].Value.ToString());
                var positionName = worksheet.Cells[row, 5].Value.ToString();
                var branchName = worksheet.Cells[row, 6].Value.ToString();

                if (string.IsNullOrEmpty(employeeName) || string.IsNullOrEmpty(positionName) || string.IsNullOrEmpty(branchName))
                {
                    _logger.LogWarning($"Skipping row {row} due to missing data");
                    continue;
                }

                var endDate = startDate.AddMonths(contractTime);

                var employee = new EmployeeModel
                {
                    EmployeeName = employeeName,
                    BirthDate = birthDate,
                    ContractPeriod = contractTime,
                    StartDate = startDate
                };

                if (!positionCache.TryGetValue(positionName, out var position))
                {
                    position = await _applicationsDBContext.Positions
                        .FirstOrDefaultAsync(p => p.PositionName == positionName);
                    if (position == null)
                    {
                        _logger.LogWarning($"Position '{positionName}' not found in database");
                        continue;
                    }
                    positionCache[positionName] = position;
                }

                if (!branchCache.TryGetValue(branchName, out var branch))
                {
                    branch = await _applicationsDBContext.Branches
                        .FirstOrDefaultAsync(b => b.BranchName == branchName);
                    if (branch == null)
                    {
                        _logger.LogWarning($"Branch '{branchName}' not found in database");
                        continue;
                    }
                    branchCache[branchName] = branch;
                }

                employees.Add(employee);

                var detailEmployee = new DetailEmployee
                {
                    Employee = employee,
                    PositionId = position.PositionId,
                    BranchId = branch.BranchId,
                    ContractEnd = endDate
                };

                detailEmployees.Add(detailEmployee);
            }

            using var transaction = await _applicationsDBContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Saving {employees.Count} employees");
                await _applicationsDBContext.Employees.AddRangeAsync(employees);
                var employeeSaveResult = await _applicationsDBContext.SaveChangesAsync();
                _logger.LogInformation($"Saved {employeeSaveResult} employees");

                _logger.LogInformation($"Saving {detailEmployees.Count} detail employees");
                await _applicationsDBContext.DetailEmployees.AddRangeAsync(detailEmployees);
                var detailSaveResult = await _applicationsDBContext.SaveChangesAsync();
                _logger.LogInformation($"Saved {detailSaveResult} detail employees");

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving imported data");
                throw;
            }
        }
    }
}