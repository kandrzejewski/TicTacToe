using TicTacToe.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace TicTacToe.Middlewares
{
    public class CommunicationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserService _userService;

        public CommunicationMiddleware(RequestDelegate next, IUserService userService)
        {
            _next = next;
            _userService = userService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Equals("/CheckEmailConfirmationStatus"))
            {
                await ProcessEmailConfirmation(context);
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task ProcessEmailConfirmation(HttpContext context)
        {
            var email = context.Request.Query["email"];
            var user = await _userService.GetUserByEmail(email);

            if (string.IsNullOrEmpty(email))
            {
                await context.Response.WriteAsync("Nieprawidłowe żądanie: Wymagany jest adres e-mail");
            }
            else if ((await _userService.GetUserByEmail(email)).IsEmailConfirmed)
            {
                await context.Response.WriteAsync("OK");
            }
            else
            {
                await context.Response.WriteAsync("Oczekiwanie na potwierdzenie adresu e-mail");
                user.IsEmailConfirmed = true;
                user.EmailCofirmationDate = DateTime.Now;
                _userService.UpdateUser(user).Wait();
            }
        }
    }
}
