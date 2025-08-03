using DG.Tweening;
using UnityEngine;


namespace Momentum
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

        private Vector3 currentTargetOffset = Vector3.zero;
        private Vector2 frozenDirection     = Vector2.zero;



        public void Initialize(CameraContext ctx)
        {
            context = ctx;
        }

        public void Tick()
        {
            if (context == null || context.composer == null || context.hero == null)
                return;

            Vector3 offset;

            if (context.hero.state.sprint)
            {
                
                offset = CalculateOffsetFromFacing();
                frozenDirection = offset;
            }
            else
            {
                offset = frozenDirection;
            }
            
            if (offset != currentTargetOffset)
            {
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

                currentTargetOffset = offset;
            }
        }

        public void TickLate()
        {
            //noop
        }

        Vector3 CalculateOffsetFromFacing()
        {
            Vector2 dir = DirectionUtility.GetDirectionVector(context.hero.movement.principal);

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

