using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services;


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

        [HttpGet]
        public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel { InvitedBy = email, Id = Guid.NewGuid() };
            Request.HttpContext.Session.SetString("email", email);
            var user = await _userService.GetUserByEmail(email);
            Request.HttpContext.Session.SetString("displayName", $"{user.FirstName} {user.LastName}");
            return View(gameInvitationModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(GameInvitationModel gameInvitationModel,
            [FromServices]IEmailService emailService)
        {
            var gameInvitationService = Request.HttpContext.RequestServices.GetService< IGameInvitationService >();
            if (ModelState.IsValid)
            {
                try
                {
                    var invitationModel = new InvitationEmailModel
                    {
                        DisplayName = $"{gameInvitationModel.EmailTo}",
                        InvitedBy = await _userService.GetUserByEmail(gameInvitationModel.InvitedBy),
                        ConfirmationUrl = Url.Action("ConfirmGameInvitation",
                            "GameInvitation",
                            new { id = gameInvitationModel.Id },
                            Request.Scheme, Request.Host.ToString()),
                        InvitedDate = gameInvitationModel.ConfirmationDate
                    };

                    var emailRenderService = HttpContext.RequestServices.GetService<IEmailTemplateRenderService>();
                    var message = await emailRenderService.RenderTemplate<InvitationEmailModel>(
                        "EmailTemplates/InvitationEmail",
                        invitationModel,
                        Request.Host.ToString());
                    await emailService.SendEmail(gameInvitationModel.EmailTo,
                        _stringLocalizer["Zaprosenie do gry Kółko i krzyżyk"],
                        message);
                }
                catch
                { 

                }
                //Wysyłanie e-maila bez generowania widoku
                //emailService.SendEmail(gameInvitationModel.EmailTo,
                //    _stringLocalizer["Zaprosenie do gry Kółko i krzyżyk"],
                //    _stringLocalizer[$"Witaj, {0} zaprasza Cię do gry Kółko i krzyżyk. Aby dołączyć do gry kliknij tutaj {1}.",
                //    gameInvitationModel.InvitedBy,
                //    Url.Action("GameInvitationConfirmation", "GameInvitation", new
                //    {
                //        gameInvitationModel.InvitedBy,
                //        gameInvitationModel.EmailTo
                //    },
                //    Request.Scheme,
                //    Request.Host.ToString())]);
                var invitation = gameInvitationService.Add(gameInvitationModel).Result;
                return RedirectToAction("GameInvitationConfirmation", new { id = gameInvitationModel.Id });
            }
            return View(gameInvitationModel);
        }

        [HttpGet]
        public IActionResult GameInvitationConfirmation(Guid id,
            [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = gameInvitationService.Get(id).Result;
            return View(gameInvitation);
        }

        [HttpGet]
        public IActionResult ConfirmGameInvitation(Guid id, 
            [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = gameInvitationService.Get(id).Result;
            gameInvitation.IsConfirmed = true;
            gameInvitation.ConfirmationDate = DateTime.Now;
            gameInvitationService.Update(gameInvitation);
            return RedirectToAction("Index", "GameSession", new { id = id });
        }
    }
}