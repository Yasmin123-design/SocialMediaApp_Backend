
using LibraryShared.Services.FeedClientService;
using LibraryShared.Services.UserClientService;
using LikeService.Data;
using LikeService.Services.LikeService;
using Microsoft.EntityFrameworkCore;
using LibraryShared.Extensions;
using LibraryShared.Services.RabbitMqPublisher;

namespace LikeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<LikeContext>(options =>
                     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ILikeService, LikeService.Services.LikeService.LikeService>();
            builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]);
            });
            builder.Services.AddHttpClient<IFeedClientService, FeedClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:FeedServiceUrl"]);
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
