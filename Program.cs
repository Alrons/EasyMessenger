var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы для сессии
builder.Services.AddDistributedMemoryCache(); // Использование памяти для хранения данных сессии
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии
    options.Cookie.HttpOnly = true; // Сессия доступна только через HTTP
    options.Cookie.IsEssential = true; // Важно для работы с GDPR
});

// Добавляем Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Включаем middleware для сессий
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
