using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Managers;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class UserService : IUserService
    {
        private ILogger<UserService> _logger;
        private ApplicationUserManager _userManager;
        private SignInManager<UserModel> _signInManager;

        public UserService(ApplicationUserManager userManager, ILogger<UserService> logger, SignInManager<UserModel> signInManager)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;

            var emailTokenProvider = new EmailTokenProvider<UserModel>();
            _userManager.RegisterTokenProvider("Default", emailTokenProvider);
        }

        public async Task<bool> ConfirmEmail(string email, string code)
        {
            var start = DateTime.Now;
            _logger.LogTrace($"Potwierdzenie adresu e-mail {email}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return false;

                var result = await _userManager.ConfirmEmailAsync(user, code);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można potwierdzić adresu e-mail {email} - {ex}");
                return false;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Potwierdzenie adresu e-mail użytkownika ukończono w ciągu {stopwatch.Elapsed}");
            }
        }

        public async Task<string> GetEmailConfirmationCode(UserModel user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<bool> RegisterUser(UserModel userModel)
        {
            //_userStore.Add(userModel);
            //return Task.FromResult(true);
            //
            //using (var db = new GameDbContext(_dbContextOptions))
            //{
            //    db.UserModels.Add(userModel);
            //    await db.SaveChangesAsync();
            //    return true;
            //}
            //
            var start = DateTime.Now;
            _logger.LogTrace($"Rozpoczęcie rejestracji użytkownika {userModel.Email} - {start}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                userModel.UserName = userModel.Email;
                var result = await _userManager.CreateAsync(userModel, userModel.Password);
                return result == IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można zarejestretować użytkownika {userModel.Email} - {ex}");
                return false;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Rozpoczęcie rejestracji użytkownika {userModel.Email} zakończyło się o {DateTime.Now} - upłynęło(y) {stopwatch.Elapsed.TotalSeconds} sekund(y).");
            }
        }
        public async Task<UserModel> GetUserByEmail(string email)
        {
            //return Task.FromResult(_userStore.FirstOrDefault(u => u.Email == email));
            //
            //using var db = new GameDbContext(_dbContextOptions);
            //return await db.UserModels.FirstOrDefaultAsync(x => x.Email == email);
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserExisting(string email)
        {
            //using var db = new GameDbContext(_dbContextOptions);
            //return await db.UserModels.AnyAsync(user => user.Email == email);
            return (await _userManager.FindByEmailAsync(email)) != null;
        }

        public async Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers)
        {
            //return Task.Run(() => (IEnumerable<UserModel>)_userStore
            //    .OrderBy(x => x.Score)
            //    .Take(numberOfUsers)
            //    .ToList());
            //
            //using var db = new GameDbContext(_dbContextOptions);
            //return await db.UserModels.OrderByDescending(x => x.Score).ToListAsync();
            return await _userManager.Users.OrderByDescending(x => x.Score).ToListAsync();
        }

        public async Task UpdateUser(UserModel userModel)
        {
            //_userStore = new ConcurrentBag<UserModel>(
            //    _userStore.Where(u => u.Email != userModel.Email))
            //{
            //    userModel
            //};
            //return Task.CompletedTask;
            //
            //using var db = new GameDbContext(_dbContextOptions);
            //db.Update(userModel);
            //await db.SaveChangesAsync();
            await _userManager.UpdateAsync(userModel);
        }

        public async Task DeleteUser(UserModel userModel)
        {
            //using var db = new GameDbContext(_dbContextOptions);
            //var user = GetUserByEmail(email).Result;
            //db.Remove(user);
            //await db.SaveChangesAsync();
            await _userManager.DeleteAsync(userModel);
        }

        public async Task<SignInResult> SignInUser(LoginModel loginModel, HttpContext httpContext)
        {
            var start = DateTime.Now;
            _logger.LogTrace($"logowanie użytkownika {loginModel.UserName}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var user = await _userManager.FindByNameAsync(loginModel.UserName);
                var isVaild = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, true);
                if (!isVaild.Succeeded)
                {
                    return SignInResult.Failed;
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return SignInResult.NotAllowed;
                }

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, loginModel.UserName));
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
                identity.AddClaim(new Claim("displayName", $"{user.FirstName} {user.LastName}"));
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    identity.AddClaim(new Claim(ClaimTypes.HomePhone, user.PhoneNumber));
                }
                identity.AddClaim(new Claim("Score", user.Score.ToString()));

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });

                return isVaild;
            }
            catch (Exception ex)
            {
                _logger.LogError($"nie można zalogować użytkownika {loginModel.UserName} - {ex}");
                throw ex;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"logowanie użytkownika {loginModel.UserName} ukończono w czasie {stopwatch.Elapsed}");
            }
        }

        public async Task SingOutUser(HttpContext httpContext)
        {
            await _signInManager.SignOutAsync();
            await httpContext.SignOutAsync(new AuthenticationProperties { IsPersistent = false });
            return;
        }
    }
}
