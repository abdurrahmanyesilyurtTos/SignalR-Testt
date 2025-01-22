using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// SignalR ve Redis baðlantýsý ekleniyor
builder.Services.AddSignalR();

// Redis baðlantýsýný tanýmla
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect("localhost:6379");
});

// Redis veritabanýný SignalR Hub'da kullanmak üzere ekle
builder.Services.AddScoped(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});

var app = builder.Build();

// SignalR endpoint'i tanýmlanýyor
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<CodeCheckerHub>("/codeCheckerHub");
});

app.Run();
