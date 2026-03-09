using NETmessenger.Infrastructure;
using NETmessenger.Web.Hubs;

const string DevClientCorsPolicy = "DevClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevClientCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5066",
                "https://localhost:5066",
                "http://localhost:5067",
                "https://localhost:5067",
                "http://127.0.0.1:5066",
                "https://127.0.0.1:5066",
                "http://127.0.0.1:5067",
                "https://127.0.0.1:5067")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(DevClientCorsPolicy);

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
