using ClipDown.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ImageConverter>();
builder.Services.AddSingleton<VideoConverter>();
builder.Services.AddSingleton<VideoDownloader>();
builder.Services.AddControllers();

// Libera o frontend (origens em appsettings.json) para chamar a API pelo navegador.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    // Sem isso o navegador esconde o nome do arquivo escolhido pelo servidor.
    .WithExposedHeaders("Content-Disposition")));

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
