using Microsoft.AspNetCore.Mvc;
using RPG.Data;

namespace RPG.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(Dtos.User.UserRegisterDto request)
        {
            var response = await _authRepo.Register(
                new Models.User { Username = request.Username }, request.Password
            );

            if(!response.Succes)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(Dtos.User.UserLoginDto request)
        {
            var response = await _authRepo.Login(request.Username, request.Password);

            if (!response.Succes)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
