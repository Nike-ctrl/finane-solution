using Blazored.LocalStorage;
using Finanza.Database.Context;
using Finanze.AccessDatabase;
using Finanze.Web.Components;
using Finanze.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Carica il file appsettings.json per leggere le configurazioni applicative
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
// Registra la configurazione come singleton per renderla accessibile in tutta l'applicazione
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);


//Connessione al db
builder.Services.AddDbContextFactory<FinanzeContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionFinanze")));

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<StorageService>();

builder.Services.AddScoped<FinanzeAccessDatabaseService>();


builder.Services.AddSingleton<VersionStaticFileService>();


builder.Services.AddMudServices();





// Abilita i servizi di autenticazione e cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddScoped<HttpClient>();

builder.Services.AddSingleton<SenderService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var defaultCulture = new CultureInfo("it-CH"); // o la cultura che vuoi, es "en-US", "fr-FR", ecc

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);



app.MapControllers();

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
