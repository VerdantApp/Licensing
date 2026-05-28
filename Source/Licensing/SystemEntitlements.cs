namespace VerdantApp.Licensing;

using LanguageExt;
using TIKSN.Deployment;
using TIKSN.Globalization;

/// <summary>
/// System Entitlements.
/// </summary>
/// <param name="SystemId">System ID.</param>
/// <param name="EnvironmentName">Deployment environment name.</param>
/// <param name="Countries">Supported Countries.</param>
public record SystemEntitlements(
    Ulid SystemId,
    EnvironmentName EnvironmentName,
    Seq<CountryInfo> Countries);
