using LetsEncrypt.Azure.Core.V2;
using LetsEncrypt.Azure.Core.V2.DnsProviders;
using LetsEncrypt.Azure.Core.V2.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace LetsEncrypt.Azure.FunctionV2
{
    public class Helper
    {
        /// <summary>
        /// Requests a Let's Encrypt wild card certificate using DNS challenge. 
        /// The DNS provider used is Azure DNS.
        /// The certificate is saved to Azure Key Vault.
        /// The Certificate is finally install to an Azure App Service. 
        /// Configuration values are stored in Environment Variables. 
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task InstallOrRenewCertificate(ILogger log)
        {
            var vaultBaseUrl = $"https://{Environment.GetEnvironmentVariable("Vault")}.vault.azure.net/";
            log.LogInformation("C# HTTP trigger function processed a request.");
            var Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            // var appCredentials = new ClientSecretCredential(
            //     Environment.GetEnvironmentVariable("AzureAppService__AzureSubscription__Tenant"), 
            //     Environment.GetEnvironmentVariable("AzureAppService__AzureSubscription__SubscriptionId"), 
            //     Environment.GetEnvironmentVariable("MYTHIC_APP_KEY"));
            var kvClient = new SecretClient(new Uri(vaultBaseUrl), new DefaultAzureCredential());

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ILogger>(log)
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
            var certificateConsumer = Configuration.GetValue<string>("CertificateConsumer");
            if (string.IsNullOrEmpty(certificateConsumer))
            {
                serviceCollection.AddAzureAppService(Configuration.GetSection("AzureAppService").Get<AzureWebAppSettings>());
            }
            else if (certificateConsumer.Equals("NullCertificateConsumer"))
            {
                serviceCollection.AddNullCertificateConsumer();
            }

            serviceCollection.AddSingleton(kvClient)
            .AddKeyVaultCertificateStore(vaultBaseUrl);


            serviceCollection.AddAcmeClient<AzureDnsProvider>(Configuration.GetSection("DnsSettings").Get<AzureDnsSettings>());

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var app = serviceProvider.GetService<LetsencryptService>();

            var dnsRequest = Configuration.GetSection("AcmeDnsRequest").Get<AcmeDnsRequest>();

            await app.Run(dnsRequest, Configuration.GetValue<int?>("RenewXNumberOfDaysBeforeExpiration") ?? 22);
        }
    }
}
