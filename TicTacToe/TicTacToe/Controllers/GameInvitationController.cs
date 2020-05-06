using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using TicTacToe.Services;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class GameInvitationController : Controller
    {
        private readonly IStringLocalizer<GameInvitationController> _stringLocalizer;
        private readonly IUserService _userService;

        public GameInvitationController(IUserService userService,
            IStringLocalizer<GameInvitationController> stringLocalizer)
        {
            _userService = userService;
            _stringLocalizer = stringLocalizer;
        }

        [HttpPost]
        public IActionResult Index(GameInvitationModel gameInvitationModel)
        {
            return Content(_stringLocalizer["GameInvitationConfirmationMessage",
                gameInvitationModel.EmailTo]);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel { InvitedBy = email };
            HttpContext.Session.SetString("email", email);
            return View(gameInvitationModel);
        }
    }
}