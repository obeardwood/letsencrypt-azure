using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LetsEncrypt.Azure.FunctionV2
{
    public static class AutoRenewCertificate
    {
        [Function("AutoRenewCertificate")]
        public static async Task Run([TimerTrigger("%CertRenewSchedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Renewing certificate at: {DateTime.Now}");

            await Helper.InstallOrRenewCertificate(log);
        }
    }
}
