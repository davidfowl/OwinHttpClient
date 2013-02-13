using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin.Builder;
using Owin.Middleware;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OwinHttpClient
    {
        private readonly AppFunc _appFunc;

        public OwinHttpClient()
            : this(app => { })
        {
        }

        public OwinHttpClient(Action<IAppBuilder> build)
        {
            var app = new AppBuilder();

            app.Properties["builder.DefaultApp"] = HttpRequestHandler.DefaultAppFunc;

            build(app);

            _appFunc = (AppFunc)app.Build(typeof(AppFunc));
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            return _appFunc(environment);
        }
    }
}
