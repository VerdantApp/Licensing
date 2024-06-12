namespace VerdantApp.Licensing;

using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using TIKSN.Deployment;
using TIKSN.Globalization;
using TIKSN.Licensing;
using static LanguageExt.Prelude;

/// <summary>
/// System Entitlements Converter.
/// </summary>
public class SystemEntitlementsConverter : IEntitlementsConverter<SystemEntitlements, SystemLicenseEntitlements>
{
    private static readonly Seq<Ulid> InvalidSystemIds =
        Seq(Ulid.Empty, Ulid.MinValue, Ulid.MaxValue);

    private readonly IRegionFactory regionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemEntitlementsConverter"/> class.
    /// </summary>
    /// <param name="regionFactory"><see cref="RegionInfo"/> Factory.</param>
    [CLSCompliant(false)]
    public SystemEntitlementsConverter(
        IRegionFactory regionFactory)
        => this.regionFactory = regionFactory ?? throw new ArgumentNullException(nameof(regionFactory));

    /// <summary>
    /// Convert from Domain Model to Data Model.
    /// </summary>
    /// <param name="entitlements">Domain Model.</param>
    /// <returns>Validation of Data Model.</returns>
    [CLSCompliant(false)]
    public Validation<Error, SystemLicenseEntitlements> Convert(
        SystemEntitlements entitlements)
    {
        var errors = new List<Error>();
        var result = new SystemLicenseEntitlements();

        if (entitlements == null)
        {
            errors.Add(Error.New(1272977306, "Value must not be NULL"));
            return errors.ToSeq();
        }

        if (InvalidSystemIds.Contains(entitlements.SystemId))
        {
            errors.Add(Error.New(1366359375, "Invalid System ID"));
        }

        result.SystemId = new ArraySegment<byte>(entitlements.SystemId.ToByteArray());

        if (string.IsNullOrWhiteSpace(entitlements.EnvironmentName.ToString()))
        {
            errors.Add(Error.New(1410594292, "Environment Name is missing"));
        }
        else
        {
            result.EnvironmentName = entitlements.EnvironmentName.ToString();
        }

        if (entitlements.Countries.Count == 0)
        {
            errors.Add(Error.New(1863106995, "Countries most not be empty"));
        }
        else
        {
            entitlements.Countries.ForEach(x => this.ValidateCountryCode(x, errors.Add));

            result.CountryCodes = [.. entitlements.Countries.Select(x => x.TwoLetterISORegionName)];
        }

        if (errors.Count > 0)
        {
            return errors.ToSeq();
        }

        return result;
    }

    /// <summary>
    /// Convert from Data Model to Domain Model.
    /// </summary>
    /// <param name="entitlementsData">Data Model.</param>
    /// <returns>Validation of Domain Model.</returns>
    [CLSCompliant(false)]
    public Validation<Error, SystemEntitlements> Convert(
        SystemLicenseEntitlements entitlementsData)
    {
        var errors = new List<Error>();

        if (entitlementsData is null)
        {
            errors.Add(Error.New(253116444, "Value must not be NULL"));

            return errors.ToSeq();
        }

        var systemId = new Ulid(entitlementsData.SystemId);

        if (InvalidSystemIds.Contains(systemId))
        {
            errors.Add(Error.New(670425534, "Invalid System ID"));
        }

        if (string.IsNullOrWhiteSpace(entitlementsData.EnvironmentName))
        {
            errors.Add(Error.New(429642749, "Environment Name is missing"));
        }

        var environmentName =
            EnvironmentName.Parse(entitlementsData.EnvironmentName, asciiOnly: true, CultureInfo.InvariantCulture);

        if (environmentName.IsNone)
        {
            errors.Add(Error.New(1804391968, "Environment Name is invalid"));
        }

        var environmentNameValue = environmentName.Match(s => s, () => throw new InvalidOperationException());

        if (entitlementsData.CountryCodes is null || entitlementsData.CountryCodes.Count == 0)
        {
            errors.Add(Error.New(1752177083, "Country Codes is missing"));
        }
        else
        {
            entitlementsData.CountryCodes.ForEach(x => this.ValidateCountryCode(x, errors.Add));
        }

        var countries = (entitlementsData.CountryCodes ?? [])
            .Choose(x => this.CreateRegion(x, errors.Add))
            .ToSeq();

        if (errors.Count > 0)
        {
            return errors.ToSeq();
        }

        return new SystemEntitlements(
            systemId,
            environmentNameValue,
            countries);
    }

    private Option<RegionInfo> CreateRegion(string? name, Action<Error> addError)
    {
        this.ValidateCountryCode(name, addError);

        if (name is not null)
        {
            try
            {
                return this.regionFactory.Create(name);
            }
            catch (ArgumentException)
            {
                addError(Error.New(9414785, "Country Code is unknown"));
                return None;
            }
        }

        return None;
    }

    private void ValidateCountryCode(RegionInfo? country, Action<Error> addError)
        => this.ValidateCountryCode(country?.TwoLetterISORegionName, addError);

    private void ValidateCountryCode(string? code, Action<Error> addError)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            addError(Error.New(253163448, "Country Code is missing"));
            return;
        }

        if (code.Length < 2)
        {
            addError(Error.New(1280422427, "Country Code is too short"));
            return;
        }

        if (code.Length > 2)
        {
            addError(Error.New(1165853056, "Country Code is too long"));
            return;
        }

        if (!code.All(char.IsAsciiLetterUpper))
        {
            addError(Error.New(1638279115, "Country Code must contain only upper case ASCII letters"));
            return;
        }

        try
        {
            _ = this.regionFactory.Create(code);
        }
        catch (ArgumentException)
        {
            addError(Error.New(111865750, "Country Code is invalid"));
        }
    }
}
