using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private const string ApiKey = "2645d3a34171a029a0ec6d4265529d9a";
        private const string BaseUrl = "http://api.openweathermap.org/data/2.5/weather?q=";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GetWeather_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("Please enter a city name.");
                return;
            }

            string url = $"{BaseUrl}{city}&appid={ApiKey}&units=metric";
            string weatherData = await GetWeatherData(url);
            if (!string.IsNullOrEmpty(weatherData))
            {
                DisplayWeather(weatherData);
            }
            else
            {
                MessageBox.Show("Error fetching weather data.");
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

            WeatherLabel.Content = $"Weather: {description}\nTemperature: {temperature}°C";
        }
    }
}
