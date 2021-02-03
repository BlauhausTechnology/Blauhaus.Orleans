using Blauhaus.Common.ValueObjects.BuildConfigs;

namespace Blauhaus.Orleans.Config
{
    public interface IOrleansConfig
    {
        string AzureStorageConnectionString { get; }
        string StorageTableName { get; } 
        string ClusterId { get; } 
        string ServiceId { get; } 
        IBuildConfig BuildConfig { get; }
        int DashboardPort { get; }
    }
}