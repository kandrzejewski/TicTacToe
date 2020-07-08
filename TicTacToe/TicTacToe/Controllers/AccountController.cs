using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLogin(string provider, string ReturnUrl)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallBack), "Account", new { ReturnUrl = ReturnUrl }, Request.Scheme, Request.Host.ToString());
            var properties = await _userService.GetExternalAuthenticationProperties(provider, redirectUrl);
            ViewBag.ReturnUrl = redirectUrl;
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallBack(string returnUrl, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Błąd zewnętrznego dostawcy: {remoteError}");
                ViewBag.ReturnUrl = returnUrl;
                return View("Login");
            }

            var info = await _userService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            var result = await _userService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                return View("NotFound");
            }
        }
    }
}