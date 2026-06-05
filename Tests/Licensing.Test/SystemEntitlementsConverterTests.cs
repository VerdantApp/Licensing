namespace VerdantApp.Licensing.Test;

using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using TIKSN.DependencyInjection;
using TIKSN.Deployment;
using TIKSN.Globalization;
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
    [InlineData("US")]
    [InlineData("en-US")]
    public void GivenValidCountryCode_WhenSystemEntitlementsConverted_Then(string countryCode)
    {
        var converter = this.serviceProvider
            .GetRequiredService<IEntitlementsConverter<SystemEntitlements, SystemLicenseEntitlements>>();
        var countryFactory = this.serviceProvider.GetRequiredService<ICountryFactory>();

        var systemId = Ulid.NewUlid();
        var environmentName = EnvironmentName.Parse("Development", asciiOnly: true, CultureInfo.InvariantCulture).Single();
        var country = countryFactory.Create(countryCode);
        var entitlements = new SystemEntitlements(
            systemId, environmentName, Seq1(country));

        var validation = converter.Convert(entitlements);

        validation.IsSuccess.ShouldBeTrue();
        _ = validation.IfSuccess(x =>
        {
            x.CountryCodes.Count.ShouldBe(1);
            x.CountryCodes.Single().ShouldBe(country.PrincipalRegion.TwoLetterISORegionName);
        });
    }

    [Theory]
    [InlineData("001")]
    [InlineData("en-001")]
    public void GivenInvalidCountryCode_WhenConvertingFromDataModel_ThenValidationFails(string countryCode)
    {
        var converter = this.serviceProvider
            .GetRequiredService<IEntitlementsConverter<SystemEntitlements, SystemLicenseEntitlements>>();

        var systemId = Ulid.NewUlid();
        var entitlementsData = new SystemLicenseEntitlements
        {
            SystemId = ByteString.CopyFrom(systemId.ToByteArray()),
            EnvironmentName = "Development",
        };
        entitlementsData.CountryCodes.Add(countryCode);

        var validation = converter.Convert(entitlementsData);

        validation.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void GivenRegisteredServices_WhenSystemEntitlementsConverterRequested_ThenServiceShouldBeResolved()
    {
        var converter = this.serviceProvider
            .GetRequiredService<IEntitlementsConverter<SystemEntitlements, SystemLicenseEntitlements>>();

        _ = converter.ShouldNotBeNull();
    }
}
