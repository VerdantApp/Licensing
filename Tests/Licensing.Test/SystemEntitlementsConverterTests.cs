namespace VerdantApp.Licensing.Test;

using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using TIKSN.DependencyInjection;
using TIKSN.Deployment;
using TIKSN.Licensing;
using Xunit;
using static LanguageExt.Prelude;

public class SystemEntitlementsConverterTests
{
    private readonly IServiceProvider serviceProvider;

    public SystemEntitlementsConverterTests()
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

    [Theory]
    [InlineData("US", true)]
    [InlineData("en-US", true)]
    [InlineData("001", false)]
    [InlineData("en-001", false)]
    public void GivenCountryCode_WhenSystemEntitlementsConverted_Then(string countryCode, bool isValid)
    {
        // Arrange

        var systemEntitlementsConverter = this.serviceProvider
            .GetRequiredService<IEntitlementsConverter<SystemEntitlements, SystemLicenseEntitlements>>();

        var systemId = Ulid.NewUlid();
        var environmentName = EnvironmentName.Parse("Development", asciiOnly: true, CultureInfo.InvariantCulture).Single();
        var country = new RegionInfo(countryCode);
        var entitlements = new SystemEntitlements(
            systemId, environmentName, Seq1(country));

        // Act

        var validation = systemEntitlementsConverter.Convert(entitlements);

        // Assert

        validation.IsSuccess.ShouldBe(isValid);
        _ = validation.IfSuccess(x =>
        {
            x.CountryCodes.Count.ShouldBe(1);
            x.CountryCodes.Single().ShouldBe(country.TwoLetterISORegionName);
        });
    }

    [Fact]
    public void GivenRegisteredServices_WhenSystemEntitlementsConverterRequested_ThenServiceShouldBeResolved()
    {
        // Arrange

        var systemEntitlementsConverter = this.serviceProvider
            .GetRequiredService<IEntitlementsConverter<SystemEntitlements, SystemLicenseEntitlements>>();

        // Act

        // Assert

        _ = systemEntitlementsConverter.ShouldNotBeNull();
    }
}
