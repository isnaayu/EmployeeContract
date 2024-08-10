using EmployeeContract.Models;
using EmployeeContract.Repository;
using OfficeOpenXml;

namespace EmployeeContract.Services
{
    public interface IPositionsService
    {
        Task ImportPositionsAsync(Stream fileStream);
    }

    public class PositionsService : IPositionsService
    {
        private readonly ApplicationsDBContext _dbContext;

        public PositionsService(ApplicationsDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task ImportPositionsAsync(Stream fileStream)
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
                        PositionName = positionsName
                    };

                    positions.Add(position);
                }

                _dbContext.Positions.AddRange(positions);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
