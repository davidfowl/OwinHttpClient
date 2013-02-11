using System.Collections.Generic;

namespace Owin
{
    internal static class OwinEnvironmentExtensions
    {
        internal static T Get<T>(this IDictionary<string, object> environment, string key)
        {
            object value;
            return environment.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}
