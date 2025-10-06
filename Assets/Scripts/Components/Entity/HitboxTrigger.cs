using System;
using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{
    [RequireComponent(typeof(Collider2D))]
    public class HitboxTrigger : MonoBehaviour
    {
        public Action<Collider2D> onHit;
        private readonly HashSet<Collider2D> alreadyHit = new();

        private void OnTriggerEnter2D(Collider2D target)
        {
            if (alreadyHit.Contains(target)) return;
            alreadyHit.Add(target);
            onHit?.Invoke(target);
        }
        public void ResetHits()
        {
            alreadyHit.Clear();
        }
    }
}
