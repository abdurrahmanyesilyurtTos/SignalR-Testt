using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System;
using System.Text;
using System.Threading.Tasks;

public class CodeCheckerHub : Hub
{
    private readonly IDatabase _redisCache;

    public CodeCheckerHub(IDatabase redisCache)
    {
        _redisCache = redisCache;
    }

    // Kod üretimi ve Redis'e kaydetme
    public async Task GenerateAndStoreCode()
    {
        Random random = new Random();
        string generatedCode = random.Next(100000, 999999).ToString();

        if (await _redisCache.KeyExistsAsync(generatedCode))
        {
            await Clients.Caller.SendAsync("Error", "Code generation failed. Try again.");
            return;
        }

        const int keyExpiryTime = 65;
        await _redisCache.StringSetAsync(generatedCode, string.Empty, TimeSpan.FromSeconds(keyExpiryTime));

        Console.WriteLine($"Generated Code: {generatedCode}");
        await Clients.Caller.SendAsync("CodeGenerated", new
        {
            Code = generatedCode,
            ExpireTime = keyExpiryTime - 5
        });
    }

    // Kod doğrulama
    public async Task CheckLogin(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            await Clients.Caller.SendAsync("Error", "Invalid code.");
            return;
        }

        string redisValue = await _redisCache.StringGetAsync(code);

        if (string.IsNullOrEmpty(redisValue))
        {
            await Clients.Caller.SendAsync("CodeValidated", new
            {
                Status = "Failed",
                Message = "Code not found or expired."
            });
            return;
        }

        // Token çözümleme işlemi
        string token = Encoding.ASCII.GetString(Convert.FromBase64String(redisValue));

        // Redis'ten kodu sil
        await _redisCache.KeyDeleteAsync(code);

        // Başarılı giriş bilgilerini istemciye gönder
        await Clients.Caller.SendAsync("CodeValidated", new
        {
            Status = "Success",
            Code = code,
            Token = token
        });

        Console.WriteLine($"Code {code} validated successfully with token {token}.");
    }


    // Kod ve token eşleme
    public async Task WebCodeLogin(string code, string userToken)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(userToken))
        {
            await Clients.Caller.SendAsync("Error", "Invalid input.");
            return;
        }

        if (!await _redisCache.KeyExistsAsync(code))
        {
            await Clients.Caller.SendAsync("Error", "Code not found.");
            return;
        }

        string hashedUserToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(userToken));
        const int keyExpiryTime = 65;
        await _redisCache.StringSetAsync(code, hashedUserToken, TimeSpan.FromSeconds(keyExpiryTime));

        await Clients.Caller.SendAsync("LoginSuccess", "Code and token mapped successfully.");
    }

    // Kod silme
    public async Task ClearStoredCode(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            await Clients.Caller.SendAsync("Error", "Invalid code.");
            return;
        }

        if (!await _redisCache.KeyExistsAsync(code))
        {
            await Clients.Caller.SendAsync("Error", "Code not found.");
            return;
        }

        await _redisCache.KeyDeleteAsync(code);
        await Clients.Caller.SendAsync("CodeCleared", "Code deleted successfully.");
    }
}
