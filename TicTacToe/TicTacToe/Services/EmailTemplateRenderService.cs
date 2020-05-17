using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Helpers;

namespace TicTacToe.Services
{
    public class EmailTemplateRenderService : IEmailTemplateRenderService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailTemplateRenderService(IHostEnvironment hostEnvironment,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> RenderTemplate<T>(string templateName,
            T model, string host) where T : class
        {
            var html = await new EmailViewRenderHelper()
                .RenderTemplate(templateName, _hostEnvironment,
                    _configuration, _httpContextAccessor, model);
            var targetDir = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "Emails");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            string dateTime = DateTime.Now.ToString("ddMMHHyyHHmmss");
            var targetFileName = Path.Combine(targetDir, templateName
                .Replace("/", "_")
                .Replace("\\", "_") + "." + dateTime + ".html");
            html = html.Replace("{ViewOnLine}",
                $"{host.TrimEnd('/')}/Emails/{Path.GetFileName(targetFileName)}");
            html = html.Replace("{ServerUrl}", host);
            File.WriteAllText(targetFileName, html);
            return html;
        }
    }
}
