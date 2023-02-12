using api.peter.biz.Models;
using api.peter.biz.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders()
  .AddConsole();

// Configure options
builder.Services.Configure<OAuthCredentials>(builder.Configuration.GetSection("OAuthCredentials"));

// Set up services
builder.Services.AddSingleton<TwitchService>();

builder.Services.AddHttpClient("TwitchOAuth", client =>
{
  client.BaseAddress = new Uri("https://id.twitch.tv/");
});

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Forward headers for hosting behind reverse proxy on Apache2
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/ping", () => "pong");

app.Run();
