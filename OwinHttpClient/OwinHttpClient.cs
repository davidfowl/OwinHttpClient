using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin.Builder;
using Owin.Client.Middleware;

namespace Owin.Client
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
            app.Use(typeof(GzipMiddleware));
            app.Use(typeof(RedirectMiddleware), 5);
            app.Use(typeof(ConnectionCloseMiddleware));
            app.Use(typeof(ContentLengthMiddleware));
            app.Use(typeof(ChunkedMiddleware));
            app.Use(typeof(NoBodyMiddleware));
            app.Use(typeof(HttpRequestMiddleware), new NetworkStreamFactory());
        }
    }
}
