using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace LetsEncrypt.Azure.FunctionV2
{
    public static class RequestWildcardCertificate
    {
        [Function("RequestWildcardCertificate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                await Helper.InstallOrRenewCertificate(log);

                return new OkResult();
            } catch(Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
                
            }
        }
    }
}
