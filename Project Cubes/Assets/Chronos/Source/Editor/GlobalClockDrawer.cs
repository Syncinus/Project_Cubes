using Chronos;
using Chronos.Controls;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GlobalClockAttribute))]
public class GlobalClockDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		Timekeeper timekeeper = Timekeeper.instance;

		List<PopupOption<string>> options = new List<PopupOption<string>>();

		foreach (GlobalClock globalClock in timekeeper
			.GetComponents<GlobalClock>()
			.Where(gc => !string.IsNullOrEmpty(gc.key)))
		{
			options.Add(new PopupOption<string>(globalClock.key));
		}

		PopupOption<string> selectedOption;

		if (options.Any(o => o.value == property.stringValue))
		{
			selectedOption = new PopupOption<string>(property.stringValue);
		}
		else if (!string.IsNullOrEmpty(property.stringValue))
		{
			selectedOption = new PopupOption<string>(property.stringValue, property.stringValue + " (Missing)");
		}
		else
		{
			selectedOption = null;
		}

		PopupOption<string> noneOption = new PopupOption<string>(null, "None");

		var currentProperty = property;

		position = EditorGUI.PrefixLabel(position, label);

		PopupGUI<string>.Render
		(
			position,
			gc => ChangeValue(currentProperty, gc),
			options,
			selectedOption,
			noneOption,
			property.hasMultipleDifferentValues
		);

		EditorGUI.EndProperty();
	}

	protected void ChangeValue(SerializedProperty property, string value)
	{
		// BUG: Multi-object editing and resetting the same property doesn't apply
		// That's probably because the "Modified" flag isn't triggered, even if one of the
		// objects has a different value.

		property.stringValue = value;
		property.serializedObject.ApplyModifiedProperties();
	}
}
