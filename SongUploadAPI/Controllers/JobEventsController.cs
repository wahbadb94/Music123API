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

        private bool EventTypeSubcriptionValidation
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
            "SubscriptionValidation";

        private bool EventTypeNotification
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "Notification";

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var jsonContent = await reader.ReadToEndAsync();

                if (EventTypeSubcriptionValidation)
                {
                    var gridEvent =
                        JsonConvert.DeserializeObject<List<Event<Dictionary<string, string>>>>(jsonContent)
                            .First();

                    await _hubContext.Clients.Groups(GetJobIdFromEventSubject(gridEvent.Subject)).SendAsync(
                        "gridUpdate",
                        gridEvent.Id,
                        gridEvent.EventType,
                        gridEvent.Subject,
                        gridEvent.EventTime.ToLongTimeString(),
                        jsonContent.ToString());

                    //await _hubContext.Clients.All.SendAsync(
                    //    "gridupdate",
                    //    gridEvent.Id,
                    //    gridEvent.EventType,
                    //    gridEvent.Subject,
                    //    gridEvent.EventTime.ToLongTimeString(),
                    //    jsonContent.ToString());

                    // Retrieve the validation code and echo back.
                    var validationCode = gridEvent.Data["validationCode"];
                    return new JsonResult(new
                    {
                        validationResponse = validationCode
                    });
                }
                else if (EventTypeNotification)
                {
                    var events = JArray.Parse(jsonContent);
                    foreach (var e in events)
                    {
                        // Invoke a method on the clients for 
                        // an event grid notiification.                        
                        var details = JsonConvert.DeserializeObject<Event<dynamic>>(e.ToString());


                        await _hubContext.Clients.Groups(GetJobIdFromEventSubject(details.Subject)).SendAsync(
                            "gridupdate",
                            details.Id,
                            details.EventType,
                            details.Subject,
                            details.EventTime.ToLongTimeString(),
                            e.ToString());


                        //await _hubContext.Clients.All.SendAsync(
                        //    "gridupdate",
                        //    details.Id,
                        //    details.EventType,
                        //    details.Subject,
                        //    details.EventTime.ToLongTimeString(),
                        //    e.ToString());
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        private string GetJobIdFromEventSubject(string subject)
        {
            // subject is of the form "transforms/VideoAnalyzerTransform/jobs/<job-id>"
            var uriComponents = subject.Split('/');
            return uriComponents.Last();
        }
    }
}
