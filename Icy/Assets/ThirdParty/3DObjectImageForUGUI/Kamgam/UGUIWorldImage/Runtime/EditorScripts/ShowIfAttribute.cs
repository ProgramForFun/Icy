using System.Collections.Generic;
using System.Reflection;

namespace Kamgam.UGUIWorldImage
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System;


    /// <summary>
    /// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        #region Fields

        public string comparedPropertyName { get; private set; }
        public object comparedValue0 { get; private set; }
        public object comparedValue1 { get; private set; }
        public DisablingType disablingType { get; private set; }
        public bool invertLogic { get; private set; }

        /// <summary>
        /// Types of effects.
        /// </summary>
        public enum DisablingType
        {
            ReadOnly = 2,
            DontDraw = 3
        }

        #endregion

        /// <summary>
        /// Only draws the field only if a condition is met. Supports enum and bool.
        /// </summary>
        /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
        /// <param name="comparedValue0">The value the property is being compared to.</param>
        /// <param name="disablingType">The type of disabling that should happen if the condition is NOT met. Use "Invert" to hide if the condition IS met. Defaulted to DisablingType.DontDraw.</param>
        /// <param name="invertLogic">Invert the result of the comparison?</param>
        public ShowIfAttribute(string comparedPropertyName, object comparedValue0, DisablingType disablingType = DisablingType.DontDraw, bool invertLogic = false, object comparedValue1 = null)
        {
#if UNITY_EDITOR
            this.comparedPropertyName = comparedPropertyName;
            this.comparedValue0 = comparedValue0;
            this.comparedValue1 = comparedValue1;
            this.disablingType = disablingType;
            this.invertLogic = invertLogic;
#endif
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        #region Fields

        // Reference to the attribute on the property.
        ShowIfAttribute drawIf;

        // Field that is being compared.
        SerializedProperty comparedField;

        #endregion

        /// <summary>
        /// Errors default to showing the property.
        /// </summary>
        private bool ShowMe(SerializedProperty property)
        {
            drawIf = attribute as ShowIfAttribute;

            // Replace property name to the value from the parameter
            string basePath = property.propertyPath;
            if (basePath.EndsWith("]")) // avoid failure if the property itself is an array.
            {
                basePath = System.Text.RegularExpressions.Regex.Replace(basePath, "\\.Array\\.data\\[[0-9]+\\]$", "");
            }
            string path = basePath.Contains(".") ? System.IO.Path.ChangeExtension(basePath, drawIf.comparedPropertyName) : drawIf.comparedPropertyName;

            PropertyInfo codeProp = null;
            comparedField = property.serializedObject.FindProperty(path);
            if (comparedField == null)
            {
                // Check if the target object has this property via reflection
                codeProp = property.serializedObject.targetObject.GetType().GetProperty(drawIf.comparedPropertyName);
                if (codeProp == null)
                {
                    Debug.LogError("Cannot find property with name: " + path);
                    return !drawIf.invertLogic;
                }
            }

            bool result;

            // get the value & compare based on types
            if ((comparedField != null && comparedField.type == "bool") || (codeProp != null && codeProp.PropertyType == typeof(bool)))
            {
                result = getBoolResult(property, codeProp, drawIf.comparedValue0);
                if (result == false && drawIf.comparedValue1 != null)
                    result = getBoolResult(property, codeProp, drawIf.comparedValue1);
            }
            else if ((comparedField != null && comparedField.type == "Enum") || (codeProp != null && codeProp.PropertyType == typeof(Enum)))
            {
                result = getEnumResult(property, codeProp, drawIf.comparedValue0);
                if (result == false && drawIf.comparedValue1 != null)
                    result = getEnumResult(property, codeProp, drawIf.comparedValue1);
            }
            else if(comparedField != null)
            {
                // Assume object if compareValue0 is null
                if (drawIf.comparedValue0 == null)
                {
                    result = comparedField.objectReferenceValue == null;
                }
                else
                {
                    Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                    return !drawIf.invertLogic;
                }
            }
            else
            {
                return !drawIf.invertLogic;
            }

            // Debug.Log("Checking " + basePath + " > " + result);
            return drawIf.invertLogic ? !result : result;
        }

        protected bool getBoolResult(SerializedProperty property, PropertyInfo codeProp, object comparedValue)
        {
            bool result;

            if (codeProp == null)
            {
                result = comparedField.boolValue.Equals(comparedValue);
            }
            else
            {
                var value = (bool)codeProp.GetValue(property.serializedObject.targetObject);
                result = (bool)comparedValue == value;
            }

            return result;
        }

        protected bool getEnumResult(SerializedProperty property, PropertyInfo codeProp, object comparedValue)
        {
            bool result;

            if (codeProp == null)
            {
                result = comparedField.enumValueIndex.Equals((int)comparedValue);
            }
            else
            {
                var value = (int)codeProp.GetValue(property.serializedObject.targetObject);
                result = (int)comparedValue == value;
            }

            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShowMe(property) == false && drawIf.disablingType == ShowIfAttribute.DisablingType.DontDraw)
            {
                return 0f;
            }

            // The height of the property should be defaulted to the default height.
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // If the condition is met, simply draw the field.
            if (ShowMe(property))
            {
                EditorGUI.PropertyField(position, property, label, includeChildren: true);
            } //...check if the disabling type is read only. If it is, draw it disabled
            else if (drawIf.disablingType == ShowIfAttribute.DisablingType.ReadOnly)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, includeChildren: true);
                GUI.enabled = true;
            }
        }

    }
#endif
}