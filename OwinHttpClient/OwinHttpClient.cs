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
            : this(ConfigureDefaultMiddleware)
        {
        }

        public OwinHttpClient(Action<IAppBuilder> build)
        {
            var app = new AppBuilder();

            build(app);

            _appFunc = (AppFunc)app.Build(typeof(AppFunc));
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            return _appFunc(environment);
        }

        public static void ConfigureDefaultMiddleware(IAppBuilder app)
        {
            // The order matters!
            app.Use(typeof(GzipHandler));
            app.Use(typeof(RedirectHandler), 5);
            app.Use(typeof(ConnectionCloseHandler));
            app.Use(typeof(ContentLengthHandler));
            app.Use(typeof(ChunkedHandler));
            app.Use(typeof(NoBodyHandler));
            app.Use(typeof(HttpRequestHandler), new NetworkStreamFactory());
        }
    }
}
