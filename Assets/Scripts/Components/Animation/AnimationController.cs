

using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{

    public interface IAnimationController
    {

        void Initialize(Hero hero);
        void Tick();
        void TickFixed();
        void Play(AnimatorRequest animationRequest);
        void Play(AnimatorRequest animationRequest, out float duration);
        
        void CrossFade(AnimatorRequest animationRequest);
        void Crossfade(AnimatorRequest animationRequest, out float duration);
        float Duration(AnimatorRequest animationRequest);
    }

    public class AnimationController : IAnimationController
    {

        private Animator animator;
        // private Context context;

        private Dictionary<string, float> clipDurations;

        public void Initialize(Hero hero)
        {
            // animator    = hero.animatorSystem;
            // context     = hero.context;

            GenerateClipDurations();
        }

        public void Play(AnimatorRequest animationRequest)
        {
            animator.Play(animationRequest.hash, animationRequest.layer, 0f);
        }

        public void Play(AnimatorRequest animationRequest, out float duration)
        {

            animator.CrossFade(animationRequest.hash, animationRequest.crossfade, animationRequest.layer);
            duration = clipDurations[animationRequest.name];
        }

        public void CrossFade(AnimatorRequest animationRequest)
        {
            animator.CrossFade(animationRequest.hash, animationRequest.crossfade, animationRequest.layer);
        }

        public void Crossfade(AnimatorRequest animationRequest, out float duration)
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
            // animator.SetFloat("MoveX", context.movement.cardinalDirection.x);
            // animator.SetFloat("MoveY", context.movement.cardinalDirection.y);
        }

        private float GetAnimationClipLength(string clipName)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
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



