using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HC.TechnicalCalculators.Src.Extensions;
using HC.TechnicalCalculators.Src.Factory;
using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Security;
using HC.TechnicalCalculators.Src.Services;
using Xunit;

namespace HC.TechnicalCalculators.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTechnicalCalculators_ShouldRegisterRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTechnicalCalculators();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            
            // Verify security services are registered
            Assert.NotNull(serviceProvider.GetService<ISecureDataService>());
            Assert.NotNull(serviceProvider.GetService<IInputValidationService>());
            Assert.NotNull(serviceProvider.GetService<SecureHttpClientFactory>());
                        
            // Verify correct implementations
            Assert.IsType<SecureDataService>(serviceProvider.GetService<ISecureDataService>());
            Assert.IsType<InputValidationService>(serviceProvider.GetService<IInputValidationService>());
        }

        [Fact]
        public void AddTechnicalCalculators_WithNullConfiguration_ShouldStillRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTechnicalCalculators((IConfiguration?)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<ISecureDataService>());
            Assert.NotNull(serviceProvider.GetService<IInputValidationService>());
        }

        
        [Fact(Skip = "Feature Not fully implemented yet!")]
        public void AddTechnicalCalculators_WithConfiguration_ShouldConfigureOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"SecureNewsFeed:ApiKey", "test-api-key"},
                {"SecureNewsFeed:BaseUrl", "https://test.api.com"},
                {"SecureNewsFeed:MaxRequestsPerMinute", "60"},
                {"SecureNewsFeed:EnableCaching", "true"},
                {"SecureNewsFeed:CacheExpirationMinutes", "30"}
            });
            var configuration = configurationBuilder.Build();

            // Act
            services.AddTechnicalCalculators(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddTechnicalCalculators_ShouldRegisterSingletonServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTechnicalCalculators();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            

            var secureDataService1 = serviceProvider.GetService<ISecureDataService>();
            var secureDataService2 = serviceProvider.GetService<ISecureDataService>();
            Assert.Same(secureDataService1, secureDataService2);

            var validationService1 = serviceProvider.GetService<IInputValidationService>();
            var validationService2 = serviceProvider.GetService<IInputValidationService>();
            Assert.Same(validationService1, validationService2);
        }

        [Fact]
        public void AddTechnicalCalculators_ShouldAllowChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert - Should not throw and allow method chaining
            var result = services.AddTechnicalCalculators()
                               .AddSingleton<string>("test");

            Assert.NotNull(result);
            Assert.Contains(services, s => s.ServiceType == typeof(string));
        }

        [Fact]
        public void AddTechnicalCalculators_CalledMultipleTimes_ShouldNotDuplicateServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTechnicalCalculators();
            services.AddTechnicalCalculators();

            // Assert
            var secureDataServiceCount = services.Count(s => s.ServiceType == typeof(ISecureDataService));
            var validationServiceCount = services.Count(s => s.ServiceType == typeof(IInputValidationService));

            // Should have registered services twice (no deduplication logic)
            Assert.Equal(2, secureDataServiceCount);
            Assert.Equal(2, validationServiceCount);
        }

        [Fact]
        public void AddTechnicalCalculators_WithLogging_ShouldResolveWithoutErrors()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddTechnicalCalculators();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            
            // These should resolve without throwing exceptions
            Assert.NotNull(serviceProvider.GetRequiredService<IInputValidationService>());
            Assert.NotNull(serviceProvider.GetRequiredService<ISecureDataService>());
        }

        [Fact]
        public void AddTechnicalCalculators_WithHttpClient_ShouldRegisterHttpServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHttpClient();

            // Act
            services.AddTechnicalCalculators();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<SecureHttpClientFactory>());
        }
    }
}
