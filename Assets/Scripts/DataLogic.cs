

public static class DataMapper
{
    public static void Map<TSource, TTarget>(TSource source, TTarget target)
    {
        var fields = typeof(TSource).GetFields();
        foreach (var field in fields)
        {
            var targetField = typeof(TTarget).GetField(field.Name);
            if (targetField != null && targetField.FieldType == field.FieldType)
            {
                targetField.SetValue(target, field.GetValue(source));
            }
        }
    }
}