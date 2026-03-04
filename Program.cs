var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); // looks for api endpoints available and adds them to the Swagger documentation
builder.Services.AddSwaggerGen(); // generates the Swagger documentation based on the API endpoints discovered by AddEndpointsApiExplorer

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger(); // enables the middleware to serve the generated Swagger as a JSON endpoint.
    app.UseSwaggerUI(); // enables the middleware to serve the Swagger UI, which provides a web-based interface for exploring and testing the API endpoints defined in the Swagger documentation.
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();