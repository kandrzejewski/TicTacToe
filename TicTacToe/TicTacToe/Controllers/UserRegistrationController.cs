using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services;


namespace TicTacToe.Controllers
{
    public class UserRegistrationController : Controller
    {
        readonly IUserService _userService;
        readonly IEmailService _emailService;

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
            else
            {
                return View(userModel);
            }  
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
                Protocol = Request.Scheme,
                Host = Request.Host.ToString()
            };

            var userRegistrationEmail = new UserRegistrationEmailModel
            {
                DisplayName = $"{user.FirstName} {user.LastName}",
                Email = email,
                ActionUrl = Url.Action(urlAction)
            };

            var emailRenderService = HttpContext.RequestServices.GetService<IEmailTemplateRenderService>();
            var message = await emailRenderService.RenderTemplate(
                "EmailTemplates/UserRegistrationEmail",
                userRegistrationEmail, Request.Host.ToString());

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
                    new { email });

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

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if(user != null)
            {
                user.IsEmailConfirmed = true;
                user.EmailConfirmationDate = DateTime.Now;
                await _userService.UpdateUser(user);
                return RedirectToAction("Index", "Home");
            }
            return BadRequest();
        }
    }
}