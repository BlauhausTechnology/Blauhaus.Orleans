using Microsoft.Extensions.Configuration;

namespace Blauhaus.Orleans.Config
{
    public abstract class OrleansConfiguration : IOrleansConfig
    {
        protected OrleansConfiguration(string azureAzureStorageConnectionString, string clusterName)
        {
            ClusterName = clusterName;
            AzureStorageConnectionString = azureAzureStorageConnectionString;
        }


        public string AzureStorageConnectionString { get; }
        public string ClusterName { get; } 
    }
}