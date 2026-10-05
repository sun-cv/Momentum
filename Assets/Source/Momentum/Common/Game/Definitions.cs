using UnityEngine;



namespace Game.Common
{

    public class Modifier
    {

        public float Positive   { get; private set; }
        public float Negative   { get; private set; }

        public void Fold(float value)
        {
            Positive = Mathf.Max(Positive, value);
            Negative = Mathf.Min(Negative, value);
        }

        public void Reset()
        {
            Positive = 0f;
            Negative = 0f;
        }

        public float Value => (1f + Negative) * (1f + Positive);
    }
}
