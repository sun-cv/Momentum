using System;
using UnityEngine;


namespace Momentum.Actor.Sensor
{


    public class ProximitySensor : MonoBehaviour
    {

        [Header("Sensor Configuration")]
        [SerializeField] float radius;
        [SerializeField] float offsetX;
        [SerializeField] float offsetY;

        [Header("Sensor Debug")]
        [SerializeField] bool debug = false;

        [Header("Sensor System")]
        [SerializeField] CircleCollider2D sensor;
        [SerializeField] SpriteRenderer debugSprite;

        public bool PlayerInRange { get; private set; }

        public void Awake()
        {
            sensor.radius = radius;
        }

        public void OnValidate()
        {
            sensor.radius       = radius;
            sensor.offset       = new Vector2(offsetX, offsetY);
            
            if (debugSprite != null)
            {
                debugSprite.enabled = debug;

                debugSprite.transform.localScale     = new Vector3(radius * 2, radius * 2, 1f);
                debugSprite.transform.localPosition  = new Vector3(offsetX, offsetY, 1f);
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerInRange = true;
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerInRange = false;
            }     
        }

    }
}