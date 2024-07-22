using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var condHAtt = (ConditionalHideAttribute)attribute;
        var enabled = GetConditionalHideAttributeResult(condHAtt, property);

        var wasEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var condHAtt = (ConditionalHideAttribute)attribute;
        var enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
    {
        var enabled = true;

        var propertyPath = property.propertyPath; // Returns the property path of the property we want to apply the attribute to
        var conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); // Changes the path to the conditionalsource property path
        var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            enabled = sourcePropertyValue.enumValueIndex == condHAtt.EnumValue;
        }
        else
        {
            Debug.LogWarning("No matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }

        return enabled;
    }
}