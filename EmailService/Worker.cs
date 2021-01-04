using EmailService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEmailSender _emailSender;

        public Worker(ILogger<Worker> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Working running at:{time}", DateTimeOffset.Now);
                using(var client =new HttpClient())
                {
                    var response = await client.GetAsync("");
                    string result = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation(result);
                    try
                    {
                        var email = JsonConvert.DeserializeObject<MessageDto>(result);
                        if (email != null)
                        {
                            var message = new Message(new string[] { "" }, "Email Servvice", email.JsonContent, "");
                            await _emailSender.SendEmail(message);
                            await client.GetAsync("" + email.Id);
                        }
                    }catch(Exception e) {
                        _logger.LogInformation(e.Message);
                    }
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
