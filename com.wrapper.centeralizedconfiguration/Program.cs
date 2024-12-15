using Azure.Core.Diagnostics;
using Azure.Identity;
using customlogging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Logging.AddAzureWebAppDiagnostics();
//builder.AddLogging(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureAppConfiguration();

using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();

builder.Configuration.AddAzureAppConfiguration(opt =>
{
    var cred = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
    {
        ManagedIdentityClientId = "d3cf4842-b720-46f9-a3f0-f57f78a8cefc"
    });
    opt.Connect(new Uri("https://pocappconfig.azconfig.io"), cred)
       .Select("Settings:*", null)
       .ConfigureRefresh(refOpt =>
       {
           refOpt.Register("Settings:Refresh", refreshAll: true);
           refOpt.SetRefreshInterval(TimeSpan.FromSeconds(60));
       });
    builder.Services.AddSingleton(opt.GetRefresher());

    opt.ConfigureKeyVault(kv =>
    {
        kv.SetCredential(cred);
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
