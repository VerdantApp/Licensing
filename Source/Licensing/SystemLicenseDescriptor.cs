namespace VerdantApp.Licensing;

using TIKSN.Licensing;

/// <summary>
/// System License Descriptor.
/// </summary>
public class SystemLicenseDescriptor : ILicenseDescriptor<SystemEntitlements>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemLicenseDescriptor"/> class.
    /// </summary>
    public SystemLicenseDescriptor()
    {
        this.Discriminator = Guid.Parse("467b04ea-26a1-4d82-be49-0154744310da");
        this.Name = "Verdant/System";
    }

    /// <summary>
    /// Gets System License Discriminator.
    /// </summary>
    public Guid Discriminator { get; }

    /// <summary>
    /// Gets System License Name.
    /// </summary>
    public string Name { get; }
}
