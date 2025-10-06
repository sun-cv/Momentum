using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace Momentum
{



    public class AttributeComponent : MonoBehaviour
    {
        [Serialize]
        public List<Attribute> attributes;

        [Inject] IAttributeSystem system;

        public void Initialize()
        {
            attributes.ForEach((attribute) => system.Register(attribute));
        }
        
        public IAttributeSystem System => system;
    }




}