using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public interface IUserService
    {
        Task<bool> ConfirmEmail(string email, string code);
        Task DeleteUser(UserModel userModel);
        Task<IdentityResult> EnableTwoFactor(string name, bool enabled);
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent);
        Task<string> GetEmailConfirmationCode(UserModel user);
        Task<AuthenticationProperties> GetExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();
        Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers);
        Task<string> GetTwoFactorCode(string userName, string tokenProvider);
        Task<UserModel> GetUserByEmail(string email);
        Task<bool> IsUserExisting(string email);
        Task<bool> RegisterUser(UserModel userModel);
        Task<SignInResult> SignInUser(LoginModel loginModel, HttpContext httpContext);
        Task SingOutUser(HttpContext httpContext);
        Task UpdateUser(UserModel userModel);
        Task<bool> ValidateTwoFactor(string userName, string tokenProvider, string token, HttpContext httpContext);
    }
}