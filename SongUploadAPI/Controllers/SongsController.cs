using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SongUploadAPI.Extensions;
using SongUploadAPI.Filters;
using SongUploadAPI.Models;
using SongUploadAPI.Services;

// complex delegate type alias to make methods more readable
using TryBindModelAsyncDelegate =
    System.Func<
        SongUploadAPI.Models.SongFormData,
        Microsoft.AspNetCore.Mvc.ModelBinding.FormValueProvider,
        System.Threading.Tasks.Task<bool>>;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SongUploadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongsService _songsService;
        public SongsController(ISongsService songsService)
        {
            _songsService = songsService;
        }

        [HttpGet]
        public  IActionResult GetAll()
        {
            try
            {
                var songs = _songsService.GetAllSongs(HttpContext.GetUserId());
                return Ok(songs);

            }
            catch (Exception e)
            {
                ModelState.AddModelError("SongRetrievalError", e.Message);
                return BadRequest(ModelState);
            }
        }

        [DisableFormValueModelBinding]
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            // create a closure to capture controller's TryUpdateModelAsync method
            async Task<bool> TryBindModelAsync(SongFormData formData, FormValueProvider valueProvider) =>
                await TryUpdateModelAsync(formData, "", valueProvider);

            var createSongResult = await _songsService.CreateSongAsync(HttpContext.GetUserId(), Request, TryBindModelAsync);

            if (createSongResult.Failed)
            {
                ModelState.AddModelError("SongUploadError", createSongResult.ErrorMessage);
                return BadRequest(ModelState);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host.ToUriComponent()}";
            var locationUri = $"{baseUrl}{Request.Path}/{createSongResult.Song.Id}";

            return Created(locationUri, createSongResult.Song);
        }
    }
}
