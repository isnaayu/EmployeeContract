using EmployeeContract.DTO.Response;
using EmployeeContract.Repository;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Dapper;

namespace EmployeeContract.Services
{
    public interface IFileListsService
    {
        Task<List<FileListResponse>> getAllFileLists();
    }
    public class FileListsService : IFileListsService
    {
        private readonly ApplicationsDBContext _dbContext;

        public FileListsService(ApplicationsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FileListResponse>> getAllFileLists()
        {
            using var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }


            var result = await connection.QueryAsync<FileListResponse>(
                "getAllFileLists",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
}
