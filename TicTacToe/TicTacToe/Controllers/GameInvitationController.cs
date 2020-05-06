using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Services;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class GameInvitationController : Controller
    {
        private readonly IUserService _userService;
        public GameInvitationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel { InvitedBy = email };
            return View(gameInvitationModel);
        }
    }
}