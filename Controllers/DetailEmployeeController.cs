using EmployeeContract.DTO.Request;
using EmployeeContract.DTO.Response;
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
        public async Task<IActionResult> ImportDetailEmployee([FromForm] IFormFile file, [FromForm] string subjectName)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not selected or is empty.");
            }

            try
            {
                await _detailEmployeeService.ImportDetailEmployeesAsync(file, subjectName);
                return Ok("Data imported successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while importing data");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDetailEmployees()
        {
            var employeeDetails = await _detailEmployeeService.GetAllDetailEmployee();
            return Ok(employeeDetails);
        }

        [HttpPut]
        public async Task<ActionResult<CommonResponse<DetailEmployeeResponse>>> UpdateDetailEmployee([FromBody] DetailEmployeeRequest request)
        {
            try
            {
                var updatedDetailEmployee = await _detailEmployeeService.UpdateDetailsEmployee(request);
                var response = new CommonResponse<DetailEmployeeResponse>
                {
                    StatusCode = 200,
                    Message = "Detail employee updated successfully",
                    Data = updatedDetailEmployee
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<DetailEmployeeResponse>
                {
                    StatusCode = 500,
                    Message = "Error updating detail employee: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, response);
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetailEmployee(int id)
        {
            var response = await _detailEmployeeService.DeleteDetailEmployeeAndEmployeeAsync(id);
            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            return StatusCode(response.StatusCode, response);
        }



    }
}
