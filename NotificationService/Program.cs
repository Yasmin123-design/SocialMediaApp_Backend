
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;
using NotificationService.Services.NotificationService;
using NotificationService.Services.RabbitMqListener;
using LibraryShared.Extensions;
using LibraryShared.Services.UserClientService;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<NotificationContext>(options =>
                     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]);
            });
            builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService.NotificationService>();
            builder.Services.AddHostedService<RabbitMqListenerService>();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddMemoryCache();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");

            app.Run();
        }
    }
}
