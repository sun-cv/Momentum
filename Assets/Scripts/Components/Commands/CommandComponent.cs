using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{


    public class CommandComponent : MonoBehaviour
    {

        [Inject] ICommandSystem system;

        public void Initialize(Context context, InputIntent intent)
        {
            system.Initialize(intent);
        }

        public ICommandSystem System => system;
    }

}