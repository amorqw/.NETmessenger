using NETmessenger.Web.Hubs;
using NETmessenger.Web.Infrastructure;
using NETmessenger.Web.Services.Chats;
using NETmessenger.Web.Services.Messages;
using NETmessenger.Web.Services.Users;

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
                "http://127.0.0.1:5066",
                "https://127.0.0.1:5066")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<InMemoryMessengerStore>();
builder.Services.AddSingleton<IUserService, InMemoryUserService>();
builder.Services.AddSingleton<IChatService, InMemoryChatService>();
builder.Services.AddSingleton<IMessageService, InMemoryMessageService>();

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
