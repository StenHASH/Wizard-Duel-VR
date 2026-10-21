using UnityEditor;
using UnityEngine;

public static class AddJoystickAxes
{
    [MenuItem("Tools/Add Joystick Scan Axes")]
    static void Add()
    {
        var asset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/InputManager.asset");
        var so = new SerializedObject(asset);
        var axes = so.FindProperty("m_Axes");

        for (int i = 1; i <= 28; i++)
        {
            string axisName = "JoyAxis" + i;
            bool exists = false;
            for (int j = 0; j < axes.arraySize; j++)
                if (axes.GetArrayElementAtIndex(j).FindPropertyRelative("m_Name").stringValue == axisName)
                    exists = true;
            if (exists) continue;

            axes.arraySize++;
            var p = axes.GetArrayElementAtIndex(axes.arraySize - 1);
            p.FindPropertyRelative("m_Name").stringValue = axisName;
            p.FindPropertyRelative("dead").floatValue = 0.2f;
            p.FindPropertyRelative("sensitivity").floatValue = 1f;
            p.FindPropertyRelative("type").intValue = 2;        // Joystick Axis
            p.FindPropertyRelative("axis").intValue = i - 1;
            p.FindPropertyRelative("joyNum").intValue = 0;      // all joysticks
        }
        so.ApplyModifiedProperties();
        Debug.Log("Joystick scan axes added.");
    }
}