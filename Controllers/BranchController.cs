using EmployeeContract.DTO.Response;
using EmployeeContract.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace EmployeeContract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpPost("import-branches")]
        public async Task<IActionResult> ImportBranches([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new CommonResponse<string>
                {
                    StatusCode = 400,
                    Message = "File is not selected or is empty.",
                    Data = null
                });
            }

            try
            {
                List<BranchResponse> datas;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    datas = await _branchService.ImportBranchesAsync(stream);
                }

                var response = new CommonResponse<List<BranchResponse>>
                {
                    StatusCode = 200,
                    Message = "Branch successfully created",
                    Data = datas
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<string>
                {
                    StatusCode = 500,
                    Message = $"Error importing branch: {ex.Message}",
                    Data = null
                };

                return StatusCode(response.StatusCode, response);
            }
        }


        [HttpGet]
        public async Task<IActionResult> getAllBranches()
        {
            try
            {
                List<BranchResponse> datas = await _branchService.getAllBranches();

                var response = new CommonResponse<List<BranchResponse>>
                {
                    StatusCode = 200,
                    Message = "successfully Fetching Branch",
                    Data = datas
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<string>
                {
                    StatusCode = 500,
                    Message = $"Error fetching branch: {ex.Message}",
                    Data = null
                };

                return StatusCode(response.StatusCode, response);
            }


        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranchById(int id)
        {

            try
            {
                var response = await _branchService.deleteBranchById(id);
                if (response == null)
                {
                    return NotFound($"Branch with id {id} not found.");
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
    }

        
}
