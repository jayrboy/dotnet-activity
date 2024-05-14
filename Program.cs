using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text.Json.Serialization;
using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

/* ------------------ Database Connect to SQL Server Management Studio ------------------ */
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ActivityContext>(options => options.UseSqlServer(connectionString));

/* --------------------------- Add services to the container. --------------------------- */
//TODO: Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://www.somehost.com")
              .WithHeaders(HeaderNames.ContentType, "application/json")
              .WithMethods("POST", "GET", "PUT", "DELETE");
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
 options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //TODO: Swagger Header
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "My Activity Project API",
        Description = "A simple example ASP.NET Core Web API"
    });

    // using System.Reflection; //TODO: XML Comment
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    //TODO: Add Security Definition - Jwt (1)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer",
    });

    //TODO: Add Security Requirement by OpenApi - Jwt (2)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference
                  {
                      Type = ReferenceType.SecurityScheme,
                      Id = "Bearer"
                  }
              },
              new string[] { }
          }
    });
});

//TODO: Add Authorization -Jwt (3)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
/* --------------------------- Added services to the container. --------------------------- */

/* -------------------------------- Web Server ASP.NET Core -------------------------------- */
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//TODO: Enable CORS middleware
app.UseCors("AngularApp");

app.MapControllers();

app.Run();
/* -------------------------------- Web Server ASP.NET Core -------------------------------- */