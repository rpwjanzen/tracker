using Tracker;
using Tracker.Database;
using SimpleInjector;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Sets up the basic configuration that for integrating Simple Injector with
// ASP.NET Core by setting the DefaultScopedLifestyle, and setting up auto
// cross wiring.
var container = new SimpleInjector.Container();

// TODO: figure out where this belongs
services.AddMvcCore();
services.AddControllersWithViews();

services.AddSimpleInjector(container, options =>
{
    // AddAspNetCore() wraps web requests in a Simple Injector scope and
    // allows request-scoped framework services to be resolved.
    options.AddAspNetCore()

        // Ensure activation of a specific framework type to be created by
        // Simple Injector instead of the built-in configuration system.
        // All calls are optional. You can enable what you need. For instance,
        // ViewComponents, PageModels, and TagHelpers are not needed when you
        // build a Web API.
        .AddControllerActivation()
        .AddViewComponentActivation()
        // allow Razor pages to work
        .AddPageModelActivation()
        .AddTagHelperActivation();

    // Optionally, allow application components to depend on the non-generic
    // ILogger (Microsoft.Extensions.Logging) or IStringLocalizer
    // (Microsoft.Extensions.Localization) abstractions.
    // options.AddLogging();
    // options.AddLocalization();
});

// Add services to the container.
container.Register<DapperContext>(Lifestyle.Scoped);
container.Register(typeof(IQueryHandler<,>), typeof(IQueryHandler<,>).Assembly);
container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly);

var app = builder.Build();

// UseSimpleInjector() finalizes the integration process.
app.Services.UseSimpleInjector(container);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Always verify the container
container.Verify();

// init DB
// {
//     using var scope = container.CreateScope();
//     var context = container.GetRequiredService<DapperContext>();
//     context.Reset();
//     context.Init();
// }

app.Run();
