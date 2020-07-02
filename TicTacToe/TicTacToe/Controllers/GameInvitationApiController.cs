using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;
using TicTacToe.Services;

namespace TicTacToe.Controllers
{
    [Produces("application/json")]
    [Route("restapi/v1/GameInvitation")]
    [ApiController]
    public class GameInvitationApiController : Controller
    {
        private readonly IGameInvitationService _gameInvitationService;
        private IUserService _userService;

        public GameInvitationApiController(IGameInvitationService gameInvitationService, IUserService userService)
        {
            _gameInvitationService = gameInvitationService;
            _userService = userService;
        }

        // GET: restapi/v1/GameInvitation
        [HttpGet]
        public async Task<IEnumerable<GameInvitationModel>> Get()
        {
            return await _gameInvitationService.All();
        }

        // GET: restapi/v1/GameInvitation/{id}
        [HttpGet("{id}", Name = "Get")]
        public async Task<GameInvitationModel> Get(Guid id)
        {
            return await _gameInvitationService.Get(id);
        }

        // POST: restapi/v1/GameInvitation
        [HttpPost]
        public IActionResult Post([FromBody]GameInvitationModel invitation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invitedPlayer = _userService.GetUserByEmail(invitation.EmailTo);
            if (invitedPlayer == null)
                return BadRequest();

            _gameInvitationService.Add(invitation);
            return Ok();
        }


        // PUT: restapi/v1/GameInvitation/{id}
        [HttpPut("t/{id}")]
        public IActionResult Put(Guid id, [FromBody]GameInvitationModel invitation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invitedPlayer = _userService.GetUserByEmail(invitation.EmailTo);
            if (invitedPlayer == null)
                return BadRequest();

            _gameInvitationService.Update(invitation);
            return Ok();
        }

        // DELETE: restapi/v1/GameInvitation/{id}
        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            _gameInvitationService.Delete(id);
        }

        // GET: restapi/v1/GameInvitation/Confirmation/{id}
        [HttpGet("Confirmation/{id}")]
        public async Task<IActionResult> ConfirmGameInvitation(Guid id)
        {
            var gameInvitationModel = await _gameInvitationService.Get(id);
            gameInvitationModel.IsConfirmed = true;
            gameInvitationModel.ConfirmationDate = DateTime.Now;
            await _gameInvitationService.Update(gameInvitationModel);
            return Ok();
        }
    }
}
