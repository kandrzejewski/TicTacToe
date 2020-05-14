using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class GameSessionService : IGameSessionService
    {
        private readonly ConcurrentBag<GameSessionModel> _session;
        private readonly IUserService _UserService;

        public GameSessionService(IUserService userService)
        {
            _session = new ConcurrentBag<GameSessionModel>();
            _UserService = userService;
        }

        public Task<GameSessionModel> GetGameSession(Guid gameSessionId)
        {
            return Task.Run(() => _session
                .FirstOrDefault(x => x.Id == gameSessionId));
        }

        public async Task<GameSessionModel> CreateGameSession(Guid invitationId, string invitedByEmail, string invitedPlayerEmail)
        {
            var invitedBy = await _UserService.GetUserByEmail(invitedByEmail);
            var invitedPlayer = await _UserService.GetUserByEmail(invitedPlayerEmail);
            GameSessionModel session = new GameSessionModel
            {
                User1 = invitedBy,
                User2 = invitedPlayer,
                Id = invitationId,
                ActiveUser = invitedBy
            };
            _session.Add(session);
            return session;
        }
    }
}
