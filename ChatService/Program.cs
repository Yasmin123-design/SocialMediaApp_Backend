
using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services.ChatServ;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;
using LibraryShared.Extensions;

namespace ChatService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ChatDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IChatService, ChatService.Services.ChatServ.ChatService>();
            builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]);
            });
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddMemoryCache();
            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200")   
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();  
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");

            app.Run();
        }
    }
}
