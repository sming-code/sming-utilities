using AutoFixture.AutoNSubstitute;

namespace SmingCode.Utilities.Testing.AutoDomainData;

public class TagCustomization : CompositeCustomization
{
    private static readonly List<ICustomization> _defaultCustomizations = [
        new AutoNSubstituteCustomization { ConfigureMembers = true },
        new AutoIncludedSpecimenBuildersCustomization()
    ];

    public TagCustomization()
        : base(_defaultCustomizations) { }

    public TagCustomization(params ICustomization[] additionalCustomizations)
        : base(_defaultCustomizations.Concat(additionalCustomizations)) { }
}