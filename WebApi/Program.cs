using Data.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Set the DataDirectory to root of solution. Used for setting db path relative to solution directory.
AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")));

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDB")));



builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
