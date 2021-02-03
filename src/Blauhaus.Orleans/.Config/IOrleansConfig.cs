namespace Blauhaus.Orleans.Config
{
    public interface IOrleansConfig
    {
        string AzureStorageConnectionString { get; }
        string ClusterName { get; } 
    }
}