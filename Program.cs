using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using EasyMessenger;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder();

// Условная база данных с пользователями
var people = new List<Person>
{
    new Person("tom@gmail.com", "12345"),
    new Person("bob@gmail.com", "55555")
};

// Добавляем сервисы для сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();

// Добавляем аутентификацию с использованием JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // Скидываем задержку по времени
        };

        // Обработка токена из cookies (если нужно)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Проверяем, есть ли токен в cookies
                if (context.Request.Cookies.ContainsKey("accessToken"))
                {
                    context.Token = context.Request.Cookies["accessToken"];
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Перенаправление на страницу логина
                context.Response.Redirect("/account/login");
                context.HandleResponse(); // Останавливаем дальнейшую обработку
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Настройка обработки ошибок в продакшене
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

// Включаем аутентификацию и авторизацию
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Маршрут для логина
app.MapPost("/login", (Person loginData) =>
{
    var person = people.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);
    if (person == null) return Results.Unauthorized(); // Ошибка 401, если пользователь не найден

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
    var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30), // Время действия токена
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
    );
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    // Возвращаем токен и имя пользователя
    var response = new
    {
        access_token = encodedJwt,
        username = person.Email
    };

    return Results.Json(response);
});

// Маршрут для выхода
app.MapPost("/logout", (HttpContext context) =>
{
    // Удаление куки с токеном при выходе
    context.Response.Cookies.Delete("accessToken");
    return Results.Ok();
});

// Запуск приложения
app.Run();
