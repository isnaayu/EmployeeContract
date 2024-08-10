using EmployeeContract.Services;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest("File is not selected or is empty.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                await _branchService.ImportBranchesAsync(stream);
            }

            return Ok("Data imported successfully");
        }
    }
}
