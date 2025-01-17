using static CodeCheckerHub;

namespace SignalR_Test
{
    public class CodeService : ICodeService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CodeService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GenerateAndStoreCode()
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
                    return result?.ResponseData?.Code.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during code generation: {ex.Message}");
            }

            return "hata var";
        }

        public async Task<bool> CheckLogin(string code)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var request = new
            {
                Code = int.Parse(code),
                OSType = 1,            // Sabit değer
                DeviceType = 1,        // Sabit değer
                DeviceId = "baran"     // Sabit değer
            };

            try
            {
                // CheckLoginCode API'sine POST isteği gönder
                var response = await httpClient.PostAsJsonAsync("https://api.pakodemy.com/api/account/checklogincode", request);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CheckLoginCode API Response: {responseContent}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during CheckLogin API call: {ex.Message}");
                return false;
            }
        }
    }
    public class CodeGenerateResponse
    {
        public CodeGenerateResponseData ResponseData { get; set; }
    }

    public class CodeGenerateResponseData
    {
        public int Code { get; set; }
        public int ExpireTime { get; set; }
    }

}
