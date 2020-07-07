using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;
using TicTacToe.Services;

namespace TicTacToe.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Login(string returnUrl)
        {
            return await Task.Run(() =>
            {
                var loginModel = new LoginModel { ReturnUrl = returnUrl };
                return View(loginModel);
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.SignInUser(loginModel, HttpContext);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(loginModel.ReturnUrl))
                        return RedirectToAction(loginModel.ReturnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", result.IsLockedOut ? 
                        "Użytkownik jest zablokowany" : "Użytkownik nie ma prawa dostępu");
            }
            return View();
        }

        public IActionResult Logout()
        {
            _userService.SingOutUser(HttpContext).Wait();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}