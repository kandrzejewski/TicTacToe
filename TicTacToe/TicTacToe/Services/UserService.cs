using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class UserService : IUserService
    {
        private static ConcurrentBag<UserModel> _userStore;
        private DbContextOptions<GameDbContext> _dbContextOptions;

        public UserService(DbContextOptions<GameDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        static UserService()
        {
            _userStore = new ConcurrentBag<UserModel>();
        }
        public async Task<bool> RegisterUser(UserModel userModel)
        {
            //_userStore.Add(userModel);
            //return Task.FromResult(true);
            using(var db = new GameDbContext(_dbContextOptions))
            {
                db.UserModels.Add(userModel);
                await db.SaveChangesAsync();
                return true;
            }
        }
        public async Task<UserModel> GetUserByEmail(string email)
        {
            //return Task.FromResult(_userStore.FirstOrDefault(u => u.Email == email));
            using var db = new GameDbContext(_dbContextOptions);
            return await db.UserModels.FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task UpdateUser(UserModel userModel)
        {
            //_userStore = new ConcurrentBag<UserModel>(
            //    _userStore.Where(u => u.Email != userModel.Email))
            //{
            //    userModel
            //};
            //return Task.CompletedTask;
            using var db = new GameDbContext(_dbContextOptions);
            db.Update(userModel);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers)
        {
            //return Task.Run(() => (IEnumerable<UserModel>)_userStore
            //    .OrderBy(x => x.Score)
            //    .Take(numberOfUsers)
            //    .ToList());
            using var db = new GameDbContext(_dbContextOptions);
            return await db.UserModels.OrderByDescending(x => x.Score).ToListAsync();
        }

        public async Task<bool> IsUserExisting(string email)
        {
            using var db = new GameDbContext(_dbContextOptions);
            return await db.UserModels.AnyAsync(user => user.Email == email);
        }
    }
}
