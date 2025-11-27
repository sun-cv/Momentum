using System;
using System.Reflection;



public static class DataMapper
{
    public static void Map<TSource, TTarget>(TSource source, TTarget target)
    {
        var sourceFields = typeof(TSource).GetFields();
        var targetProps  = typeof(TTarget).GetProperties();

        foreach (var field in sourceFields)
        {
            var targetProp = Array.Find(targetProps, p => p.Name == field.Name && p.PropertyType == field.FieldType && p.CanWrite);
            if (targetProp != null)
            {
                targetProp.SetValue(target, field.GetValue(source));
            }
        }
    }
}
