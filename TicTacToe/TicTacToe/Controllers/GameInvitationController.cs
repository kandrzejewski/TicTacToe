using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
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
        public IActionResult Index(GameInvitationModel gameInvitationModel,
            [FromServices]IEmailService emailService)
        {
            var gameInvitationService = Request.HttpContext.RequestServices.GetService< IGameInvitationService >();
            if (ModelState.IsValid)
            {
                emailService.SendEmail(gameInvitationModel.EmailTo,
                    _stringLocalizer["Zaprosenie do gry Kółko i krzyżyk"],
                    _stringLocalizer[$"Witaj, {0} zaprasza Cię do gry Kółko i krzyżyk. Aby dołączyć do gry kliknij tutaj {1}.",
                    gameInvitationModel.InvitedBy,
                    Url.Action("GameInvitationConfirmation", "GameInvitation", new
                    {
                        gameInvitationModel.InvitedBy,
                        gameInvitationModel.EmailTo
                    },
                    Request.Scheme,
                    Request.Host.ToString())]);
                var invitation = gameInvitationService.Add(gameInvitationModel).Result;
                return RedirectToAction("GameInvitationConfirmation", new { id = invitation.Id });
            }
            return View(gameInvitationModel);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel { InvitedBy = email };
            HttpContext.Session.SetString("email", email);
            return View(gameInvitationModel);
        }

        [HttpGet]
        public IActionResult GameInvitationConfirmation(Guid id,
            [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = gameInvitationService.Get(id).Result;
            return View(gameInvitation);
        }
    }
}