using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelController(IHttpClientFactory httpClientFactory) : ControllerBase
    {
        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromBody] object mushroomData)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                
                string malApiBaseUrl = "http://mal-api:5000";
                string predictEndpoint = $"{malApiBaseUrl}/predict";
                

                var content = new StringContent(
                    JsonSerializer.Serialize(mushroomData),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(predictEndpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error forwarding request to prediction service");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                string malApiBaseUrl = "http://mal-api:5000";
                
                var response = await client.GetAsync($"{malApiBaseUrl}/health");
                
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { status = "healthy", upstream = "healthy" });
                }
                else
                {
                    return Ok(new { status = "healthy", upstream = "unhealthy" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = "healthy", upstream = "unreachable" });
            }
        }
    }
}