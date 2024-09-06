using System.Reflection;

namespace strategy_by_reflection;

public enum StateIdentifier
{
    None,
    StateA,
    StateB
}

public abstract class StateStrategy
{
    public abstract void Execute();
}

public class StateStrategyA : StateStrategy
{
    public static readonly StateIdentifier Identifier = StateIdentifier.StateA;

    public override void Execute()
    {
        Console.WriteLine("Executing Strategy A");
    }
}

public class StateStrategyB : StateStrategy
{
    public static readonly StateIdentifier Identifier = StateIdentifier.StateB;

    public override void Execute()
    {
        Console.WriteLine("Executing Strategy B");
    }
}

public static class StateStrategyFactory
{
    public static StateStrategy CreateInstance(StateIdentifier identifier)
    {
        if(identifier.Equals(StateIdentifier.None))
        {
            throw new ArgumentException("Use an identifier other than None");
        }

        var helper = new StrategyReflectionHelper();
        var strategyTypes = helper.GetStrategyTypes();
        var strategies = new Dictionary<StateIdentifier, Type>();

        foreach(var strategyType in strategyTypes)
        {
            var strategyIdentifier = helper.GetSubClassIdentifier(strategyType);

            if(strategies.ContainsKey(strategyIdentifier))
            {
                throw new ArgumentException($"{Enum.GetName(strategyIdentifier)} identifier found in {strategyType.Name} already exists in {strategies[identifier].Name}");
            }

            strategies[strategyIdentifier] = strategyType;
        }

        return (Activator.CreateInstance(strategies[identifier]) as StateStrategy)!;

        throw new ArgumentException($"No StateStrategy found for identifier '{identifier}'");
    }
}

public class StrategyReflectionHelper
{
    public IReadOnlyList<Type> GetStrategyTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(StateStrategy)))
            .ToList();
    }

    public StateIdentifier GetSubClassIdentifier(Type subClass)
    {
        var field = GetIdentifierField(subClass);
        var value = field.GetValue(null);

        if(value is null)
        {
            throw new NullReferenceException($"{subClass.Name} Identifier value cannot be null");
        }

        if(value.GetType() != typeof(StateIdentifier))
        {
            throw new ArgumentException($"{subClass.Name} Identifier should be {nameof(StateIdentifier)} enum type");
        }

        var identifier = (StateIdentifier)value;

        if(identifier == StateIdentifier.None)
        {
            throw new ArgumentException($"{subClass.Name} Identifier must use a value other than {nameof(StateIdentifier.None)}");
        }

        return (StateIdentifier)value!;
    }

    private FieldInfo GetIdentifierField(Type subClass)
    {
        var field = subClass.GetField("Identifier", BindingFlags.Public | BindingFlags.Static);

        if(field is null)
        {
            throw new InvalidOperationException($"{subClass.Name} does not have an Identifier field");
        }

        return field;
    }
}