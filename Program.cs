using FluentValidation;
using Saitynai.Services;

using Saitynai.DTO;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Saitynai.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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


builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddTransient<SessionLoginService>();
builder.Services.AddScoped<AuthSeeder>();

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
    // Raktas kuriuo pasirašinėjame tokenus
    // Jeigu pavogia tavo tokena, galima pakeisti secretą, šiaip rekomenduojame jį keist kartas nuo karto
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]));

});

builder.Services.AddAuthorization();
var app = builder.Build();

// nes user manager ir role manager klasės sukuria kiekvieno userio gyvavimo laikotarpį iš to scope galim pasiimt db context
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();

await dbSeeder.SeedAsync();



app.AddAuthApi();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseCors("Frontend");   // must come BEFORE UseAuthentication / UseAuthorization
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
