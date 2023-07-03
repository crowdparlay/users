namespace CrowdParlay.Users.IntegrationTests.Attribute;

public class SetupsAttribute : AutoDataAttribute
{
    public SetupsAttribute(params Type[] setupTypes) : base(() => setupTypes
        .Aggregate<Type, IFixture>(new Fixture(), (current, setupType) => current
            .Customize((ICustomization)Activator.CreateInstance(setupType)!))) { }
}