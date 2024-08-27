using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ColaApps.Services
{
    public class FirebaseService
    {
        private readonly string firebaseDatabaseUrl = "https://colaapp-c983e-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly string apiKey = "AIzaSyD9cnew6nBaJ5g6Mzn0YPKsmvIveM842bw";

        public async Task<dynamic> ReadDataAsync(string path)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"{firebaseDatabaseUrl}/{path}.json?key={apiKey}";
                var response = await httpClient.GetStringAsync(url);

                Debug.WriteLine("Firebase Data: " + response);

                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }
        public async Task<dynamic> ReadStoresAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"{firebaseDatabaseUrl}/stores.json?key={apiKey}";
                var response = await httpClient.GetStringAsync(url);

                Debug.WriteLine("Firebase Stores Data: " + response);

                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }

        public async Task WriteDataAsync(string path, object data)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"{firebaseDatabaseUrl}/{path}.json?key={apiKey}";
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                Debug.WriteLine("Data written successfully.");
            }
        }
    }
}
