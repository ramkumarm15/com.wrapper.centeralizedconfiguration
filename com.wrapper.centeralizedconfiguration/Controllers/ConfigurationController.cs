using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.Extensions;
using System.Text.Json;

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
                logger.LogInformation(JsonSerializer.Serialize(result));
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


        [HttpPost("EventGridAsync")]
        public async Task<IActionResult> EventGridAsync()
        {
            var events = await BinaryData.FromStreamAsync(Request.Body);

            EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);

            logger.LogInformation("data: " + JsonSerializer.Serialize(eventGridEvents));

            foreach (var eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.TryGetSystemEventData(out var eventData))
                {
                    if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                    {
                        logger.LogInformation("SubscriptionValidationEventData triggered");
                        var responseData = new SubscriptionValidationResponse()
                        {
                            ValidationResponse = subscriptionValidationEventData.ValidationCode
                        };
                        logger.LogInformation(JsonSerializer.Serialize(responseData));
                        return Ok(responseData);
                    }
                    if (eventData is AppConfigurationKeyValueModifiedEventData appConfigurationKeyValueModifiedEventData)
                    {
                        logger.LogInformation($"Updating config data...");
                        if (eventGridEvent.TryCreatePushNotification(out PushNotification pushNotification))
                        {

                            foreach (var refresh in provider.Refreshers)
                            {
                                //refresh.ProcessPushNotification(pushNotification);

                                var result = await refresh.TryRefreshAsync();
                                if (result)
                                {
                                    logger.LogInformation("Configuration updated..");
                                    return Ok(result);
                                }
                            }
                        }
                    }
                }
            }

            return Ok();
        }
    }
}
