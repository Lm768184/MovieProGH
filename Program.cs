using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoviePro.Data;
using MoviePro.Services;
using Microsoft.AspNetCore.Hosting;
using MoviePro.Models.Settings;
using MoviePro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(ConnectionService.GetConnectionString(builder.Configuration),
                //using splitQUeries is a more efficient way than using default code
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddTransient<SeedService>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRemoteMovieService, TMDBMovieService>();
builder.Services.AddScoped<IDataMappingService, TMDBMappingService>();
builder.Services.AddSingleton<IImageService, BasicImageService>();
var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapDefaultControllerRoute();


var dataService = app.Services.CreateScope()
                              .ServiceProvider
                              .GetRequiredService<SeedService>();

await dataService.ManageDataAsync();

app.Run();