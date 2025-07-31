using DG.Tweening;
using UnityEngine;
using Momentum.Definition;
using Momentum.Interface;

namespace Momentum.Cameras
{

    public class CameraOffsetBehavior : MonoBehaviour, ICameraBehavior
    {
        [Header("Offset")]
        [SerializeField] float followSpeedX = 2f;
        [SerializeField] float followSpeedY = 1f;
        [SerializeField] float verticalOffset = 3f;
        [SerializeField] float horizontalOffset = 1.5f;
        [SerializeField] Ease easing = Ease.OutSine;

        CameraContext context;
        Tween offsetTweenX;
        Tween offsetTweenY;

        public void Initialize(CameraContext ctx)
        {
            context = ctx;
        }

    public void Tick()
    {
        if (context == null || context.composer == null || context.hero == null)
            return;

        Vector3 offset = CalculateOffsetFromFacing();

        offsetTweenX?.Kill();
        offsetTweenY?.Kill();

        offsetTweenX = DOTween.To(() => context.composer.TargetOffset.x, x => {
            var current = context.composer.TargetOffset;
            context.composer.TargetOffset = new Vector3(x, current.y, current.z);
        }, offset.x, 1f / followSpeedX).SetEase(easing);

        offsetTweenY = DOTween.To(() => context.composer.TargetOffset.y, y => {
            var current = context.composer.TargetOffset;
            context.composer.TargetOffset = new Vector3(current.x, y, current.z);
        }, offset.y, 1f / followSpeedY).SetEase(easing);
    }

        public void TickLate()
        {
            //noop
        }

        Vector3 CalculateOffsetFromFacing()
        {
            Vector2 dir = context.hero.movement.principalDirection switch
            {
                PrincipalDirection.North     => new Vector2( 0,  1),
                PrincipalDirection.NorthEast => new Vector2(-1,  1),
                PrincipalDirection.East      => new Vector2(-1,  0),
                PrincipalDirection.SouthEast => new Vector2(-1, -1),
                PrincipalDirection.South     => new Vector2( 0, -1),
                PrincipalDirection.SouthWest => new Vector2( 1, -1),
                PrincipalDirection.West      => new Vector2( 1,  0),
                PrincipalDirection.NorthWest => new Vector2( 1,  1),
                _                            => Vector2.zero
            };

            Vector2 normalized = dir.normalized;

            return new Vector3(normalized.x * horizontalOffset, normalized.y * verticalOffset, 0f);
        }

        void OnDisable()
        {
            offsetTweenX?.Kill();
            offsetTweenY?.Kill();        
        }
    }
}

