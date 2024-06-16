using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private const string ApiKey = "0aac235af5e24577b2d130100241606";
        private const string BaseUrl = "http://api.weatherapi.com/v1/current.json";
        private const string ForecastUrl = "http://api.weatherapi.com/v1/forecast.json"; // Updated forecast URL
        private const string IconBaseUrl = "http:"; // Fixing the icon URL base
        private const string GeoIpUrl = "http://ip-api.com/json/";
        private DispatcherTimer weatherTimer;

        public MainWindow()
        {
            InitializeComponent();
            FetchWeatherData();
            weatherTimer = new DispatcherTimer();
            weatherTimer.Interval = TimeSpan.FromMinutes(15);
            weatherTimer.Tick += WeatherTimer_Tick;
            weatherTimer.Start();
        }

        private async void WeatherTimer_Tick(object sender, EventArgs e)
        {
            await FetchWeatherData();
        }

        private async Task FetchWeatherData()
        {
            var location = await GetLocation();
            if (location.Item1 == 0 && location.Item2 == 0)
            {
                MessageBox.Show("Error fetching location.");
                return;
            }

            // Update the location label with the fetched city name
            LocationLabel.Content = $"Location: {location.Item3}";

            string weatherUrl = $"{BaseUrl}?key={ApiKey}&q={location.Item3}&aqi=yes";
            string weatherData = await GetWeatherData(weatherUrl);
            if (!string.IsNullOrEmpty(weatherData))
            {
                DisplayWeather(weatherData);
            }
            else
            {
                MessageBox.Show("Error fetching weather data.");
            }

            string forecastUrl = $"{ForecastUrl}?key={ApiKey}&q={location.Item3}&days=7&aqi=yes";
            string forecastData = await GetWeatherData(forecastUrl);
            if (!string.IsNullOrEmpty(forecastData))
            {
                DisplayForecast(forecastData);
            }
            else
            {
                MessageBox.Show("Error fetching weather forecast data.");
            }
        }

        private async Task<(double, double, string)> GetLocation()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(GeoIpUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject locationJson = JObject.Parse(responseBody);
                    double lat = (double)locationJson["lat"];
                    double lon = (double)locationJson["lon"];
                    string city = locationJson["city"].ToString();
                    return (lat, lon, city);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Request failed: {ex.Message}");
                    return (0, 0, string.Empty);
                }
            }
        }

        private async Task<string> GetWeatherData(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Request failed: {ex.Message}");
                    return null;
                }
            }
        }

        private void DisplayWeather(string weatherData)
        {
            JObject weatherJson = JObject.Parse(weatherData);
            string description = weatherJson["current"]["condition"]["text"].ToString();
            string temperature = weatherJson["current"]["temp_c"].ToString();
            string iconCode = weatherJson["current"]["condition"]["icon"].ToString();
            string iconUrl = $"{IconBaseUrl}{iconCode}"; // Update icon URL to include full path

            WeatherLabel.Content = $"Weather: {description}\nTemperature: {temperature}°C";
            WeatherIcon.Source = new BitmapImage(new Uri(iconUrl));
        }

        private void DisplayForecast(string forecastData)
        {
            JObject forecastJson = JObject.Parse(forecastData);
            var forecastList = forecastJson["forecast"]["forecastday"];
            var forecastTable = new List<WeatherForecast>();

            foreach (var forecast in forecastList)
            {
                string dateTime = forecast["date"].ToString();
                string temp = forecast["day"]["avgtemp_c"].ToString();
                string description = forecast["day"]["condition"]["text"].ToString();
                string iconCode = forecast["day"]["condition"]["icon"].ToString();
                string iconUrl = $"{IconBaseUrl}{iconCode}"; // Update icon URL to include full path

                forecastTable.Add(new WeatherForecast
                {
                    DateTime = dateTime,
                    Temperature = $"{temp} °C",
                    Description = description,
                    IconUrl = iconUrl
                });
            }

            ForecastDataGrid.ItemsSource = forecastTable;
        }

        public class WeatherForecast
        {
            public string DateTime { get; set; }
            public string Temperature { get; set; }
            public string Description { get; set; }
            public string IconUrl { get; set; }
        }
    }
}
