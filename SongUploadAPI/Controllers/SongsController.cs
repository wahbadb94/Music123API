using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Extensions;
using SongUploadAPI.Filters;
using SongUploadAPI.Models;
using SongUploadAPI.Services;

namespace SongUploadAPI.Controllers
{
    //TODO: add better swagger docs, check out nick chapsas's video on advanced swagger 

    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongsService _songsService;
        public SongsController(ISongsService songsService) => _songsService = songsService;

        [DisableFormValueModelBinding]
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            // create a closure to capture controller's TryUpdateModelAsync method,
            // it is needed to do manual model binding
            async Task<bool> TryBindModelAsync(SongFormData formData, FormValueProvider valueProvider) =>
                await TryUpdateModelAsync(formData, "", valueProvider);

            var result = await _songsService.CreateSongAsync(HttpContext.GetUserId(), Request, TryBindModelAsync);

            return result.Match<IActionResult>(
                song =>
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host.ToUriComponent()}";
                    var locationUri = $"{baseUrl}{Request.Path}/{song.Id}";
                    return Created(locationUri, song);
                },
                error => BadRequest(error.Message));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            (await _songsService.GetAllSongsAsync(HttpContext.GetUserId()))
                .Match<IActionResult>(
                    Ok,
                    err => BadRequest(err.Message));

        [HttpGet("id")]
        public async Task<IActionResult> Get(string songId) =>
            (await _songsService.GetSongAsync(HttpContext.GetUserId(), songId))
                .Match<IActionResult>(
                    Ok,
                    err => BadRequest(err.Message));

        [HttpPut("id")]
        public async Task<IActionResult> Put(string songId, [FromBody] SongFormData updatedSongData) =>
            (await _songsService.UpdateSongAsync(HttpContext.GetUserId(), songId, updatedSongData))
            .Match<IActionResult>(
                Ok,
                err => BadRequest(err.Message));

        [HttpDelete("id")]
        public async Task<IActionResult> Delete(string songId) =>
            (await _songsService.DeleteSongAsync(HttpContext.GetUserId(), songId))
                .Match<IActionResult>(
                    Ok,
                    err => BadRequest(err.Message));

    }
}
