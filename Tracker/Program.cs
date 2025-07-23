using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tracker;
using Tracker.Database;
using SimpleInjector;
using Tracker.Domain;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// configure routing to use lowercase to avoid hard to debug issues in production
// services.AddRouting(options => options.LowercaseUrls = true);
services.AddRouting(option =>
{
    option.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
});

// TODO: figure out where this belongs
services.AddMvcCore();
services.AddControllersWithViews();
// .AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.Converters.Add(new YearMonthConverter());
// });

// Sets up the basic configuration that for integrating Simple Injector with
// ASP.NET Core by setting the DefaultScopedLifestyle, and setting up auto
// cross wiring.
var container = new SimpleInjector.Container();
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
services.AddServerTiming();

// Add services to the container.
container.Register<DapperContext>(Lifestyle.Scoped);
container.Register(typeof(IQueryHandler<,>), typeof(IQueryHandler<,>).Assembly);
container.Register(typeof(IDbQueryHandler<,>), typeof(IDbQueryHandler<,>).Assembly);

container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly);
container.Register(typeof(IDbCommandHandler<>), typeof(IDbCommandHandler<>).Assembly);

// expose all DB query/command handlers as non-DB query/command handlers via the adapters
container.RegisterConditional(typeof(IQueryHandler<,>), typeof(DbQueryHandlerAdapter<,>), c => !c.Handled);
container.RegisterConditional(typeof(ICommandHandler<>), typeof(DbCommandHandlerAdapter<>), c => !c.Handled);

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
app.UseServerTiming();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller:slugify=Home}/{action:slugify=Index}/{id?}")
    .WithStaticAssets();

container.Verify();


app.Run();
