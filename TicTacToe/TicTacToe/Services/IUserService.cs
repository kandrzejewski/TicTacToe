using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public interface IUserService
    {
        Task DeleteUser(string userId);
        Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers);
        Task<UserModel> GetUserByEmail(string email);
        Task<bool> IsUserExisting(string email);
        Task<bool> RegisterUser(UserModel userModel);
        Task UpdateUser(UserModel userModel);
    }
}