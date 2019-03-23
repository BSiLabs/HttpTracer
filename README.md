# HttpTracer
A simple http tracing library to write request and response information to your output window. Making your life easier when debugging http calls!

|Channel|Status|
| ------------------- | :------------------: |
|Build|[![Build status](https://burstingsilver.visualstudio.com/BSiLabs/_apis/build/status/HttpTracer)](https://burstingsilver.visualstudio.com/BSiLabs/_build/latest?definitionId=78)|
|NuGet.org |[![#](https://img.shields.io/nuget/v/httptracer.svg?style=flat)](https://www.nuget.org/packages/HttpTracer/)|

![](https://danielcauserblog.files.wordpress.com/2018/04/tracer_tracing_get1.gif)

**Platform Support**

Http Tracer is a .NET Standard 2.0 library.

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.iOS|iOS 7+|
|Xamarin.Android|API 14+|
|Windows 10 UWP|10.0.16299+|
|.NET Core|2.0+|
|ASP.NET Core|2.0+|
|.NET|4.6.1+|

## Getting Started

It is really easy to start using and debugging your Http requests, just add a instance of HttpTracerHandler to your HttpClient creation and start picking up the traces in your Visual Studio console window.

```csharp
using HttpTracer;
public async Task GetMyData()
{
    var tracer = new HttpTracerHandler();
    tracer.Verbosity = LogLevel.Information;
    var client = new HttpClient(tracer);
    var result = await client.GetAsync("http://myserviceurl.com");
}
```

If you happen to use custom Http Handlers in your project, we suggest you use our Http handler builder:

```csharp
using HttpTracer;
public async Task GetMyData()
{
    var builder = new HttpHandlerBuilder();

    builder.AddHandler(new MyHandler3())
           .AddHandler(new MyHandler2())
           .AddHandler(new MyHandler1());
           
    var tracer = builder.Build();
    tracer.Verbosity = LogLevel.Information;
    
    var client = new HttpClient(tracer);
    var result = await client.GetAsync("http://myserviceurl.com");
}
```

### License
Under MIT (see license file)

### Want To Support This Project?
All we ask is to be active by submitting bugs, features, and sending those pull requests down!