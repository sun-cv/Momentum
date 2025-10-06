using System;


namespace Momentum
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RequirementCategoryAttribute : System.Attribute
    {
        public string Category { get; }
        public RequirementCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}
