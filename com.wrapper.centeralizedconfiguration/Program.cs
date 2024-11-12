using Azure.Identity;
using customlogging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Logging.AddAzureWebAppDiagnostics();
builder.AddLogging(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureAppConfiguration();
builder.Configuration.AddAzureAppConfiguration(opt =>
{
    opt.Connect("Endpoint=https://pocappconfig.azconfig.io;Id=Inoy;Secret=ChbHpqtmee2JMjbMcXYw8liR0amYQp0GOngiRFA2ltInosqhkpPjJQQJ99AIACYeBjFPa7ZeAAACAZACVftz")
       .Select("Testing:*", null)
       .ConfigureRefresh(refOpt =>
       {
           refOpt.Register("Testing:Settings:Refresh", refreshAll: true);
           refOpt.SetRefreshInterval(TimeSpan.FromSeconds(60));
       });
    builder.Services.AddSingleton(opt.GetRefresher());

    opt.ConfigureKeyVault(kv =>
    {
        kv.SetCredential(new ClientSecretCredential("b845856f-adbb-4e37-9e00-9fed123512ee", "5da4786e-3f6a-4cb0-b75f-e9da632fd888", "1Tz8Q~IRL72NguBqE4xCnzBx-O9I2GaaPzsDFaDO"));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseLogContext();

app.UseAuthorization();

app.MapControllers();

app.Run();
