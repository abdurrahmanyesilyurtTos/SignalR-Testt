using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// SignalR ve Redis ba�lant�s� ekleniyor
builder.Services.AddSignalR();

// Redis ba�lant�s�n� tan�mla
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect("localhost:6379");
});

// Redis veritaban�n� SignalR Hub'da kullanmak �zere ekle
builder.Services.AddScoped(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});

var app = builder.Build();

// SignalR endpoint'i tan�mlan�yor
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<CodeCheckerHub>("/codeCheckerHub");
});

app.Run();
