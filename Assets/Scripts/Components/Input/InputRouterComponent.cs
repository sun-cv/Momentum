using UnityEngine;


namespace Momentum
{

    
    public class InputRouterComponent : MonoBehaviour
    {

        [Inject] IInputRouter router;

        public void Initialize()
        {
            router.Initialize();
        }

        public IInputRouter Router => router;

        
    }

}