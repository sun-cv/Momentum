using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Momentum
{
    [CustomPropertyDrawer(typeof(AbilityPredicate), true)]
    public class AbilityPredicateDrawer : PropertyDrawer
    {
        static Dictionary<string, Type> typeMap;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typeMap == null) BuildTypeMap();

            var typeRect    = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);

            string typeName = property.managedReferenceFullTypename;
            string displayName = GetShortTypeName(typeName) ?? "Select Predicate Type";

            // Dropdown to select type
            if (EditorGUI.DropdownButton(typeRect, new GUIContent(displayName), FocusType.Keyboard))
            {
                var menu = new GenericMenu();

                if (typeMap == null || typeMap.Count == 0)
                {
                    menu.AddDisabledItem(new GUIContent("No Ability Predicates available"));
                }
                else
                {
                    foreach (var kvp in typeMap.OrderBy(k => k.Key))
                    {
                        string path = kvp.Key;
                        Type type = kvp.Value;

                        menu.AddItem(new GUIContent(path), type.FullName == typeName, () =>
                        {
                            Undo.RecordObject(property.serializedObject.targetObject, "Change Ability Predicates");

                            property.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                        });
                    }
                }

                menu.ShowAsContext();
            }

            // Draw inner properties
            if (property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight;
        }

        static void BuildTypeMap()
        {
            var baseType = typeof(AbilityPredicate);

            typeMap = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm =>
                {
                    try { return asm.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
                .ToDictionary(
                    t => GetMenuPath(t),
                    t => t
                );
        }

        static string GetMenuPath(Type type)
        {
            var attr = (RequirementCategoryAttribute)System.Attribute.GetCustomAttribute(type, typeof(RequirementCategoryAttribute));
            string prefix = attr?.Category;
            return string.IsNullOrEmpty(prefix)
                ? ObjectNames.NicifyVariableName(type.Name)
                : prefix + "/" + ObjectNames.NicifyVariableName(type.Name);
        }

        static string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName)) return null;

            // Unity gives "AssemblyName TypeName"
            var spaceIndex = fullTypeName.IndexOf(' ');
            if (spaceIndex < 0) return fullTypeName;

            string classPath = fullTypeName.Substring(spaceIndex + 1);
            return classPath.Split('.').Last();
        }
    }
}
