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


builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDB")));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IWorkoutCustomerRepository, WorkoutCustomerRepository>();
builder.Services.AddScoped<IWorkoutLocationRepository, WorkoutLocationRepository>();
builder.Services.AddScoped<IWorkoutInstructorRepository, WorkoutInstructorRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();

builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})

.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();


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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Business.Data.DbInitializer.SeedRolesAsync(services);
}

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
