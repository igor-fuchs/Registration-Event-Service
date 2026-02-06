using RegistrationEventService.Application;
using RegistrationEventService.Infrastructure;
using RegistrationEventService.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (Clean Architecture layers)
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Add API controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "User Registration Event Service API",
        Version = "v1",
        Description = "Event-driven user registration API with AWS SNS integration."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Registration API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();