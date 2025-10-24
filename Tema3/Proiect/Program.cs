using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Tema3;
using Tema3.Features.Products;
using Tema3.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc
    (
        "v1",
        new OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "API for managing products.",
            Contact = new OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com",


            }
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlite"Data Source=productmanagement.db"));
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<IValidator<CreateProductProfileRequest>, CreateProductProfileValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI
    (
        c=>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API V1");
            c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
            c.DisplayRequestDuration();
        }
    );
    
    app.MapOpenApi();
}


app.UseHttpsRedirection();


