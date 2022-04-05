using Blauhaus.Common.ValueObjects.BuildConfigs;

namespace Blauhaus.Orleans.Config
{
    public abstract class OrleansConfiguration : IOrleansConfig
    {
        protected OrleansConfiguration(
            string azureAzureStorageConnectionString, 
            string clusterName, 
            IBuildConfig buildConfig, 
            int dashboardPort)
        {
            BuildConfig = buildConfig;
            DashboardPort = dashboardPort;
            AzureStorageConnectionString = azureAzureStorageConnectionString;

            ClusterName = clusterName;

            StorageTableName = clusterName.Replace("-", "");
            ServiceId = clusterName;
            ClusterId = clusterName;
        }

        public string ClusterName { get; }
        public string AzureStorageConnectionString { get; }
        public string StorageTableName { get; }
        public string ClusterId { get; }
        public string ServiceId { get; }
        public IBuildConfig BuildConfig { get; }
        public int DashboardPort { get; }
    }
}