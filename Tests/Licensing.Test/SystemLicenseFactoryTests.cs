namespace VerdantApp.Licensing.Test;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TIKSN.DependencyInjection;
using TIKSN.Licensing;
using Xunit;

public class SystemLicenseFactoryTests
{
    private readonly IServiceProvider serviceProvider;

    public SystemLicenseFactoryTests()
    {
        var services = new ServiceCollection();
        _ = services.AddLicense();
        _ = services.AddFrameworkCore();

        var fakeTimeProvider = new FakeTimeProvider(
            new DateTimeOffset(2022, 9, 24, 0, 0, 0, TimeSpan.Zero));
        _ = services.AddSingleton(fakeTimeProvider);

        ContainerBuilder containerBuilder = new();
        _ = containerBuilder.RegisterModule<CoreModule>();
        containerBuilder.Populate(services);

        this.serviceProvider = new AutofacServiceProvider(containerBuilder.Build());
    }

    [Fact]
    public void GivenRegisteredServices_WhenSystemLicenseFactoryRequested_ThenServiceShouldBeResolved()
    {
        // Arrange

        var systemLicenseFactory = this.serviceProvider
            .GetRequiredService<ILicenseFactory<SystemEntitlements, SystemLicenseEntitlements>>();

        // Act

        // Assert

        _ = systemLicenseFactory.Should().NotBeNull();
    }
}
