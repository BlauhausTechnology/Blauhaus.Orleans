using Blauhaus.Common.ValueObjects.BuildConfigs;
using Blauhaus.Orleans.Config;

namespace Blauhaus.Orleans.Ioc
{
    public class ClientConfig : OrleansConfiguration
    {
        public ClientConfig(
            string azureAzureStorageConnectionString, 
            string clusterName, 
            IBuildConfig buildConfig) : base(azureAzureStorageConnectionString, clusterName, buildConfig, 0)
        {
        }
    }
}