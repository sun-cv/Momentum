using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

namespace Momentum
{

    public class DeadzoneCameraBehavior : MonoBehaviour, ICameraBehavior
    {
        private enum DeadzoneState
        {
            Open,
            Opening,
            Closed,
            Shrinking
        }

        [Header("Deadzone Timing")]
        [SerializeField] float idleTimeThreshold    = 0.5f;
        [SerializeField] float deadzoneTimeThreshold= 1f;
        [SerializeField] float openSpeed            = 2f;
        [SerializeField] float shrinkSpeed          = 1f;
        [SerializeField] float sizeThreshold        = 0.2f;

        [Header("Deadzone Easing")]
        [SerializeField] Ease openEase              = Ease.Linear;
        [SerializeField] Ease shrinkEase            = Ease.Linear;

        [Header("Deadzone Sizes")]
        [SerializeField] Vector2 idleSize           = new(0.5f, 0.5f);
        [SerializeField] Vector2 movingSize         = new(0f, 0f);

        private CameraContext context;
        private CinemachinePositionComposer composer;

        private Tween sizeTween;
        private DeadzoneState state = DeadzoneState.Closed;

        private Stopwatch timer;
        private Stopwatch idleTimer;


        public void Initialize(CameraContext context)
        {
            this.context = context;

            timer = new Stopwatch();
            timer.Start();

            idleTimer = context.hero.movement.IdleTimer;

            composer  = context.composer;
            composer.Composition.DeadZone.Enabled   = true;
            composer.Composition.DeadZone.Size      = movingSize;

            state = DeadzoneState.Closed;
        }

        public void Tick()
        {
            Vector2 currentSize = composer.Composition.DeadZone.Size;

            switch (state)
            {
                case DeadzoneState.Closed:
                    if (idleTimer.CurrentTime > idleTimeThreshold && timer.CurrentTime > deadzoneTimeThreshold)
                    {
                        StartSizeTween(idleSize, openSpeed, openEase);
                        state = DeadzoneState.Opening;
                    }
                    break;

                case DeadzoneState.Opening:
                    if (Vector2.Distance(currentSize, idleSize) <= sizeThreshold)
                    {
                        state = DeadzoneState.Open;
                        
                        timer = new Stopwatch(); 
                        timer.Start();
                    }
                    break;

                case DeadzoneState.Open:
                    if (IsPlayerOutsideDeadzone())
                    {
                        StartSizeTween(movingSize, shrinkSpeed, shrinkEase);
                        state = DeadzoneState.Shrinking;
                    }
                    break;

                case DeadzoneState.Shrinking:
                    if (Vector2.Distance(currentSize, movingSize) <= sizeThreshold)
                    {
                        state = DeadzoneState.Closed;

                        timer = new Stopwatch(); 
                        timer.Start();
                    }
                    break;
            }
        }

        public void TickLate()
        {
            // Noop
        }

        private bool IsPlayerOutsideDeadzone()
        {
            var (rect, _) = GetDeadzoneWorldBounds();

            Vector3 offsetTarget = context.cameraTarget.position + composer.TargetOffset;
            Vector2 point        = offsetTarget;

            return !rect.Contains(point);
        }

        private (Rect, Vector3) GetDeadzoneWorldBounds()
        {
            float camHeight     = context.camera.orthographicSize * 2f;
            float camWidth      = camHeight * context.camera.aspect;

            Vector2 screenSize  = new(camWidth, camHeight);
            Vector2 zoneSize    = Vector2.Scale(composer.Composition.DeadZone.Size, screenSize);

            Vector3 center      = context.camera.transform.position;
            Vector2 half        = zoneSize / 2f;
    
            Vector3 min         = center - new Vector3(half.x, half.y, 0f);
            return (new Rect(min, zoneSize), center);
        }

        private void StartSizeTween(Vector2 toSize, float speed, Ease ease)
        {
            sizeTween?.Kill();

            Vector2 currentSize = composer.Composition.DeadZone.Size;
            float distance      = Vector2.Distance(currentSize, toSize);
            float duration      = distance / Mathf.Max(speed, 0.01f);

            sizeTween = DOTween.To(
                () => composer.Composition.DeadZone.Size,
                x => composer.Composition.DeadZone.Size = x,
                toSize,
                duration
            ).SetEase(ease);
        }

        void OnDisable()
        {
            sizeTween?.Kill();
        }
    }
}


