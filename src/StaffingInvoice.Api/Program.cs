using Microsoft.EntityFrameworkCore;
using Serilog;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.Middleware;
using StaffingInvoice.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<InvoiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InvoiceDb")));
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPdfInvoiceService, PdfInvoiceService>();
builder.Services.AddCors(options => options.AddPolicy("AngularClient", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
          .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseCors("AngularClient");
app.MapControllers();
app.Run();

public partial class Program { }
