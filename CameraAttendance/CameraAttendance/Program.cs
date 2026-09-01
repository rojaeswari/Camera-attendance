using CameraAttendance.Data;
using CameraAttendance.Interface;
using CameraAttendance.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IAuth, Authservice>();
builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer( builder.Configuration.GetConnectionString("DefaultConnection")));
// FTP Image Service
builder.Services.AddScoped<FtpImageService>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// FTP Image Watcher
builder.Services.AddHostedService<FtpImageWatcher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("ReactPolicy");


//app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapControllers();

//// FTP IMAGE TEST
//using (var scope = app.Services.CreateScope())
//{
//    var ftpService = scope.ServiceProvider
//        .GetRequiredService<FtpImageService>();

//    ftpService.ProcessImages();
//}


app.Run();
