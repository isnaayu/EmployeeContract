using EmployeeContract.DTO.Response;
using EmployeeContract.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeContract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileListsController : Controller
    {
        private readonly IFileListsService _fileListsService;

        public FileListsController(IFileListsService fileListsService)
        {
            _fileListsService = fileListsService;
        }

        [HttpGet]
        public async Task<IActionResult> getAllBranches()
        {
            try
            {
                List<FileListResponse> datas = await _fileListsService.getAllFileLists();

                var response = new CommonResponse<List<FileListResponse>>
                {
                    StatusCode = 200,
                    Message = "successfully Fetching File Lists",
                    Data = datas
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<string>
                {
                    StatusCode = 500,
                    Message = $"Error fetching file list: {ex.Message}",
                    Data = null
                };

                return StatusCode(response.StatusCode, response);
            }
        }
    }
}
