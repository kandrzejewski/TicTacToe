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
        public IActionResult DeleteUser(string email)
        {
            var user = _userService.GetUserByEmail(email).Result;
            if (user == null)
                return BadRequest();
            _userService.DeleteUser(email);
            return Ok();
        }
    }
}