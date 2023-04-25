using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TestWebAssemblyAndGrpcWeb.Client;
using TestWebAssemblyAndGrpcWeb.Shared.Grpc;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(services =>
{
	var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
	var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
	var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
	return new WeatherForecasts.WeatherForecastsClient(channel);
});

await builder.Build().RunAsync();
