using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using EasyMessenger;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder();

// �������� ���� ������ � ��������������
var people = new List<Person>
{
    new Person("tom@gmail.com", "12345"),
    new Person("bob@gmail.com", "55555")
};

// ��������� ������� ��� ������
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();

// ��������� �������������� � �������������� JWT
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
            ClockSkew = TimeSpan.Zero, // ��������� �������� �� �������
        };

        // ��������� ������ �� cookies (���� �����)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // ���������, ���� �� ����� � cookies
                if (context.Request.Cookies.ContainsKey("accessToken"))
                {
                    context.Token = context.Request.Cookies["accessToken"];
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // ��������������� �� �������� ������
                context.Response.Redirect("/account/login");
                context.HandleResponse(); // ������������� ���������� ���������
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ��������� ��������� ������ � ����������
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

// �������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// ������� ��� ������
app.MapPost("/login", (Person loginData) =>
{
    var person = people.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);
    if (person == null) return Results.Unauthorized(); // ������ 401, ���� ������������ �� ������

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
    var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30), // ����� �������� ������
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
    );
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    // ���������� ����� � ��� ������������
    var response = new
    {
        access_token = encodedJwt,
        username = person.Email
    };

    return Results.Json(response);
});

// ������� ��� ������
app.MapPost("/logout", (HttpContext context) =>
{
    // �������� ���� � ������� ��� ������
    context.Response.Cookies.Delete("accessToken");
    return Results.Ok();
});

// ������ ����������
app.Run();
