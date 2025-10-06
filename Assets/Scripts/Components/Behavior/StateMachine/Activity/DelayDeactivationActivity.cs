using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Momentum 
{
    public class DelayDeactivationActivity : Activity 
    {
        public float seconds = 5f;

        public override async Task DeactivateAsync(CancellationToken ct) 
        {
            Debug.Log($"Deactivating {GetType().Name} (mode={this.Mode}) after {seconds} seconds");
            await Task.Delay(TimeSpan.FromSeconds(seconds), ct);
            await base.DeactivateAsync(ct);
        }
    }
}