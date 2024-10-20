using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace com.wrapper.centeralizedconfiguration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private IConfiguration configuration;
        private IConfigurationRefresherProvider provider;
        private ILogger<ConfigurationController> logger;

        public ConfigurationController(IConfiguration _configuration, IConfigurationRefresherProvider provider, ILogger<ConfigurationController> logger)
        {
            configuration = _configuration;
            this.provider = provider;
            this.logger = logger;
        }

        [HttpGet("GetConfig/{prefix}")]
        public IActionResult GetConfig([FromRoute] string prefix)
        {
            IConfigurationSection section = configuration.GetSection(prefix);
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (section.Exists())
            {
                result = Helper.GetSubSectionConfigs(section, prefix);
            }
            else
            {
                return BadRequest("No config section is not found");
            }

            return Ok(result);
        }

        [HttpGet("Refresh")]
        public async Task<IActionResult> Refresh()
        {
            logger.LogInformation("Refresh method started");
            foreach (var refresh in provider.Refreshers)
            {
                return Ok(await refresh.TryRefreshAsync());
            }
            return BadRequest();
        }
    }
}
