using Dapper;
using EmployeeContract.DTO.Response;
using EmployeeContract.Models;
using EmployeeContract.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;

namespace EmployeeContract.Services
{
    public interface IPositionsService
    {
        Task<List<PositionResponse>> ImportPositionsAsync(Stream fileStream);
        Task<List<PositionResponse>> getAllPositions();
        Task<IActionResult> deletePositionById(int id);
    }

    public class PositionsService : IPositionsService
    {
        private readonly ApplicationsDBContext _dbContext;

        public PositionsService(ApplicationsDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<PositionResponse>> ImportPositionsAsync(Stream fileStream)
        {
            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                var positions = new List<PositionsModel>();

                for (int row = 2; row <= rowCount; row++)
                {
                    var positionsCode = worksheet.Cells[row, 1].Value.ToString();
                    var positionsName = worksheet.Cells[row, 2].Value.ToString();

                    var position = new PositionsModel
                    {
                        PositionCode = positionsCode,
                        PositionName = positionsName,
                        IsActive = true
                    };

                    positions.Add(position);
                }

                _dbContext.Positions.AddRange(positions);
                await _dbContext.SaveChangesAsync();

                var resultList = positions.Select(p => new PositionResponse
                {
                    PositionId = p.PositionId,
                    PositionCode = p.PositionCode,
                    PositionName = p.PositionName,
                    IsActive = p.IsActive
                }).ToList();

                return resultList;
            }
        }


        public async Task<List<PositionResponse>> getAllPositions()
        {
            using var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }


            var result = await connection.QueryAsync<PositionResponse>(
                "getAllPositions",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<IActionResult> deletePositionById(int id)
        {
            var position = await _dbContext.Positions.FindAsync(id);

            if (position == null)
            {
                return new NotFoundResult();
            }

            position.IsActive = false;
            _dbContext.Positions.Update(position);
            await _dbContext.SaveChangesAsync();

            return new NoContentResult();
        }
    }

    


}
