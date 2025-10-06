using UnityEngine;


namespace Momentum
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementEngineComponent : MonoBehaviour
    {

        [Inject]
        IMovementEngine engine;

        [SerializeField] 
        MovementEngineConfig config;


        public void Initialize(Context context, MovementIntent intent)
        {
            engine.BindConfig(config);
            engine.BindBody(context.entity.body.rigidBody);
            engine.BindIntent(intent);
            engine.BindContext(context);
            
            engine.Initialize();
        }

        public IMovementEngine Engine => engine;
    }

}