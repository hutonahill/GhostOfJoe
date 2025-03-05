using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

using Discord;
using Microsoft.Extensions.Hosting;


using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;


namespace GhostOfJoe {
    class Program {
        
        // eventually these will just be their filenames as the program will have those files in the bin. but Rider is being annoying.
        private static readonly string configPath = "C:/Users/evanriker/Desktop/GhostOfJoe/GhostOfJoe/GhostOfJoe/bin/config.json";

        private static JsonSerializerSettings? JsonSettings;

        public static async Task Main(string[] args) {
            
            JsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented, // For readable output
                TypeNameHandling = TypeNameHandling.None, // Optional: Specify if you need type name handling
                Converters = new List<JsonConverter> { new SettingBaseConverter() } // Register the custom converter
            };

            //config = new DiscordOptions();
            //SaveConfig();
            
            LoadConfig();
            Debug.Assert(Bot.config != null, nameof(Bot.config) + " != null");


            WebApplication app = buildAPI(args);

            await app.RunAsync();

        }
        
        /*public static void ValidateConfiguration(IConfiguration configuration) {
            var requiredSections = new[] { 
            };

            foreach (string section in requiredSections) {
                IConfigurationSection configValue = configuration.GetSection(section);
                if (!configValue.Exists()) {
                    throw new InvalidOperationException($"Configuration section '{section}' is missing.");
                }
            }
            
            Console.WriteLine("appsettings.json appears to be complete.");
        }*/
        
        private static WebApplication buildAPI(string[] args) {
            // this is classified as a minimal API.
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
            // validate that appsettings.json contains the required endpoints
            //ValidateConfiguration(builder.Configuration);
            
            builder.Services.AddHttpContextAccessor();
            
            
            // registers the discord options and will pull values from config files (appsettings.json, etc)
            // The system will try and get a complete set of all values in the class then give
            // the object to services that ask for IOptions<DiscordOptions>
            builder.Services
                .AddOptions<DiscordOptions>()
                .BindConfiguration(DiscordOptions.SectionName);
            
            builder.Services.AddHostedService<Bot>();
            
            builder.Services.AddControllersWithViews();
            
            
            
             // for no roles
            /*
            builder.Services.AddIdentityApiEndpoints<IdentityUser>()
                .AddEntityFrameworkStores<DataContext>();
            */
            
            // This disables the conformed email requirement. We should Re-enable this eventually.
            builder.Services.Configure<IdentityOptions>(options => {
                options.SignIn.RequireConfirmedEmail = false;
            });
            
            // attach our email class.
            //builder.Services.AddTransient<IEmailSender<IdentityUser>, MyEmailService>();
            
            // control how known users interact with the API
            builder.Services.AddAuthorization(options => {
                
                // Define a default policy that requires nothing
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            });
            
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            
            builder.Services.AddHttpContextAccessor();

            
            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });
            
            return builder.Build();
        }
        
        
        private static void LoadConfig() {
            // Check if the config file exists
            if (!File.Exists(configPath)) {
                // If it doesn't exist, create a default config object and save it
                Bot.config = new DiscordOptions(); // Initialize with default values if necessary
                SaveConfig(); // Create the file with default settings
            }
            else {
                // If it exists, read the config file
                string configJson = File.ReadAllText(configPath);

                // Deserialize the JSON into the DiscordOptions object
                Bot.config = JsonConvert.DeserializeObject<DiscordOptions>(configJson, JsonSettings);
            }
        }

        public static void SaveConfig() {
            // Serialize the config object to JSON
            string configJson = JsonConvert.SerializeObject(Bot.config, JsonSettings);

            // Write the JSON to the config file
            File.WriteAllText(configPath, configJson);
        }
    }

}
