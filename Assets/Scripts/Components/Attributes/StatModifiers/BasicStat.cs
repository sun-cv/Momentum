using System;
using UnityEngine;

namespace Momentum
{

    public class BasicStatModifier : StatModifier
    {
        readonly string stat;
        readonly Func<float, float> operation;


        public BasicStatModifier(string stat, float duration, Func<float, float> operation) : base (duration)
        {
            this.stat      = stat;
            this.operation = operation;
        }

        public override void Handle(object sender, Query query)
        {
            if (query.stat == stat)
                return;

            query.value = operation(query.value);
        }

    }

}