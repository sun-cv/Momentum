using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;



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


public static class EnumUtils
{
    public static IEnumerable<T> GetEnumValues<T>() where T : Enum => (T[])Enum.GetValues(typeof(T));
}

public static class Snapshot
{
    public static IReadOnlyDictionary<TKey, TValue> ReadOnly<TKey, TValue>(Dictionary<TKey, TValue> source)
    {
            return new ReadOnlyDictionary<TKey, TValue>(source);
    }

    public static IReadOnlyDictionary<TKey, IReadOnlyList<TValue>> ReadOnly<TKey, TValue>(Dictionary<TKey, List<TValue>> source)
    {
        return source.ToDictionary( kvp => kvp.Key, kvp => ReadOnly(kvp.Value));
    }

    public static IReadOnlyList<TValue> ReadOnly<TValue>(List<TValue> source)
    {
        return source.AsReadOnly();
    }
}