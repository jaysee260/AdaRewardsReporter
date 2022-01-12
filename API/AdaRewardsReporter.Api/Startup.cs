using System.Text.Json.Serialization;
using AdaRewardsReporter.Core;
using Blockfrost.Api.Extensions;

namespace AdaRewardsReporter.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));;
            // services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddBlockfrost(Configuration["CardanoNetwork"], Configuration["BlockfrostApiKey"]);
            services.AddScoped<IRewardsReporter, RewardsReporter>();
            services.AddScoped<IAddressResolver, AddressResolver>();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to the ADA Rewards Reporter API");
                });
            });
        }
    }
}
