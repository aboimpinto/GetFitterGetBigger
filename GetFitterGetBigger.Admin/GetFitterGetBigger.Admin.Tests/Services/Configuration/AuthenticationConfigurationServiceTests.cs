using FluentAssertions;
using GetFitterGetBigger.Admin.Services.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services.Configuration
{
    public class AuthenticationConfigurationServiceTests
    {
        private readonly AuthenticationConfigurationService _service;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockGoogleSection;
        private readonly Mock<IConfigurationSection> _mockFacebookSection;
        private readonly ServiceCollection _services;
        private readonly AuthenticationBuilder _authBuilder;

        public AuthenticationConfigurationServiceTests()
        {
            _service = new AuthenticationConfigurationService();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockGoogleSection = new Mock<IConfigurationSection>();
            _mockFacebookSection = new Mock<IConfigurationSection>();
            
            _services = new ServiceCollection();
            _authBuilder = new AuthenticationBuilder(_services);
            
            SetupConfiguration();
        }

        private void SetupConfiguration()
        {
            _mockGoogleSection.Setup(x => x["ClientId"]).Returns("test-google-client-id");
            _mockGoogleSection.Setup(x => x["ClientSecret"]).Returns("test-google-client-secret");
            
            _mockFacebookSection.Setup(x => x["AppId"]).Returns("test-facebook-app-id");
            _mockFacebookSection.Setup(x => x["AppSecret"]).Returns("test-facebook-app-secret");
            
            _mockConfiguration.Setup(x => x.GetSection("Authentication:Google")).Returns(_mockGoogleSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("Authentication:Facebook")).Returns(_mockFacebookSection.Object);
            _mockConfiguration.Setup(x => x["Authentication:Google:ClientId"]).Returns("test-google-client-id");
            _mockConfiguration.Setup(x => x["Authentication:Google:ClientSecret"]).Returns("test-google-client-secret");
            _mockConfiguration.Setup(x => x["Authentication:Facebook:AppId"]).Returns("test-facebook-app-id");
            _mockConfiguration.Setup(x => x["Authentication:Facebook:AppSecret"]).Returns("test-facebook-app-secret");
        }

        [Fact]
        public void ConfigureAuthenticationOptions_ShouldAddCookieAuthentication()
        {
            // Act
            _service.ConfigureAuthenticationOptions(_authBuilder, _mockConfiguration.Object);

            // Assert
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<CookieAuthenticationOptions>));
        }

        [Fact]
        public void ConfigureAuthenticationOptions_ShouldAddGoogleAuthentication()
        {
            // Act
            _service.ConfigureAuthenticationOptions(_authBuilder, _mockConfiguration.Object);

            // Assert
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<GoogleOptions>));
        }

        [Fact]
        public void ConfigureAuthenticationOptions_ShouldAddFacebookAuthentication()
        {
            // Act
            _service.ConfigureAuthenticationOptions(_authBuilder, _mockConfiguration.Object);

            // Assert
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<FacebookOptions>));
        }

        [Fact]
        public void ConfigureAuthenticationOptions_WithNullConfiguration_ShouldUseDefaultValues()
        {
            // Arrange
            var emptyConfig = new Mock<IConfiguration>();
            emptyConfig.Setup(x => x["Authentication:Google:ClientId"]).Returns((string?)null);
            emptyConfig.Setup(x => x["Authentication:Google:ClientSecret"]).Returns((string?)null);
            emptyConfig.Setup(x => x["Authentication:Facebook:AppId"]).Returns((string?)null);
            emptyConfig.Setup(x => x["Authentication:Facebook:AppSecret"]).Returns((string?)null);

            // Act
            _service.ConfigureAuthenticationOptions(_authBuilder, emptyConfig.Object);

            // Assert
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<GoogleOptions>));
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<FacebookOptions>));
        }

        [Fact]
        public void ConfigureAuthenticationOptions_ShouldConfigureAllThreeProviders()
        {
            // Act
            _service.ConfigureAuthenticationOptions(_authBuilder, _mockConfiguration.Object);

            // Assert
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<CookieAuthenticationOptions>));
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<GoogleOptions>));
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<FacebookOptions>));
        }

        [Fact]
        public void Service_ShouldImplementInterface()
        {
            // Assert
            _service.Should().BeAssignableTo<IAuthenticationConfigurationService>();
        }

        [Fact]
        public void ConfigureAuthenticationOptions_WithValidConfiguration_ShouldExecuteSuccessfully()
        {
            // Act & Assert
            var action = () => _service.ConfigureAuthenticationOptions(_authBuilder, _mockConfiguration.Object);
            action.Should().NotThrow();
            
            // Verify all three authentication providers are configured
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<CookieAuthenticationOptions>));
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<GoogleOptions>));
            _services.Should().Contain(sd => sd.ServiceType == typeof(IConfigureOptions<FacebookOptions>));
        }

        [Fact]
        public void ConfigureAuthenticationOptions_WithEmptyConfiguration_ShouldStillWork()
        {
            // Arrange
            var emptyConfig = new Mock<IConfiguration>();
            
            // Act & Assert
            var action = () => _service.ConfigureAuthenticationOptions(_authBuilder, emptyConfig.Object);
            action.Should().NotThrow();
        }

        [Fact]
        public void ConfigureAuthenticationOptions_ShouldNotThrowException()
        {
            // Act & Assert
            var action = () => _service.ConfigureAuthenticationOptions(_authBuilder, _mockConfiguration.Object);
            action.Should().NotThrow();
        }
    }
}