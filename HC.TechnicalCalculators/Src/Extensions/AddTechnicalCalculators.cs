using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Security;
using HC.TechnicalCalculators.Src.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HC.TechnicalCalculators.Src.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Technical Calculators services with security features to the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">Configuration instance for binding options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTechnicalCalculators(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // Register security services
            services.AddSingleton<ISecureDataService, SecureDataService>();
            services.AddSingleton<IInputValidationService, InputValidationService>();
            services.AddSingleton<SecureHttpClientFactory>();

            // Register the new DI-based calculator factory
            // services.AddSingleton<ITechnicalCalculatorFactory, DependencyInjectionCalculatorFactory>();

            // Configure news feed options with validation
            if (configuration != null)
            {
                services.Configure<SecureNewsFeedOptions>(configuration.GetSection("TechnicalCalculators:NewsFeed"));
            }
            else
            {
                // Default configuration for scenarios where IConfiguration is not available
                services.Configure<SecureNewsFeedOptions>(options =>
                {
                    options.NewsApiEndpoint = "https://api.example.com"; // Should be configured in appsettings
                    options.ApiKey = string.Empty; // Should be configured securely
                    options.CacheTimeMinutes = 15;
                    options.RateLimitPerSecond = 10;
                    options.TimeoutSeconds = 30;
                    options.MaxRetries = 3;
                });
            }

            // Register news feed service (prefer secure implementation)
            services.AddScoped<INewsFeedService, NewsFeedService>();

            // Add HTTP client for news service
            services.AddHttpClient();

            // Add logging if not already registered
            services.AddLogging();

            return services;
        }

        /// <summary>
        /// Adds Technical Calculators services with custom security configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure news feed options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTechnicalCalculators(
            this IServiceCollection services,
            Action<SecureNewsFeedOptions> configureOptions)
        {
            // Register security services
            services.AddSingleton<ISecureDataService, SecureDataService>();
            services.AddSingleton<IInputValidationService, InputValidationService>();
            services.AddSingleton<SecureHttpClientFactory>();

            // Register the new DI-based calculator factory
            //services.AddSingleton<ITechnicalCalculatorFactory, DependencyInjectionCalculatorFactory>();

            // Configure options with provided action
            services.Configure(configureOptions);

            // Register news feed service
            services.AddScoped<INewsFeedService, NewsFeedService>();

            // Add HTTP client for news service
            services.AddHttpClient();

            // Add logging if not already registered
            services.AddLogging();

            return services;
        }

        /// <summary>
        /// Adds Technical Calculators services without news feed functionality (minimal setup)
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTechnicalCalculatorsCore(this IServiceCollection services)
        {
            // Register only core security services
            services.AddSingleton<IInputValidationService, InputValidationService>();

            // Register the new DI-based calculator factory
            //services.AddSingleton<ITechnicalCalculatorFactory, DependencyInjectionCalculatorFactory>();

            // Add logging if not already registered
            services.AddLogging();

            return services;
        }
    }
}
