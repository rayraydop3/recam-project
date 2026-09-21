using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Data;
using RecamNewBackend.Models;
using RecamNewBackend.Middlewares;
using RecamNewBackend.Common.Utilities;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RecamNewBackend.Services;

using Microsoft.OpenApi.Models;
using FluentValidation;
using RecamNewBackend.Filters;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

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
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<RecamDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<RecamDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPhotographyCompanyService, PhotographyCompanyService>();
builder.Services.AddScoped<IListingCaseService, ListingCaseService>();
builder.Services.AddScoped<ICaseContactService, CaseContactService>();
builder.Services.AddScoped<IMediaAssetService, MediaAssetService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<ISelectedMediaService, SelectedMediaService>();
builder.Services.AddScoped<IBlobService, BlobService>();


builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roles = ["Admin", "Agent"];
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();