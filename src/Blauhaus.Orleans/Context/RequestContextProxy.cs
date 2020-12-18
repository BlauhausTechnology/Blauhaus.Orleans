using Orleans.Runtime;

namespace Blauhaus.Orleans.Context
{
    public class RequestContextProxy : IRequestContext
    {
        public void Set(string key, object value)
        {
            RequestContext.Set(key, value);
        }

        public object Get(string key)
        {
            return RequestContext.Get(key);
        }
    }
}