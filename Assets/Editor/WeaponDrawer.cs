using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(BaseWeapon), true)]
public class EffectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var type = GetInstanceTypes().FirstOrDefault(t => property.managedReferenceFullTypename.Contains(t.Name));
        var buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        if (GUI.Button(buttonRect, type != null ? type.Name : "Select Effect Type"))
        {
            ShowTypePicker(property);
        }

        var contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height);

        if (property.hasVisibleChildren)
        {
            EditorGUI.PropertyField(contentRect, property, true);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight + 4;
    }

    private void ShowTypePicker(SerializedProperty property)
    {
        var menu = new GenericMenu();
        foreach (var type in GetInstanceTypes())
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();
    }

    private IEnumerable<Type> GetInstanceTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(BaseWeapon).IsAssignableFrom(t) && !t.IsAbstract);
    }
}
