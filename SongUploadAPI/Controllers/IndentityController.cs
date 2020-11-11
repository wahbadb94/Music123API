using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Contracts.Responses;
using SongUploadAPI.Services;

namespace SongUploadAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IndentityController : ControllerBase
    {
        private readonly IIdentityService _indentityService;

        public IndentityController(IIdentityService indentityService)
        {
            _indentityService = indentityService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            var authResponse = await _indentityService.RegisterAsync(request.Email, request.Password);

            if(authResponse.RequestSuccess == false)
            {
                return BadRequest(new RegistrationFailedResponse
                {
                    Errors = authResponse.ErrorMessages
                });
            }

            return Ok( new RegistrationSuccessResponse
            { 
                Token = authResponse.Token
            });
        }
    }
}
