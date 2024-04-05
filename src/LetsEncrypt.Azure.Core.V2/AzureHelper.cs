using LetsEncrypt.Azure.Core.V2.Models;
using System;
using Azure.Identity;
using Azure.ResourceManager;

namespace LetsEncrypt.Azure.Core.V2
{
    public class AzureHelper
    {
        public static AzureCredentials GetAzureCredentials(AzureServicePrincipal servicePrincipal, AzureSubscription azureSubscription)
        {
            if (servicePrincipal == null)
            {
                throw new ArgumentNullException(nameof(servicePrincipal));
            }

            if (azureSubscription == null)
            {
                throw new ArgumentNullException(nameof(azureSubscription));
            }

            if (servicePrincipal.UseManagendIdentity)
            {               
                return new AzureCredentials(new MSILoginInformation(MSIResourceType.AppService), Microsoft.Azure.Management.ResourceManager.Fluent.AzureEnvironment.FromName(azureSubscription.AzureRegion));
            }


            return new AzureCredentials(servicePrincipal.ServicePrincipalLoginInformation,
               azureSubscription.Tenant, Microsoft.Azure.Management.ResourceManager.Fluent.AzureEnvironment.FromName(azureSubscription.AzureRegion));
        }


        public static ArmClient GetClient(AzureServicePrincipal servicePrincipal, AzureSubscription azureSubscription)
        {            
            ClientSecretCredential credential = new ClientSecretCredential(azureSubscription.Tenant, servicePrincipal.ClientId, servicePrincipal.ClientSecret);
            return new ArmClient(credential, azureSubscription.SubscriptionId);
        }
    }
}
