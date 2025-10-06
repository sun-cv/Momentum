using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Momentum 
{
    public class DelayActivationActivity : Activity 
    {
        public float seconds = 5f;

        public override async Task ActivateAsync(CancellationToken ct) 
        {
            Debug.Log($"Activating {GetType().Name} (mode={this.Mode}) after {seconds} seconds");
            await Task.Delay(TimeSpan.FromSeconds(seconds), ct);
            await base.ActivateAsync(ct);
        }
    }
}