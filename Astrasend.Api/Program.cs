using System.Globalization;
using System.Reflection;
using Astrasend.Api.Configuration;
using Astrasend.DataLayer;
using Astrasend.Infrastructure.Np.Logging;
using Hellang.Middleware.ProblemDetails;
using Polly;

var builder = WebApplication.CreateBuilder(args);

const string appName = "Astrasend";

builder.WebHost.UseLogging(Assembly.GetExecutingAssembly().GetName().Name ?? appName,
        (_, _) => { }, configuration:builder.Configuration)
    .ConfigureBackgroundServices();

builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.ConfigureDatabase();
builder.Services.AddControllers();
ProblemDetailsExtensions.AddProblemDetails(builder.Services);
builder.Services.ConfigureTracer(appName);
builder.Services.ConfigureApiClients();
builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureHttpClients();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.ConfigureRabbitMqServices();

var app = builder.Build();

var cultureInfo = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseProblemDetails();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

InitializeDatabase(app);

app.MapControllers();

app.Run();


void InitializeDatabase(IApplicationBuilder applicationBuilder)
{
    var retryOnFailPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetry(3, _ => TimeSpan.FromSeconds(30));

    retryOnFailPolicy.Execute(applicationBuilder.MigrationDbContext<DataDbContext>);
}