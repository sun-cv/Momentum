
using UnityEngine;
using System;

namespace Momentum
{

        // NOTE - add override in constructor?


    public abstract class Cooldown
    {
        protected HeroContext context;

        public Countdown countdown;
        protected float duration;

        public float Duration  => duration;
        public float Remaining => countdown.CurrentTime;
        public bool IsFinished => countdown.IsFinished;
        public bool  IsActive  => countdown.IsRunning;

        public StatusFlag cancelled     = new();

        public virtual void Start()     { countdown.Start(); }
        public virtual void Cancel()    {}
        public virtual void Interrupt() {}
        public virtual void Override()  {}

        public static T Create<T>() where T : Cooldown
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public static T Create<T>(params object[] args) where T : Cooldown
        {
            return (T)Activator.CreateInstance(typeof(T), args);
        }
    }
    

    public class DashCooldown : Cooldown
    {
        public DashCooldown(HeroContext context)
        {
            this.context    = context;
            countdown       = new Countdown(context.attributes.dash.dashCooldown);
        }
    }


    public class AttackIntervalCooldown : Cooldown
    {
        public AttackIntervalCooldown(HeroContext _context)
        {
            context         = _context;
            countdown       = new Countdown(context.attributes.attack.attackInterval);


            countdown.OnTimerStart+= () => CooldownService.Cancel<AttackComboBreakCooldown>();
            countdown.OnTimerStop += () => CooldownService.Add(Cooldown.Create<AttackComboBreakCooldown>(context));

            if (context.action.basicAttack.attackCount == context.attributes.attack.attackCount)
            {
                countdown.OnTimerStop += () => CooldownService.Add(Cooldown.Create<AttackComboIntervalCooldown>(context));
            }
        }
    }

    public class AttackComboIntervalCooldown : Cooldown
    {
        public AttackComboIntervalCooldown(HeroContext context)
        {
            this.context    = context;
            countdown       = new Countdown(context.attributes.attack.attackComboInterval);

            countdown.OnTimerStop += () => context.action.basicAttack.attackCount = 0;
        }
    }

    public class AttackComboBreakCooldown : Cooldown, ICancellable
    {
        public AttackComboBreakCooldown(HeroContext context)
        {
            this.context    = context;
            countdown       = new Countdown(context.attributes.attack.attackComboBreak);

            countdown.OnTimerStop += AdjustAttackCount;
        }

        void AdjustAttackCount()
        {
            context.action.basicAttack.attackCount = 0;
        }

        public override void Cancel()
        {
            countdown.Cancel();
            cancelled.Set();
        }
    }


}