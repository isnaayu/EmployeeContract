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
                var datas = await _detailEmployeeService.ImportDetailEmployeesAsync(file, subjectName);

                var response = new CommonResponse<List<DetailEmployeeResponse>>
                {
                    StatusCode = 200,
                    Message = "Employees successfully created",
                    Data = datas
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<List<DetailEmployeeResponse>>
                {
                    StatusCode = 500,
                    Message = "Error Import employees: " + ex.Message,
                    Data = null
                };

                return StatusCode(response.StatusCode, response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDetailEmployees([FromQuery] int? daysUntilContractEnds)
        {
            try
            {
                var employeeDetails = await _detailEmployeeService.GetAllDetailEmployee(daysUntilContractEnds);

                var response = new CommonResponse<List<DetailEmployeeResponse>>
                {
                    StatusCode = 200,
                    Message = "Employees retrieved successfully",
                    Data = employeeDetails
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<List<DetailEmployeeResponse>>
                {
                    StatusCode = 500,
                    Message = "Error retrieving employees: " + ex.Message,
                    Data = null
                };

                return StatusCode(response.StatusCode, response);
            }
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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetailEmployeeById(int id)
        {
            var result = await _detailEmployeeService.GetDetailEmployeeById(id);

            if (result == null)
            {
                return NotFound(new CommonResponse<DetailEmployeeResponse>
                {
                    StatusCode = 404,
                    Message = "Detail employee not found",
                    Data = null
                });
            }

            return Ok(new CommonResponse<DetailEmployeeResponse>
            {
                StatusCode = 200,
                Message = "Success",
                Data = result
            });
        }




    }
}
