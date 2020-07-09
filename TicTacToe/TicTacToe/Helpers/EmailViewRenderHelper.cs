using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TicTacToe.ViewEngines;

namespace TicTacToe.Helpers
{
    public class EmailViewRenderHelper
    {
        IHostEnvironment _hostEnvironment;
        IConfiguration _configurationRoot;
        IHttpContextAccessor _httpContextAccessor;

        public async Task<string> RenderTemplate<T>(string template,
            IHostEnvironment hostEnvironment, IConfiguration configurationRoot,
            IHttpContextAccessor httpContextAccessor, T model) where T : class
        {
            _hostEnvironment = hostEnvironment;
            _configurationRoot = configurationRoot;
            _httpContextAccessor = httpContextAccessor;
            var renderer = httpContextAccessor.HttpContext.RequestServices
                .GetRequiredService<IEmailViewEngine>();
            return await renderer.RenderEmailToString<T>(template, model);
        }
    }
}