using System;
using System.Net.Http;
using System.Threading.Tasks;

public class WebAPIManager
{
    private readonly HttpClient client = new HttpClient();
    private const string apiUrl = "http://localhost:55555";
    
    public static WebAPIManager Instance { get; } = new WebAPIManager();

    public string GetApiUrl()
    {
        return apiUrl;
    }
    public HttpClient GetHttpClient()
    {
        return client;
    }

    public async Task InitAPI()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Web API Database started at http://localhost:55555/");
            }
            else
            {
                Console.WriteLine($"Web API Database error: {response.StatusCode}");
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Web API Database: Connect fail! " + ex.Message);
        }
    }

    public async Task<string> PostAsync(string endpoint)
    {
        try
        {
            string url = endpoint.StartsWith("https") ? endpoint : $"{apiUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            HttpResponseMessage response = await client.PostAsync(url, null);

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("POST API lỗi: " + ex.Message);
            return null;
        }
    }
    public async Task<string> PostAsync(string endpoint, HttpContent content)
    {
        try
        {
            string url = endpoint.StartsWith("https")
                ? endpoint
                : $"{apiUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            HttpResponseMessage response = await client.PostAsync(url, content);

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("POST API lỗi: " + ex.Message);
            return null;
        }
    }

}
