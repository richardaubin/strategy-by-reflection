using FluentAssertions;

namespace strategy_by_reflection;

public class ReflectionHelperTests
{
    [Fact]
    public void StateStrategy_has_1_or_more_subclasses()
    {
        new StrategyReflectionHelper()
            .GetStrategyTypes().Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void SubClasses_have_StateIdentifier_Identifier_field()
    {
        var helper = new StrategyReflectionHelper();
        var subClasses = helper.GetStrategyTypes();

        foreach(var subClass in subClasses)
        {
            helper.GetSubClassIdentifier(subClass);
        }
    }

    [Fact]
    public void One_strategy_for_every_identifier_that_is_not_None()
    {
        var helper = new StrategyReflectionHelper();
        var subClasses = helper.GetStrategyTypes();

        var enumValues = Enum.GetValues<StateIdentifier>()
            .Where(x => x.Equals(StateIdentifier.None) == false).ToList();

        var subClassIdentifiers = subClasses.Select(x => helper.GetSubClassIdentifier(x)).Distinct().ToList();

        subClassIdentifiers.Count.Should().Be(enumValues.Count);
        foreach(var identifier in subClassIdentifiers)
        {
            enumValues.Contains(identifier).Should().BeTrue();
        }
    }

    [Fact]
    public void CreateInstance_throws_exception_on_Identifier_None()
    {
        var create = () => StateStrategyFactory.CreateInstance(StateIdentifier.None);

        create.Should().Throw<ArgumentException>();
    }
}