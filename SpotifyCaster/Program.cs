using DiscordBotCore.Extensions;
using SpotifyCaster;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddConfigs(builder.Configuration);
builder.Services.AddBackgroundServices();
builder.Services.AddServices();
builder.Services.AddDiscordBotCoreServices();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();