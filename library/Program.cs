using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OneBeyondApi;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model.Dtos;
using OneBeyondApi.Model;
using OneBeyondApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBorrowerRepository, BorrowerRepository>();
builder.Services.AddScoped<ICatalogueRepository, CatalogueRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LibraryContext>(options => options.UseInMemoryDatabase("LibraryDb"));
builder.Services.AddTransient<SeedData>();

builder.Services.AddScoped<IValidator<ReserveBookRequestDto>, ReserveBookRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ReturnBookRequestDto>, ReturnBookRequestDtoValidator>();

var app = builder.Build();

// Seed data at startup using a scoped service
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var seeder = new SeedData(context);
    seeder.SetInitialData();
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
