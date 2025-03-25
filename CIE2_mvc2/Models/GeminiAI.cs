using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class GeminiAI
{
    private static readonly string apiKey = "AIzaSyCUnr8eqh1CFagpzxEuEOyyEBLuoEBGLSE"; // Replace with your actual API key
    private static readonly string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateText";

    public static async Task<string> AskGeminiAsync(string userQuestion)
    {
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                prompt = new { text = userQuestion }
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{endpoint}?key={apiKey}", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                return result?.candidates[0]?.output ?? "No response from AI.";
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }
    }
}
