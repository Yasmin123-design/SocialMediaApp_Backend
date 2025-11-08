
using FeedService.Data;
using FeedService.Services.FeedService;
using FeedService.Services.ImageClientService;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;
using LibraryShared.Extensions;
using LibraryShared.Services.RabbitMqPublisher;

namespace FeedService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<FeedDbContext>(options =>
                     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IFeedService,FeedService.Services.FeedService.FeedService>();
            builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]);
            });
            builder.Services.AddHttpClient<IImageClientService, ImageClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:ImageServiceUrl"]);
            });
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddMemoryCache();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
