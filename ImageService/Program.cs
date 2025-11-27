
using ImageService.Data;
using ImageService.Services.ImageService;
using LibraryShared.Services.FeedClientService;
using LibraryShared.Services.UserClientService;
using LibraryShared.Extensions;
using Microsoft.EntityFrameworkCore;
using LibraryShared.Services.RabbitMqPublisher;


namespace ImageService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ImageDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IImageService,ImageService.Services.ImageService.ImageService>();
            builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]);
            });
            builder.Services.AddHttpClient<IFeedClientService, FeedClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:FeedServiceUrl"]); 
            });

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            builder.Services.AddMemoryCache();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
