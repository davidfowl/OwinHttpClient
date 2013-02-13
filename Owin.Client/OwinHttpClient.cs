using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin.Builder;
using Owin.Client.Middleware;
using Owin.Types;

namespace Owin.Client
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OwinHttpClient : IDisposable
    {
        private readonly AppFunc _appFunc;
        private readonly CancellationTokenSource _shutdownTokenSource;

        public OwinHttpClient()
            : this(ConfigureDefaultMiddleware)
        {
        }

        public OwinHttpClient(Action<IAppBuilder> build)
        {
            var app = new AppBuilder();
            _shutdownTokenSource = new CancellationTokenSource();

            // Some default properties (this is the instance name so make it unique per instance)
            app.Properties["server.Capabilities"] = new Dictionary<string, object>();
            app.Properties["host.AppName"] = "OwinHttpClient(" + GetHashCode() + ")";
            app.Properties["host.OnAppDisposing"] = _shutdownTokenSource.Token;

            build(app);

            _appFunc = (AppFunc)app.Build(typeof(AppFunc));
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _appFunc(environment);
            RewindResponseBody(environment);
        }

        private static void RewindResponseBody(IDictionary<string, object> environment)
        {
            var response = new OwinResponse(environment);

            var ms = response.Body as MemoryStream;
            if (ms != null)
            {
                // Rewind
                ms.Seek(0, SeekOrigin.Begin);
            }
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

        public void Dispose()
        {
            _shutdownTokenSource.Cancel();

            _shutdownTokenSource.Dispose();
        }
    }
}
