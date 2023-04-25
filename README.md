# TestWebAssemblyAndGrpcWeb

Test project for configuring a WebAssembly application to interact with gRPC.

## Create a project

When creating a project, you need to do the following:
1. Create a Blazor WebAssembly app.
2. Select the ASP.NET Core hosting option.

After that, a solution containing 3 projects will be created: Client, Server and Shared.

## Installing required packages

Through the Nuget Package Manager, you need to install the following packages for the following projects.

For Shared Project:
1. Google.Protobuf
2. Grpc.Net.Client
3. Grpc.Tools

For Server Project:
1. Grpc.AspNetCore
2. Grpc.AspNetCore.Web

For Client Project:
1. Grpc.Net.Client.Web

After installing the packages, you can proceed to create the necessary files for application interaction.

## Application setup

### In the Shared project

You need to create a weather.proto file in the Protos folder.

```csharp
syntax = "proto3"; 

option csharp_namespace = "TestWebAssemblyAndGrpcWeb.Shared.Grpc"; // replace namespace with your own

package WeatherForecastPackage; 

import "google/protobuf/timestamp.proto"; 

service WeatherForecasts {
	rpc GetWeather (WeatherForecast) returns (WeatherReply); 
}

message WeatherReply {
	repeated WeatherForecast forecasts = 1; 
}

message WeatherForecast {
	google.protobuf.Timestamp dateTimeStamp = 1; 
	int32 temperatureC = 2; 
	string summary = 3; 
}
```

In the .csproj configuration file, you need to add these lines to generate the files.

```csharp
<ItemGroup>
  <Protobuf Include="Protos\*.proto" GrpcServices="Both" />
</ItemGroup>
```

Create the WeatherService class in the Services folder and make it public. We inherit it from the generated WeatherForecasts.WeatherForecastsBase classes and override their methods.

```csharp
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using TestWebAssemblyAndGrpcWeb.Shared.Grpc;

namespace TestWebAssemblyAndGrpcWeb.Shared.Services
{
	public class WeatherService : WeatherForecasts.WeatherForecastsBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		public override Task<WeatherReply> GetWeather(WeatherForecast request, ServerCallContext context)
		{
			var reply = new WeatherReply();
			var rng = new Random();

			reply.Forecasts.Add(Enumerable.Range(1, 10).Select(index => new WeatherForecast
			{
				DateTimeStamp = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now.AddDays(index), DateTimeKind.Utc)),
				TemperatureC = rng.Next(20, 55),
				Summary = Summaries[rng.Next(Summaries.Length)]
			}));

			return Task.FromResult(reply);
		}
	}
}
```

### In the Server project

The gRPC service needs to be registered.

```csharp
builder.Services.AddGrpc();
```

You then need to add the gRPC-Web middleware to your application configuration and register the gRPC service. It must be added after UseRouting and before UseEndpoints.

```csharp
app.UseGrpcWeb();
app.MapGrpcService<WeatherService>().EnableGrpcWeb();
```

### In the Client project

You need to add a gRPC channel that connects to the gRPC server using an HttpClient object.

```csharp
builder.Services.AddSingleton(services =>
{
    var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
    var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
    return new WeatherForecasts.WeatherForecastsClient(channel);
});
```

In the _Imports.razor file, add a link to the generated files from the Shared project.

```csharp
@using TestWebAssemblyAndGrpcWeb.Shared.Grpc
```

Let's create a FetchData.razor file in the Pages folder. And let's fill it.

```csharp
@page "/fetchdata" 

@inject WeatherForecasts.WeatherForecastsClient WeatherForecastsClient 

<h1>Weather forecast</h1> 
<p>This component demonstrates fetching data from the server.</p> 

@if (forecasts == null) 
{ 
	<p><em>Loading...</em></p> 
} 
else 
{ 
	<table class="table"> 
	<thead> 
	<tr> 
	<th>Date</th> 
	<th>Temp. (C)</th> 
	<th>Summary</th> 
	</tr> 
	</thead> 
	<tbody> 
	@foreach (var forecast in forecasts) 
	{ 
		<tr> 
		<td>@forecast.DateTimeStamp.ToDateTime().ToShortDateString()</td> 
		<td>@forecast.TemperatureC</td> 
		<td>@forecast.Summary</td> 
		</tr> 
	} 
	</tbody> 
	</table> 
} 

@code 
{ 
	private IList<WeatherForecast> forecasts; 
	protected override async Task OnInitializedAsync() 
	{ 
		forecasts = (await WeatherForecastsClient.GetWeatherAsync(new WeatherForecast())).Forecasts; 
	} 
}
```

Let's add a link to call the link page in the MainLayout.razor file.

```csharp
<NavLink href="" Match="NavLinkMatch.All">
    <span>Home</span> 
</NavLink>
<br />
<NavLink class="nav-link" href="fetchdata">
    <span>Fetch data</span>
</NavLink>
<br />
```

The minimum set for the application to work is configured.

## Publication

## Sources

The main information is taken from this [site](https://azure.github.io/AppService/2021/03/15/How-to-use-gRPC-Web-with-Blazor-WebAssembly-on-App-Service.html#configure-grpc-web-in-the-server) and supplemented with changes.