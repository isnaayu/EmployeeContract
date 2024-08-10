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


    }
}
