using FluentValidation;
using Saitynai.Services;

using Saitynai.DTO;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<UserResultService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<CreateSessionDto.CreateSessionDtoValidator>();
// builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// builder.Services.AddFluentValidationAutoValidation(configuration =>
// {
//     //configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();

//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
