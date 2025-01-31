using ChatApp.ApplicationDbContext;
using ChatApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatApp.Interfaces;
using ChatApp.Repositories;
using ChatApp.HubContext;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetSection
("ApplicationDbContext")["ConnStr"] ?? throw new InvalidOperationException("Connection string not found.")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

#region Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
#endregion

//builder.Services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddHttpContextAccessor();


builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddDistributedSqlServerCache(m =>
{
    m.ConnectionString = builder.Configuration.GetSection("ApplicationDbContext")["ConnStr"];
    m.SchemaName = "dbo";
    m.TableName = "SessionData";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapPost("broadcast", async (string message, IHubContext<ChatHub, IChatHub> context) =>
//    {
//        await context.Clients.All.RecieveMessage(message);

//        return Results.NoContent;
//});

app.UseSession();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapHub<ChatHub>("chat-hub");

app.MapControllers();

app.Run();
