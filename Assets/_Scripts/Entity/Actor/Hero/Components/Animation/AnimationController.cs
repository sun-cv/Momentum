

using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{

    public interface IAnimationController {};


    public class AnimationController : IAnimationController
    {   

        private Animator animator;
        private HeroContext context;

        private Dictionary<string, float> clipDurations;

        public void Initialize(Hero hero)
        {
            animator    = hero.animatorSystem;
            context     = hero.context;

            GenerateClipDurations();
        }

        public void Play(AnimatorRequest animationRequest)
        {
            animator.CrossFade(animationRequest.hash, animationRequest.crossfade, animationRequest.layer);
        }

        public void Play(AnimatorRequest animationRequest, out float duration)
        {

            animator.CrossFade(animationRequest.hash, animationRequest.crossfade, animationRequest.layer);
            duration = clipDurations[animationRequest.name];
        }

        public float Duration(AnimatorRequest animationRequest)
        {
            return clipDurations[animationRequest.name];
        }


        public void Tick()
        {
            SetAnimatorValues();
        }

        public void TickFixed()
        {
            // noop
        }

        private void SetAnimatorValues()
        {
            animator.SetFloat("MoveX", context.movement.cardinalDirection.x);
            animator.SetFloat("MoveY", context.movement.cardinalDirection.y);
        }

        private float GetAnimationClipLength(string clipName)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                Debug.Log($"{clip.name}");
                if (clip.name == clipName)
                    return clip.length;
            }
            return 0f;
        }

        private void GenerateClipDurations()
        {
            clipDurations = new();

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                var key = clip.name.Split('_')[0];

                if (!clipDurations.ContainsKey(key))
                {
                    clipDurations[key] = clip.length;
                }
            }
        }

    }








}



