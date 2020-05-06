using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Services;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class UserRegistrationController : Controller
    {
        private readonly IUserService _userService;

        public UserRegistrationController(IUserService userService)
        {
            _userService = userService;
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
            if(user?.IsEmailConfirmed == true)
            {
                return RedirectToAction("Index", "GameInvitation", new
                {
                    email
                });
            }
            ViewBag.Email = email;
            return View();
        }
    }
}