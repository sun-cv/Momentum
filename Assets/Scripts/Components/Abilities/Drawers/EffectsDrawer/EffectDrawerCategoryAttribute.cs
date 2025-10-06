using System;


namespace Momentum
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EffectCategoryAttribute : System.Attribute
    {
        public string Category { get; }
        public EffectCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}
