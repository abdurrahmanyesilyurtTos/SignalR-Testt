using Microsoft.AspNetCore.SignalR;
using SignalR_Test;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class CodeCheckerHub : Hub
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static string GeneratedCode;

    public CodeCheckerHub(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Kod üret ve sakla
    public async Task GenerateAndStoreCode()
    {
        var httpClient = _httpClientFactory.CreateClient();

        try
        {
            var response = await httpClient.GetAsync("https://api.pakodemy.com/api/account/getlogincode");
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Generated Code API Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CodeGenerateResponse>();

                if (result != null && result.ResponseData != null)
                {
                    GeneratedCode = result.ResponseData.Code.ToString();
                    Console.WriteLine($"Generated Code: {GeneratedCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during code generation: {ex.Message}");
        }
    }

    // Konsoldan gelen sinyale göre CheckLogin API'sini çalıştır
    public async Task CheckLogin(string code)
    {

        var httpClient = _httpClientFactory.CreateClient();
        var request = new
        {
            Code = int.Parse(code),
            OSType = 1,
            DeviceType = 1,
            DeviceId = "baran"
        };

        try
        {
            var response = await httpClient.PostAsJsonAsync("https://api.pakodemy.com/api/account/checklogincode", request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CheckLogin API Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                await Clients.Caller.SendAsync("CodeValidated", "Success");
                Console.WriteLine("Code validated successfully.");
            }
            else
            {
                await Clients.Caller.SendAsync("CodeValidated", "Failed");
                Console.WriteLine("Code validation failed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during CheckLogin API call: {ex.Message}");
        }
    }
}
