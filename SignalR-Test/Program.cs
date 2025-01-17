using SignalR_Test;

var builder = WebApplication.CreateBuilder(args);

// SignalR ve HttpClient hizmetlerini ekleyin
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICodeService, CodeService>();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<CodeCheckerHub>("/codeCheckerHub");
});

// Kod üretimini tetikleyin
using var scope = app.Services.CreateScope();
var codeService = scope.ServiceProvider.GetRequiredService<ICodeService>();
var generatedCode = await codeService.GenerateAndStoreCode();
Console.WriteLine($"Generated Code: {generatedCode}");

app.Run();
