using Latta_CSharp;
using Latta_CSharp.models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LattaASPNet
{
    public class LattaMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly LattaAPI _lattaAPI;
        private LattaInstance? _lattaInstance;
        private string? relationID;

        public LattaMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _lattaAPI = new LattaAPI(configuration["LATTA_APIKEY"]);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            InitRecording(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(context, ex);
            }
        }

        public void InitRecording(HttpContext context)
        {
            string filePath = "latta-instance.txt";
            if (!File.Exists(filePath))
            {
                _lattaInstance = _lattaAPI.putInstance(
                        Environment.OSVersion.Platform.ToString(),
                        Environment.OSVersion.Version.ToString(),
                        CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                        "server",
                        "ASP.NET Core",
                        Environment.Version.Build.ToString()
                    );

                if (_lattaInstance != null)
                    File.WriteAllText(filePath, _lattaInstance.id);
            }
            else
            {
                _lattaInstance = new LattaInstance();
                _lattaInstance.id = File.ReadAllText(filePath);
            }

            relationID = context.Request.Cookies["Latta-Recording-Relation-Id"]
                             ?? context.Request.Headers["Latta-Recording-Relation-Id"];

            if (string.IsNullOrEmpty(relationID))
            {
                relationID = Guid.NewGuid().ToString();
                context.Response.Cookies.Append("Latta-Recording-Relation-Id", relationID, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(10),
                    Path = "/"
                });
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (_lattaInstance != null)
            {
                var attachment = new LattaAttachment(exception, context, LattaLogger.getLogs());
                var snapshot = _lattaAPI.putSnapshot(_lattaInstance, "", null, relationID);

                if (snapshot != null)
                    _lattaAPI.putAttachment(snapshot, attachment);
            }
        }
    }
}
