using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using TestWebAssemblyAndGrpcWeb.Shared.Grpc;

namespace TestMAUIBlazorAndGrpcWeb
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Services.AddMauiBlazorWebView();

			#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
			#endif

			builder.Services.AddSingleton(services =>
			{
				var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
				var baseUri = false ? "https://localhost:7247/" : "https://filatik.bsite.net/";
				var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
				return new WeatherForecasts.WeatherForecastsClient(channel);
			});

			return builder.Build();
		}
	}
}