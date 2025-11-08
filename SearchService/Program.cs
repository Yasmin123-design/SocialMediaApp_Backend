
using Nest;
using SearchService.Services.RabbitMqListener;
using LibraryShared.Extensions;

namespace SearchService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();

            builder.Services.AddSingleton<IElasticClient>(sp =>
            {
                var settings = new ConnectionSettings(new Uri(builder.Configuration["Elasticsearch:Url"]))
                    .DefaultIndex("posts_index"); 
                return new ElasticClient(settings);
            });
            builder.Services.AddScoped<Services.SearchService.SearchService.ISearchService, Services.SearchServicee.SearchService>();
            builder.Services.AddHostedService<SearchIndexListener>();
            builder.Services.AddJwtAuthentication(builder.Configuration);

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


            app.MapControllers();

            app.Run();
        }
    }
}
