using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[CustomEditor(typeof(MainStart))]
public class MainStartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("ResourceManager Mode");
        EditorGUILayout.BeginVertical(GUI.skin.textArea);
        {
            EditorGUI.BeginChangeCheck();
            bool Deploy_AB = EditorPrefs.GetBool("Deploy_AB", false);
            Deploy_AB = EditorGUILayout.ToggleLeft("Deploy_AB", Deploy_AB);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("Deploy_AB",Deploy_AB);
                if (Deploy_AB)
                {
                    if (EditorPrefs.GetBool("Develop",true))
                    {
                        EditorPrefs.SetBool("Develop",false);
                    }
                }
            }
        }

        {
            EditorGUI.BeginChangeCheck();
            bool Develop = EditorPrefs.GetBool("Develop", true);
            Develop = EditorGUILayout.ToggleLeft("Develop", Develop);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("Develop", Develop);
                if (Develop)
                {
                    if (EditorPrefs.GetBool("Deploy_AB", false))
                    {
                        EditorPrefs.SetBool("Deploy_AB", false);
                    }
                }
            }
        }
    
        EditorGUILayout.EndVertical();
    }
}