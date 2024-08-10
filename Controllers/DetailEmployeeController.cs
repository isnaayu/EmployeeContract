using EmployeeContract.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeContract.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DetailEmployeeController : Controller
    {
        private readonly IDetailEmployeeService _detailEmployeeService;
        private readonly ILogger<DetailEmployeeController> _logger;
        public DetailEmployeeController(IDetailEmployeeService detailEmployeeService, ILogger<DetailEmployeeController> logger)
        {
            _detailEmployeeService = detailEmployeeService;
            _logger = logger;
        }

        [HttpPost("import-detailemployee")]
        public async Task<IActionResult> ImportDetailEmployee([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not selected or is empty.");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    await _detailEmployeeService.ImportDetailEmployeesAsync(stream);
                }
                return Ok("Data imported successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while importing data");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}
