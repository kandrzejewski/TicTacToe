﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TicTacToe.Services;

namespace TicTacToe.Components
{
    [ViewComponent(Name ="GameSession")]
    public class GameSessionViewComponent: ViewComponent
    {
        readonly IGameSessionService _gameSessionService;
        
        public GameSessionViewComponent(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid gameSessionId)
        {
            var session = await _gameSessionService.GetGameSession(gameSessionId);
            return View(session);
        }
    }
}
