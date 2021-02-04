using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SongUploadAPI.Hubs;
using SongUploadAPI.Models;

namespace SongUploadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class JobEventsController : ControllerBase
    {
        private readonly IHubContext<JobUpdateHub> _hubContext;

        public JobEventsController(IHubContext<JobUpdateHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private bool EventTypeSubscriptionValidation
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
            "SubscriptionValidation";

        private bool EventTypeNotification
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "Notification";

        [HttpOptions]
        public IActionResult Options()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();
                var webhookRequestCallback = HttpContext.Request.Headers["WebHook-Request-Callback"];
                var webhookRequestRate = HttpContext.Request.Headers["WebHook-Request-Rate"];
                HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
                HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var jsonContent = await reader.ReadToEndAsync();

                if (EventTypeSubscriptionValidation)
                {
                    var gridEvent =
                        JsonConvert.DeserializeObject<List<Event<Dictionary<string, string>>>>(jsonContent)
                            .First();

                    // Retrieve the validation code and echo back.
                    var validationCode = gridEvent.Data["validationCode"];
                    return new JsonResult(new
                    {
                        validationResponse = validationCode
                    });
                }

                if (EventTypeNotification)
                {
                    var events = JArray.Parse(jsonContent);
                    foreach (var e in events)
                    {
                        // Invoke a method on the clients for 
                        // an event grid notiification.                        
                        var details = JsonConvert.DeserializeObject<Event<dynamic>>(e.ToString());

                        if (details.Subject != "")
                        {
                            await _hubContext.Clients.Groups(GetJobIdFromEventSubject(details.Subject)).SendAsync(
                                "gridUpdate",
                                details.Id,
                                details.EventType,
                                details.Subject,
                                details.EventTime.ToLongTimeString(),
                                e.ToString());
                        }
                    }

                    return Ok();
                }

                // otherwise request is missing an appropriate azure event grid header
                return BadRequest("Missing appropriate \"aeg-event-type\" header." +
                                  "Expected either SubscriptionValidation or Notification");
            }
        }

        private static string GetJobIdFromEventSubject(string subject)
        {
            // subject is of the form "transforms/VideoAnalyzerTransform/jobs/<job-id>"
            var uriComponents = subject.Split('/');
            var jobId = uriComponents.Last();
            return jobId;
        }
    }
}
