using ApiEnvio.Repositories;
using ApiEnvio.Repositories.Interfaces;
using ApiEnvio.Services;
using ApiEnvio.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Repack - API de Envio", Version = "v1" });
});

//CORS - libera o front React local
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "*")
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

//HttpClient para chamadas entre microsserviços (API de Pontuação)
builder.Services.AddHttpClient("ApiPontuacao", client =>
{
    client.BaseAddress = new Uri("http://localhost:5003");
});

//Injeção de dependência
builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();
builder.Services.AddScoped<IItemEnvioRepository, ItemEnvioRepository>();
builder.Services.AddScoped<IEnvioService, EnvioService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();   
app.Run();