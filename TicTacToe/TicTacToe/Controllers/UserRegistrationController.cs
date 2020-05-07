using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Services;
using TicTacToe.Models;
using Microsoft.AspNetCore.Mvc.Routing;

namespace TicTacToe.Controllers
{
    public class UserRegistrationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public UserRegistrationController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                await _userService.RegisterUser(userModel);
                //return Content($"Użytkownik {userModel.FirstName} {userModel.LastName} został pomyślnie zarejestrowany.");
                return RedirectToAction(nameof(EmailConfirmation), new { userModel.Email });
            }
            return View(userModel);
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirmation (string email)
        {
            var user = await _userService.GetUserByEmail(email);
            var urlAction = new UrlActionContext
            {
                Action = "ConfirmEmail",
                Controller = "UserRegistration",
                Values = new { email },
                Host = Request.Host.ToString()
            };

            var message = $"Dziękujemy za rejestracje na naszej stronie. Aby potwierdzić adres e-mail, proszę kliknąć tutaj {Url.Action(urlAction)}";
            try
            {
                _emailService.SendEmail(email,
                    "Potwierdzenie adresu e-mail w grze Kółko i krzyżyk", message)
                    .Wait();
            }
            catch (Exception e)
            {
            }

            if (user?.IsEmailConfirmed == true)
                return RedirectToAction("Index", "GameInvitation",
                    new { email = email });

            ViewBag.Email = email;
            return View();

            //Sumulowanie potwierdzenia adresu email
            //if(user?.IsEmailConfirmed == true)
            //{
            //    return RedirectToAction("Index", "GameInvitation", new
            //    {
            //        email
            //    });
            //}
            //ViewBag.Email = email;
            //return View();
        }
    }
}