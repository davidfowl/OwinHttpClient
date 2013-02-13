# OwinHttpClient

A bare bones, low ceremony, http client. There's a single method, Invoke that takes an owin environment
that gets mutated with the response when the call to invoke is completed.

### Middleware 

The client takes advantage of Owin middleware to build up functionality. This of course is completely optional but the list of default
middleware supports making http requests via sockets, ssl support, gzipped responses, handling automatic redirects and more...

See the list of middlware [here](https://github.com/davidfowl/OwinHttpClient/tree/master/OwinHttpClient/Middleware)

**NOTE: The api is still in flux and it will be until it feels right.**

### Sample

The sample uses Owin.Types from http://www.myget.org/F/owin/ for OwinResponse.

```csharp
var env = Request.Get("http://www.reddit.com");

var client = new OwinHttpClient();

await client.Invoke(env);

var response = new OwinResponse(env);

var reader = new StreamReader(response.Body);
Console.WriteLine(await reader.ReadToEndAsync());
```
