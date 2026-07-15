using System;
using System.Collections.Generic;
using System.Linq;



public class AbilityCooldownSystem
{
    readonly List<AbilityCooldownInstance> cooldowns = new();

    public void RegisterAbilities(Ability trigger)
    {
        foreach (var (ability, cooldown) in trigger.Timing.Cooldown)
        {
            RegisterAbility(ability, cooldown);
        }
    }

    public void RegisterAbility(string ability, int cooldown)
    {
        var instance = new AbilityCooldownInstance(ability, cooldown);

        instance.OnClear  += () => cooldowns.Remove(instance);
        instance.OnCancel += () => cooldowns.Remove(instance);

        cooldowns.Add(instance);
        instance.Initialize();
    }

    public bool IsOnCooldown(string name)
    {
         return cooldowns.Any(instance => instance.ability == name);
    }

    public int GetRemainingCooldown(string name)
    {
        return cooldowns.FirstOrDefault(instance => instance.ability == name)?.timer.CurrentFrame ?? 0;
    }
}


public class AbilityCooldownInstance
{
    public string ability;
    public FrameTimer timer;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public AbilityCooldownInstance(string instance, int cooldown)
    {
        ability     = instance;
        timer       = new(cooldown);
    }

    public void Initialize()
    {
        timer.OnTimerStart += OnApply;
        timer.OnTimerStop  += OnClear;
        timer.Start();
    }

    public void Cancel()
    {
        timer.Cancel();
        OnCancel?.Invoke();
    }
}

