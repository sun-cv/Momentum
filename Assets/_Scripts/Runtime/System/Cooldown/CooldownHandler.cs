using System;
using System.Collections.Generic;

using UnityEngine;

namespace Momentum
{


    public interface ICooldownHandler 
    {
        public bool IsActive<T>() where T : Cooldown; 
        public void Set(Cooldown cooldown);
        public Cooldown Get<T>() where T : Cooldown;
        public Cooldown Cancel<T>() where T : Cooldown;
    }

    public static class CooldownService
    {
        public static void Add(Cooldown cooldown) =>
            Registry.Get<ICooldownHandler>().Set(cooldown);

        public static void Cancel<T>() where T : Cooldown =>
            Registry.Get<ICooldownHandler>().Cancel<T>();

        public static bool IsActive<T>() where T : Cooldown =>
            Registry.Get<ICooldownHandler>().IsActive<T>();

        public static Cooldown Get<T>() where T : Cooldown =>
            Registry.Get<ICooldownHandler>().Get<T>();

    }

    public class CooldownHandler : ICooldownHandler
    {
        private readonly Dictionary<Type, Cooldown> cooldowns   = new();
        private readonly List<Type> expired                     = new();

        public CooldownHandler()
        {
            GameTickBinding.Tick.Add(Tick);
        }

        public void Tick()
        {
            DebugLog();
            RemoveExpiredCooldowns();
        }

        void DebugLog()
        {
            foreach(var type in cooldowns)
            {
                Debug.Log($"in queue: {type.Value.GetType().Name} is active: {type.Value.IsActive} cancelled: {type.Value.cancelled.Value}");
            }
        }
        

        public void Set(Cooldown cooldown)
        {
            if (cooldowns.ContainsKey(cooldown.GetType()))
            {
                Debug.Log($"ITS THIS PIECE OF SHIT {cooldown.GetType().Name}");
                // REWORK REQUIRED - Overwrite mechanic? MAY EAT COOLDOWNS
                return;
            }
            cooldowns.Add(cooldown.GetType(), cooldown);
            cooldown.Start();
        }

        public Cooldown Get<T>() where T : Cooldown
        {
            cooldowns.TryGetValue(typeof(T), out var cooldown);
            return cooldown;
        }

        public Cooldown Cancel<T>() where T : Cooldown
        {
            cooldowns.TryGetValue(typeof(T), out var cooldown);
            if (cooldown is ICancellable)
            {
                Debug.Log($"cancelling: {cooldown.GetType().Name}");
                cooldown.Cancel();
            }
            return cooldown;
        }

        public bool IsActive<T>() where T : Cooldown
        {
            if (!cooldowns.TryGetValue(typeof(T), out var cooldown))
            {
                return false;
            }
            return cooldown.IsActive;
        }


        private void RemoveExpiredCooldowns()
        {   

            foreach (var entry in cooldowns)
            {
                var cooldown = entry.Value;

                if (cooldown.IsFinished)
                {
                    Debug.Log($"Removing: {cooldown.GetType().Name}");
                    expired.Add(entry.Key);
                }

                if (!cooldown.IsActive && cooldown.cancelled)
                {
                    Debug.Log($"Removing: {cooldown.GetType().Name}");
                    expired.Add(entry.Key);
                }
            }

            foreach (var cooldown in expired)
            {
                cooldowns.Remove(cooldown);
            }        

            expired.Clear();
        }
    }






}