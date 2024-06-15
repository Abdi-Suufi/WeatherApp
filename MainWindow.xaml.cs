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
        private const string ApiKey = "2645d3a34171a029a0ec6d4265529d9a";
        private const string BaseUrl = "http://api.openweathermap.org/data/2.5/weather";
        private const string ForecastUrl = "http://api.openweathermap.org/data/2.5/forecast";
        private const string IconBaseUrl = "http://openweathermap.org/img/wn/";
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

            string weatherUrl = $"{BaseUrl}?lat={location.Item1}&lon={location.Item2}&appid={ApiKey}&units=metric";
            string weatherData = await GetWeatherData(weatherUrl);
            if (!string.IsNullOrEmpty(weatherData))
            {
                DisplayWeather(weatherData);
            }
            else
            {
                MessageBox.Show("Error fetching weather data.");
            }

            string forecastUrl = $"{ForecastUrl}?lat={location.Item1}&lon={location.Item2}&appid={ApiKey}&units=metric";
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

        private async Task<(double, double)> GetLocation()
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
                    return (lat, lon);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Request failed: {ex.Message}");
                    return (0, 0);
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
            string description = weatherJson["weather"][0]["description"].ToString();
            string temperature = weatherJson["main"]["temp"].ToString();
            string iconCode = weatherJson["weather"][0]["icon"].ToString();
            string iconUrl = $"{IconBaseUrl}{iconCode}@2x.png";

            WeatherLabel.Content = $"Weather: {description}\nTemperature: {temperature}°C";
            WeatherIcon.Source = new BitmapImage(new Uri(iconUrl));
        }

        private void DisplayForecast(string forecastData)
        {
            JObject forecastJson = JObject.Parse(forecastData);
            var forecastList = forecastJson["list"];
            var forecastTable = new List<WeatherForecast>();

            foreach (var forecast in forecastList)
            {
                string dateTime = forecast["dt_txt"].ToString();
                string temp = forecast["main"]["temp"].ToString();
                string description = forecast["weather"][0]["description"].ToString();
                string iconCode = forecast["weather"][0]["icon"].ToString();
                string iconUrl = $"{IconBaseUrl}{iconCode}@2x.png";

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
