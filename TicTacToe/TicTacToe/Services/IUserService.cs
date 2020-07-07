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
        Task<string> GetEmailConfirmationCode(UserModel user);
        Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers);
        Task<UserModel> GetUserByEmail(string email);
        Task<bool> IsUserExisting(string email);
        Task<bool> RegisterUser(UserModel userModel);
        Task<SignInResult> SignInUser(LoginModel loginModel, HttpContext httpContext);
        Task SingOutUser(HttpContext httpContext);
        Task UpdateUser(UserModel userModel);
    }
}