using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Contracts.Responses;
using SongUploadAPI.Extensions;
using SongUploadAPI.Filters;
using SongUploadAPI.Services;

namespace SongUploadAPI.Controllers
{
    //TODO: add better swagger docs, check out nick chapsas's video on advanced swagger 

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SongsController : ControllerBase
    {
        private readonly ISongsService _songsService;
        public SongsController(ISongsService songsService) => _songsService = songsService;

        [HttpPost]
        [DisableFormValueModelBinding]
        [ProducesResponseType(typeof(SongSuccessResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Create()
        {
            // create a closure to capture controller's TryUpdateModelAsync method,
            // it is needed to do manual model binding
            async Task<bool> TryBindModelAsync(SongFormData formData, FormValueProvider valueProvider) =>
                await TryUpdateModelAsync(formData, "", valueProvider);

            var result = await _songsService.CreateSongAsync(HttpContext.GetUserId(), Request, TryBindModelAsync);

            return result.Match<IActionResult>(
                songDto =>
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host.ToUriComponent()}";
                    var locationUri = $"{baseUrl}{Request.Path}/{songDto.Id}";
                    return Created(locationUri, songDto.Adapt<SongSuccessResponse>());
                },
                error => BadRequest(error.Adapt<ErrorResponse>()));
        }

        [HttpGet]
        [ProducesResponseType(typeof(SongSuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll() =>
            (await _songsService.GetAllSongsAsync(HttpContext.GetUserId()))
                .Match<IActionResult>(
                    songDtos => Ok(songDtos.Adapt<IList<SongSuccessResponse>>()),
                    error => BadRequest(error.Adapt<ErrorResponse>()));

        [HttpGet("id")]
        [ProducesResponseType(typeof(SongSuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Get(string songId) =>
            (await _songsService.GetSongAsync(HttpContext.GetUserId(), songId))
                .Match<IActionResult>(
                    song => Ok(song.Adapt<SongSuccessResponse>()),
                    error => BadRequest(error.Adapt<ErrorResponse>()));

        [HttpPut("id")]
        [ProducesResponseType(typeof(SongSuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Put(string songId, [FromBody] SongFormData updatedSongData) =>
            (await _songsService.UpdateSongAsync(HttpContext.GetUserId(), songId, updatedSongData))
            .Match<IActionResult>(
                songDto => Ok(songDto.Adapt<SongSuccessResponse>()),
                error => BadRequest(error.Adapt<ErrorResponse>()));

        [HttpDelete("id")]
        [ProducesResponseType(typeof(SongSuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Delete(string songId) =>
            (await _songsService.DeleteSongAsync(HttpContext.GetUserId(), songId))
                .Match<IActionResult>(
                    songDto => Ok(songDto.Adapt<SongSuccessResponse>()),
                    error => BadRequest(error.Adapt<ErrorResponse>()));

    }
}
