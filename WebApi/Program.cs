using Business.Interfaces;
using Business.Services;
using Data.Contexts;
using Data.Interfaces;
using Data.Repositories;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Set the DataDirectory to root of solution. Used for setting db path relative to solution directory.
//AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")));

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDB")));


builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IWorkoutCustomerRepository, WorkoutCustomerRepository>();
builder.Services.AddScoped<IWorkoutLocationRepository, WorkoutLocationRepository>();
builder.Services.AddScoped<IWorkoutInstructorRepository, WorkoutInstructorRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<Business.Services.AuthService>();

builder.Services.AddCors(p =>
{
    p.AddPolicy("frontend", policy => policy
    .WithOrigins("http://localhost:3000", "http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
