using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Services;

namespace TicTacToe.Controllers
{
    [Produces("application/json")]
    [Route("restapi/v1/UserRegistration")]
    [ApiController]
    public class UserRegistrationApiController : Controller
    {
        private IUserService _userService;

        public UserRegistrationApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpDelete("{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null)
                return BadRequest();
            await _userService.DeleteUser(user);
            return Ok();
        }
    }
}