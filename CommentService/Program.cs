
using CommentService.Data;
using CommentService.Services.CommentService;
using LibraryShared.Services.FeedClientService;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;
using LibraryShared.Extensions;
using LibraryShared.Services.RabbitMqPublisher;

namespace CommentService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<CommentContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICommentService, CommentService.Services.CommentService.CommentService>();
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

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
