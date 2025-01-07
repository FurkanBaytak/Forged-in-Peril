using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BattleAreaManager))]
public class CustomBattleAreaManagerEditor : Editor
{
    SerializedProperty battleAreasProp;
    SerializedProperty selectedBattleAreaIndexProp;
    SerializedProperty mainCameraProp;
    SerializedProperty cameraMoveDurationProp;
    SerializedProperty cameraMoveDelayProp;

    void OnEnable()
    {
        battleAreasProp = serializedObject.FindProperty("battleAreas");
        selectedBattleAreaIndexProp = serializedObject.FindProperty("selectedBattleAreaIndex");
        mainCameraProp = serializedObject.FindProperty("mainCamera");
        cameraMoveDurationProp = serializedObject.FindProperty("cameraMoveDuration");
        cameraMoveDelayProp = serializedObject.FindProperty("cameraMoveDelay");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(selectedBattleAreaIndexProp, new GUIContent("Selected Battle Area Index"));
        EditorGUILayout.PropertyField(mainCameraProp, new GUIContent("Main Camera"));
        EditorGUILayout.PropertyField(cameraMoveDurationProp, new GUIContent("Camera Move Duration"));
        EditorGUILayout.PropertyField(cameraMoveDelayProp, new GUIContent("Camera Move Delay"));
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(battleAreasProp, new GUIContent("Battle Areas"), true);
        EditorGUILayout.Space();

        List<string> battleAreaNames = new List<string>();
        for (int i = 0; i < battleAreasProp.arraySize; i++)
        {
            SerializedProperty battleArea = battleAreasProp.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = battleArea.FindPropertyRelative("battleAreaName");
            string name = "";

            if (nameProp != null && nameProp.propertyType == SerializedPropertyType.String)
            {
                name = nameProp.stringValue;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "Battle Area " + i;
            }

            battleAreaNames.Add(name);
        }

        if (battleAreaNames.Count > 0)
        {
            int currentIndex = selectedBattleAreaIndexProp.intValue;
            if (currentIndex < 0 || currentIndex >= battleAreaNames.Count)
            {
                currentIndex = 0;
                selectedBattleAreaIndexProp.intValue = currentIndex;
            }

            int newIndex = EditorGUILayout.Popup("Select Battle Area", currentIndex, battleAreaNames.ToArray());

            if (newIndex != currentIndex)
            {
                selectedBattleAreaIndexProp.intValue = newIndex;
                if (Application.isPlaying)
                {
                    BattleAreaManager manager = (BattleAreaManager)target;
                    manager.SelectedBattleAreaIndex = newIndex;
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Battle Areas listesi boþ. Lütfen listeye Battle Areas ekleyin.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
