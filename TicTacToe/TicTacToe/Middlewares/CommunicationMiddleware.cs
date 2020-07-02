using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System;
using System.Net.WebSockets;
using System.IO;
using TicTacToe.Services;
using TicTacToe.Models;

namespace TicTacToe.Middlewares
{
    public class CommunicationMiddleware
    {
        private readonly RequestDelegate _next;

        public CommunicationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var ct = context.RequestAborted;
                var json = await ReceiveStringAsync(webSocket, ct);
                var command = JsonConvert.DeserializeObject<dynamic>(json);

                switch (command.Operation.ToString())
                {
                    case "CheckEmailConfirmationStatus":
                        {
                            await ProcessEmailConfirmation(context, webSocket, 
                                ct, command.Parameters.ToString());
                            break;
                        }
                    case "CheckGameInvitationConfirmationStatus":
                        {
                            await ProcessGameInvitationConfirmation(context, webSocket,
                                ct, command.Parameters.ToString());
                            break;
                        }
                }
            }
            else if (context.Request.Path.Equals("/CheckEmailConfirmationStatus"))
            {
                await ProcessEmailConfirmation(context);
            }
            else if (context.Request.Path.Equals("/CheckGameInvitationConfirmationStatus"))
            {
                await ProcessGameInvitationConfirmation(context);
            }
            else
            {
                await _next?.Invoke(context);
            }
        }

        private async Task ProcessEmailConfirmation(HttpContext context)
        {
            var userService = context.RequestServices.GetRequiredService<IUserService>();
            var email = context.Request.Query["email"];
            
            UserModel user = await userService.GetUserByEmail(email);

            if (string.IsNullOrEmpty(email))
            {
                await context.Response.WriteAsync("Nieprawidłowe żądanie: Wymagany jest adres e-mail");
            }
            else if ((await userService.GetUserByEmail(email)).IsEmailConfirmed)
            {
                await context.Response.WriteAsync("OK");
            }
        }

        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken ct = default)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        private static async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken ct = default)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (MemoryStream ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                    throw new Exception("Nieoczekiwany komunikat");

                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public async Task ProcessEmailConfirmation(HttpContext context, WebSocket webSocket, CancellationToken ct, string email)
        {
            var userService = context.RequestServices.GetRequiredService<IUserService>();
            var user = await userService.GetUserByEmail(email);
            while(!ct.IsCancellationRequested && !webSocket.CloseStatus.HasValue && user?.IsEmailConfirmed == false)
            {
                await SendStringAsync(webSocket, "WaitEmailConfirmation", ct);
                await Task.Delay(500);
                user = await userService.GetUserByEmail(email);
            }

            if (user.IsEmailConfirmed)
            {
                await SendStringAsync(webSocket, "OK", ct);
            }
        }

        //Przegląarki nie obsługujące WebSocket
        private async Task ProcessGameInvitationConfirmation(HttpContext httpContext)
        {
            var id = httpContext.Request.Query["id"];
            if (string.IsNullOrEmpty(id))
                await httpContext.Response.WriteAsync("Nieprawidłowe rządanie: Wymagany jest identyfikator id");
            
            var gameInvitationService = httpContext.RequestServices.GetService<IGameInvitationService>();
            var gameInvitationModel = await gameInvitationService.Get(Guid.Parse(id));
            
            if (gameInvitationModel.IsConfirmed)
                await httpContext.Response.WriteAsync(
                    JsonConvert.SerializeObject(new
                    {
                        Result = "OK",
                        Email = gameInvitationModel.InvitedBy,
                        gameInvitationModel.EmailTo
                    }));
            else
            {
                await httpContext.Response.WriteAsync("Oczekiwanie na potwierdzenie zaproszenie do gry");
            }
        }

        //Przeglądarki obsługujące WebSocket
        private async Task ProcessGameInvitationConfirmation(HttpContext httpContext, WebSocket webSocket,
            CancellationToken ct, string parameters)
        {
            var gameInvitationService = httpContext.RequestServices.GetService<IGameInvitationService>();
            var id = Guid.Parse(parameters);
            var gameInvitationModel = await gameInvitationService.Get(id);

            while (!ct.IsCancellationRequested &&
                !webSocket.CloseStatus.HasValue &&
                gameInvitationModel?.IsConfirmed == false)
            {
                await Task.Delay(500);
                gameInvitationModel = await gameInvitationService.Get(id);
                await SendStringAsync(webSocket, "WaitForConfirmation", ct);
            }

            if(gameInvitationModel.IsConfirmed)
            {
                await SendStringAsync(webSocket, JsonConvert.SerializeObject(new
                {
                    Result = "OK",
                    Email = gameInvitationModel.InvitedBy,
                    gameInvitationModel.EmailTo,
                    gameInvitationModel.Id
                }), ct);
            }
        }
    }
}
