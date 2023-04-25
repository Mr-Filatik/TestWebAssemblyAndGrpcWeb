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
				//Timestamp to DateTime => date.ToDateTime()
				//DateTime to Timestamp => Timestamp.FromDateTime(DateTime.SpecifyKind(date, DateTimeKind.Utc))
				DateTimeStamp = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now.AddDays(index), DateTimeKind.Utc)),
				TemperatureC = rng.Next(20, 55),
				Summary = Summaries[rng.Next(Summaries.Length)]
			}));

			return Task.FromResult(reply);
		}
	}
}
