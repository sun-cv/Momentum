using System;


namespace Momentum
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CooldownCategoryAttribute : System.Attribute
    {
        public string Category { get; }
        public CooldownCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}
