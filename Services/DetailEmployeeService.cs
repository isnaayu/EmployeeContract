using Dapper;
using EmployeeContract.DTO.Request;
using EmployeeContract.DTO.Response;
using EmployeeContract.Models;
using EmployeeContract.Repository;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;

namespace EmployeeContract.Services
{
    public interface IDetailEmployeeService
    {
        Task ImportDetailEmployeesAsync(IFormFile file, string subjectName);
        Task<List<DetailEmployeeResponse>> GetAllDetailEmployee(int? daysUntilContractEnds);
        Task<DetailEmployeeResponse> UpdateDetailsEmployee(DetailEmployeeRequest detailEmployeeRequest);
        Task<CommonResponse<object>> DeleteDetailEmployeeAndEmployeeAsync(int detailEmployeeId);
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

        public async Task ImportDetailEmployeesAsync(IFormFile file, string subjectName)
        {
            var fileName = file.FileName;
            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            var employees = new List<EmployeeModel>();
            var detailEmployees = new List<DetailEmployeeModel>();

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

                var detailEmployee = new DetailEmployeeModel
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

                var fileList = new FileListModel
                {
                    SubjectName = subjectName,
                    FileName = fileName
                };
                await _applicationsDBContext.FileLists.AddAsync(fileList);
                await _applicationsDBContext.SaveChangesAsync();


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

        public async Task<List<DetailEmployeeResponse>> GetAllDetailEmployee(int? daysUntilContractEnds)
        {
            using var connection = _applicationsDBContext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@DaysUntilContractEnds", daysUntilContractEnds, DbType.Int32, ParameterDirection.Input);

            var result = await connection.QueryAsync<DetailEmployeeResponse>(
                "GetAllEmployeesWithDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<DetailEmployeeResponse> UpdateDetailsEmployee(DetailEmployeeRequest updateRequest)
        {
            using var transaction = await _applicationsDBContext.Database.BeginTransactionAsync();

            try
            {
                var employee = await _applicationsDBContext.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == updateRequest.EmployeeId);

                if (employee == null)
                {
                    _logger.LogWarning($"Employee with ID {updateRequest.EmployeeId} not found.");
                    return null;
                }

                employee.EmployeeName = updateRequest.EmployeeName;
                employee.BirthDate = updateRequest.BirthDate;
                employee.ContractPeriod = updateRequest.ContractPeriod;
                employee.StartDate = updateRequest.StartDate;

                var endDate = employee.StartDate.AddMonths(employee.ContractPeriod);

                var detailEmployee = await _applicationsDBContext.DetailEmployees
                    .Include(de => de.Position)
                    .Include(de => de.Branch)
                    .FirstOrDefaultAsync(de => de.DetailEmployeeId == updateRequest.DetailEmployeeId && de.EmployeeId == updateRequest.EmployeeId);

                if (detailEmployee == null)
                {
                    _logger.LogWarning($"DetailEmployee with ID {updateRequest.DetailEmployeeId} not found for Employee ID {updateRequest.EmployeeId}.");
                    return null;
                }

                detailEmployee.PositionId = updateRequest.PositionId;
                detailEmployee.BranchId = updateRequest.BranchId;
                detailEmployee.ContractEnd = endDate;

                await _applicationsDBContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new DetailEmployeeResponse
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.EmployeeName,
                    BirthDate = employee.BirthDate,
                    ContractPeriod = employee.ContractPeriod,
                    StartDate = employee.StartDate,
                    DetailEmployeeId = detailEmployee.DetailEmployeeId,
                    PositionId = detailEmployee.PositionId,
                    BranchId = detailEmployee.BranchId,
                    PositionName = detailEmployee.Position?.PositionName ?? "Unknown Position", 
                    BranchName = detailEmployee.Branch?.BranchName ?? "Unknown Branch",
                    ContractEnd = detailEmployee.ContractEnd
                };

                _logger.LogInformation($"Successfully updated and fetched Employee ID {updateRequest.EmployeeId} with DetailEmployee ID {updateRequest.DetailEmployeeId}.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Employee and DetailEmployee.");
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<CommonResponse<object>> DeleteDetailEmployeeAndEmployeeAsync(int detailEmployeeId)
        {
            using var transaction = await _applicationsDBContext.Database.BeginTransactionAsync();

            try
            {
                var detailEmployee = await _applicationsDBContext.DetailEmployees
                    .Include(de => de.Employee)
                    .FirstOrDefaultAsync(de => de.DetailEmployeeId == detailEmployeeId);

                if (detailEmployee == null)
                {
                    _logger.LogWarning($"DetailEmployee with ID {detailEmployeeId} not found.");
                    return new CommonResponse<object>
                    {
                        StatusCode = 404,
                        Message = $"DetailEmployee with ID {detailEmployeeId} not found.",
                        Data = null
                    };
                }

                _applicationsDBContext.DetailEmployees.Remove(detailEmployee);

                var employeeHasOtherDetails = await _applicationsDBContext.DetailEmployees
                    .AnyAsync(de => de.EmployeeId == detailEmployee.EmployeeId && de.DetailEmployeeId != detailEmployeeId);

                if (!employeeHasOtherDetails)
                {
                    var employee = detailEmployee.Employee;
                    _applicationsDBContext.Employees.Remove(employee);
                }

                await _applicationsDBContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Successfully deleted DetailEmployee with ID {detailEmployeeId} and associated Employee if applicable.");
                return new CommonResponse<object>
                {
                    StatusCode = 200,
                    Message = $"Successfully deleted DetailEmployee with ID {detailEmployeeId} and associated Employee if applicable.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting DetailEmployee and Employee.");
                return new CommonResponse<object>
                {
                    StatusCode = 500,
                    Message = "Error occurred while deleting DetailEmployee and Employee.",
                    Data = null
                };
            }
        }


    }
}