var builder = WebApplication.CreateBuilder(args);

// ��������� ������� ��� ������
builder.Services.AddDistributedMemoryCache(); // ������������� ������ ��� �������� ������ ������
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ����� ����� ������
    options.Cookie.HttpOnly = true; // ������ �������� ������ ����� HTTP
    options.Cookie.IsEssential = true; // ����� ��� ������ � GDPR
});

// ��������� Razor Pages
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

// �������� middleware ��� ������
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
