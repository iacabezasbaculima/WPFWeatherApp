using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Diagnostics;
using WPFApp.Models;

namespace WPFApp.Services.OpenWeather
{
	public class OpenWeatherMapService
	{
		private const string _APP_KEY = "cf57eb65fef30ff16cf331c040d18b32";
		private HttpClient _client;

		public enum QueryType { SINGLE_DAY, FIVE_DAYS }
		public OpenWeatherMapService()
		{
			_client = new HttpClient();
			_client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
		}
		public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(string location)
		{
			if (location == null) throw new ArgumentNullException("Location can't be null.");
			if (location == string.Empty) throw new ArgumentException("Location can't be an empty string.");

			var query = $"forecast?q={location}&type=accurate&units=metric&mode=xml&appid={_APP_KEY}";
		
			var response = await _client.GetAsync(query);

			switch (response.StatusCode)
			{
				case HttpStatusCode.Unauthorized:
					throw new Exception("Invalid API key.");
				case HttpStatusCode.NotFound:
					throw new Exception("Location not found.");
				case HttpStatusCode.OK:
					var s = await response.Content.ReadAsStringAsync();
					var x = XElement.Load(new StringReader(s));
					
						var data = x.Descendants("time").Select(w => new WeatherForecast
						{
							City = w.Parent.Parent.Element("location").Element("name").Value,
							Country = w.Parent.Parent.Element("location").Element("country").Value,
							Date = DateTime.Parse(w.Attribute("from").Value),
							SunSet = DateTime.Parse(w.Parent.Parent.Element("sun").Attribute("set").Value.Substring(11,8)),
							SunRise = DateTime.Parse(w.Parent.Parent.Element("sun").Attribute("rise").Value.Substring(11,8)),
							Description = w.Element("symbol").Attribute("name").Value,
							ImageId = w.Element("symbol").Attribute("var").Value,
							WindSpeed = double.Parse(w.Element("windSpeed").Attribute("mps").Value),
							CurrentTemperature = double.Parse(w.Element("temperature").Attribute("value").Value),
							MaxTemperature = double.Parse(w.Element("temperature").Attribute("max").Value),
							MinTemperature = double.Parse(w.Element("temperature").Attribute("min").Value),
							Pressure = int.Parse(w.Element("pressure").Attribute("value").Value),
							Humidity = int.Parse(w.Element("humidity").Attribute("value").Value)
						});
						return data;
				default:
					throw new NotImplementedException(response.StatusCode.ToString());
			}
		}

		public async Task<WeatherForecast> GetSingleForecastAsync(string location)
		{
			if (location == null) throw new ArgumentNullException("Location can't be null.");
			if (location == string.Empty) throw new ArgumentException("Location can't be an empty string.");

			var query = $"weather?q={location}&type=accurate&units=metric&mode=xml&appid={_APP_KEY}";
			var response = await _client.GetAsync(query);

			switch (response.StatusCode)
			{
				case HttpStatusCode.Unauthorized:
					throw new Exception("Invalid API key.");
				case HttpStatusCode.NotFound:
					throw new Exception("Location not found.");
				case HttpStatusCode.OK:
					var s = await response.Content.ReadAsStringAsync();
					var x = XElement.Load(new StringReader(s));

					WeatherForecast forecast = new WeatherForecast
					{
						City = x.Element("city").Attribute("name").Value,
						Country = x.Element("city").Element("country").Value,
						Date = DateTime.Parse(x.Element("lastupdate").Attribute("value").Value),
						SunSet = DateTime.Parse(x.Element("city").Element("sun").Attribute("set").Value),
						SunRise = DateTime.Parse(x.Element("city").Element("sun").Attribute("rise").Value),
						Description = x.Element("weather").Attribute("value").Value,
						CurrentTemperature = double.Parse(x.Element("temperature").Attribute("value").Value),
						MaxTemperature = double.Parse(x.Element("temperature").Attribute("max").Value),
						MinTemperature = double.Parse(x.Element("temperature").Attribute("min").Value),
						WindSpeed = double.Parse(x.Element("wind").Element("speed").Attribute("value").Value),
						Humidity = int.Parse(x.Element("humidity").Attribute("value").Value),
						Pressure = int.Parse(x.Element("pressure").Attribute("value").Value),
						IconId = int.Parse(x.Element("weather").Attribute("number").Value),
						ImageId = x.Element("weather").Attribute("icon").Value
					};
					return forecast;
				default:
					throw new NotImplementedException(response.StatusCode.ToString());
			}
		}
	}
}
