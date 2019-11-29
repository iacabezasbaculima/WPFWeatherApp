using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFApp.Models;
using WPFApp.Services.OpenWeather;
using System.Diagnostics;
using WPFApp.Entities;
using System.IO;
using WPFApp.Utils;

namespace WPFApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<WeatherForecast> forecast { get; set; }
		private List<Location> cityList { get; set; }
		private WeatherForecast currentWeather;
		private WeatherForecast wTomorrow;
		private WeatherForecast wAfterTomorrow;
		private OpenWeatherMapService service;
		private Task _API_CALL;
		private string _iconFolderPath;
		private static OpenWeatherMapService.QueryType _searchFor = OpenWeatherMapService.QueryType.FIVE_DAYS;

		public MainWindow()
		{
			InitializeComponent();
			cityList = new List<Location>();
			forecast = new List<WeatherForecast>();
			service = new OpenWeatherMapService();
			tbx_Input.Focus();

			using (var db = new WeatherEntities())
			{
				cityList = db.Locations.ToList();
			}
			// Set the ListView source, i.e. collection
			lv_cities.ItemsSource = cityList;
			lv_cities.DisplayMemberPath = "City";
			ForecastPanel.Visibility = Visibility.Collapsed;
			CityListPanel.Visibility = Visibility.Visible;
			
			// Get app root folder path
			var appFolder = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString();
			// Set path of WeatherIcons
			_iconFolderPath = $"{appFolder}{"\\WeatherIcons\\"}";
		}

		public async Task GetWeather(string location)
		{
			try
			{
				#region Get and Filter Weather Data
				double maxtemp;
				double mintemp;
				switch (_searchFor)
				{
					case OpenWeatherMapService.QueryType.SINGLE_DAY:
						currentWeather = await service.GetSingleForecastAsync(location);
						forecast.Add(currentWeather);
						break;
					case OpenWeatherMapService.QueryType.FIVE_DAYS:
						var weather = await service.GetForecastAsync(location);
						currentWeather = weather.First();
						
						maxtemp = weather.Where(i => i.Date.Day == TimeOfDay.Tomorrow.Day).Where(i => i.Date.TimeOfDay == TimeOfDay.Midday.TimeOfDay).First().CurrentTemperature;
						mintemp = weather.Where(i => i.Date.Day == TimeOfDay.Tomorrow.Day).Where(i => i.Date.TimeOfDay == TimeOfDay.NinePM.TimeOfDay).First().CurrentTemperature;
						wTomorrow = weather.Where(i => i.Date.Day == TimeOfDay.Tomorrow.Day).FirstOrDefault();
						wTomorrow.MaxTemperature = maxtemp > mintemp ? Math.Round(maxtemp) : Math.Round(mintemp);
						wTomorrow.MinTemperature = mintemp < maxtemp ? Math.Round(mintemp) : Math.Round(maxtemp);

						maxtemp = weather.Where(i => i.Date.Day == TimeOfDay.AfterTomorrow.Day).Where(i => i.Date.TimeOfDay == TimeOfDay.Midday.TimeOfDay).First().CurrentTemperature;
						mintemp = weather.Where(i => i.Date.Day == TimeOfDay.AfterTomorrow.Day).Where(i => i.Date.TimeOfDay == TimeOfDay.NinePM.TimeOfDay).First().CurrentTemperature;
						wAfterTomorrow = weather.Where(i => i.Date.Day == TimeOfDay.AfterTomorrow.Day).First();
						wTomorrow.MaxTemperature = maxtemp > mintemp ? Math.Round(maxtemp) : Math.Round(mintemp);
						wTomorrow.MinTemperature = mintemp < maxtemp ? Math.Round(mintemp) : Math.Round(maxtemp);

						maxtemp = weather.Where(i => i.Date.Day == TimeOfDay.Today.Day).Where(i => i.Date.TimeOfDay == TimeOfDay.Midday.TimeOfDay).First().CurrentTemperature;
						mintemp = weather.Where(i => i.Date.Day == TimeOfDay.Today.Day).Where(i => i.Date.TimeOfDay == TimeOfDay.NinePM.TimeOfDay).First().CurrentTemperature;
						currentWeather = weather.Where(i => i.Date.Day == TimeOfDay.Today.Day).First();
						currentWeather.MaxTemperature = maxtemp > mintemp ? Math.Round(maxtemp) : Math.Round(mintemp);
						currentWeather.MinTemperature = mintemp < maxtemp ? Math.Round(mintemp) : Math.Round(maxtemp);
						break;
				}
				#endregion

				#region Update UI
				CityName.Text = currentWeather.City;
				WeatherIcon.Source = new BitmapImage(new Uri($"{_iconFolderPath}{currentWeather.ImageId}@2x.png"));
				Description.Text = currentWeather.Description.First().ToString().ToUpper() + currentWeather.Description.Substring(1);
				Date.Text = currentWeather.Date.ToString("MM/dd/yyyy");
				Temp.Text = $"{Math.Round(currentWeather.CurrentTemperature).ToString()}";
				MaxTemp.Text = $"{Math.Round(currentWeather.MaxTemperature).ToString()}{"\u00B0"}";
				MinTemp.Text = $"{Math.Round(currentWeather.MinTemperature).ToString()}{"\u00B0"}";
				WindSpeed.Text = $"{currentWeather.WindSpeed.ToString()} m/s";
				Humidity.Text = $"{currentWeather.Humidity} %";
				Pressure.Text = $"{currentWeather.Pressure} hPa";

				// Tomorrow UI
				TomorrowDay.Text = wTomorrow.Date.DayOfWeek.ToString().Substring(0,3);
				TomorrowDate.Text = wTomorrow.Date.ToString("dd/MM");
				TomorrowTemp.Text = $"{Math.Round(wTomorrow.MaxTemperature).ToString()} / {Math.Round(wTomorrow.MinTemperature).ToString()}";
				TomorrowIcon.Source = new BitmapImage(new Uri($"{_iconFolderPath}{wTomorrow.ImageId}@2x.png"));
				// AfterTomorrow UI
				AfterTomorrowDay.Text = wAfterTomorrow.Date.DayOfWeek.ToString().Substring(0, 3);
				AfterTomorrowDate.Text = wAfterTomorrow.Date.ToString("dd/MM");
				AfterTomorrowTemp.Text = $"{Math.Round(wAfterTomorrow.MaxTemperature).ToString()} / {Math.Round(wAfterTomorrow.MinTemperature).ToString()}";
				AfterTomorrowIcon.Source = new BitmapImage(new Uri($"{_iconFolderPath}{wAfterTomorrow.ImageId}@2x.png"));

				var inputCity = new Location { City = location };
				// Check if a city already exists in the list before we add it 
				bool check = cityList.Any(i => i.City == inputCity.City);

				#endregion

				#region Adding City To Database
				if (!check)
				{
					using (var db = new WeatherEntities())
					{
						db.Locations.Add(inputCity);
						db.SaveChanges();
						lv_cities.ItemsSource = null;
						cityList.Add(inputCity);
						lv_cities.ItemsSource = cityList;
						lv_cities.DisplayMemberPath = "City";
					}
				} 
				#endregion

				#region Swap Panels
				CityListPanel.Visibility = Visibility.Collapsed;
				ForecastPanel.Visibility = Visibility.Visible;
				#endregion
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		private void btn_CityListClick(object sender, RoutedEventArgs e)
		{
			ForecastPanel.Visibility = Visibility.Collapsed;
			CityListPanel.Visibility = Visibility.Visible;

			tbx_Input.Text = "";	// clear textbox content
			tbx_Input.Focus();		// set cursor in textbox
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (CityListPanel.Visibility == Visibility.Visible)
				{
					if (tbx_Input.Text != string.Empty)
					{
						// Get Data
						_API_CALL = GetWeather(tbx_Input.Text);
					}
				}
			}
		}

		private void lv_cities_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Location city = (Location)lv_cities.SelectedItem;

			if (city != null)
			{
				if (city.City != string.Empty)
				{
					_API_CALL = GetWeather(city.City);
				}
			}

		}
	}
}
