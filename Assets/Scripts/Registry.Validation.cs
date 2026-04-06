


public static class Validate
{
    public static bool Definition(string name)
    {
        if (Definitions.Exists(name)) 
            return true;

        Log.Alert($"Definition not found: '{name}'");
        return false;
    }

    public static bool Definition<TDefinition>()
    {
        if (Definitions.TryGet<TDefinition>(out _)) 
            return true;

        Log.Alert($"Definition not found: '{typeof(TDefinition).Name}'");
        return false;
    }

    public static bool Prefab(string name)
    {
        if (Assets.Exists(name)) 
            return true;

        Log.Alert($"Prefab not found: '{name}'");
        return false;
    }

    public static bool Asset(string name)
    {
        if (Definition(name) && Prefab(name)) 
            return true;

        Log.Alert($"Cannot spawn '{name}': missing definition or prefab.");
        return false;
    }

    static readonly Logger Log = Logging.For(LogSystem.Validation);
}
