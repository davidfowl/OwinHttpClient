# OwinHttpClient

A bare bones, low ceremony, http client. There's a single method, Invoke that takes an owin environment
that gets mutated with the response when the call to invoke is completed. It's fulling async using the socket API (no wininet/winhttp).

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
