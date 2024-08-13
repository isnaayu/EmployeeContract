using EmployeeContract.DTO.Response;
using EmployeeContract.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeContract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : Controller
    {
        private readonly IPositionsService _positionsService;

        public PositionsController(IPositionsService positionsService)
        {
           _positionsService = positionsService;
        }

        [HttpPost("import-positions")]
        public async Task<IActionResult> ImportPositions([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not selected or is empty.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                await _positionsService.ImportPositionsAsync(stream);
            }

            return Ok("Data imported successfully");
        }

        [HttpGet]
        public async Task<IActionResult> getAllBranches()
        {
            try
            {
                List<PositionResponse> datas = await _positionsService.getAllPositions();

                var response = new CommonResponse<List<PositionResponse>>
                {
                    StatusCode = 200,
                    Message = "successfully Fetching Positions",
                    Data = datas
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new CommonResponse<string>
                {
                    StatusCode = 500,
                    Message = $"Error fetching positions: {ex.Message}",
                    Data = null
                };

                return StatusCode(response.StatusCode, response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePositionById(int id)
        {
            try
            {
                var response = await _positionsService.deletePositionById(id);
                if (response == null)
                {
                    return NotFound($"Position with id {id} not found.");
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
