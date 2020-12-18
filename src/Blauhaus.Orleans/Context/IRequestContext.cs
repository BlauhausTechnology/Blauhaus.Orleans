namespace Blauhaus.Orleans.Context
{
    public interface IRequestContext
    {
        void Set(string key, object value);
        object Get(string key);
    }
}