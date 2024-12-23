using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Text;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomEditor(typeof(UIWidget),true)]
public class UIWidgetEditor : Editor
{
        public override void OnInspectorGUI()
        {
                base.OnInspectorGUI();
                
                UIWidget panel = target as UIWidget;
                SerializedObject so = new SerializedObject(target);

                SerializedProperty so_links = so.FindProperty("Links");
                if (GUILayout.Button("AttachAllWidget(R_*)"))
                {
                        so_links.ClearArray();

                        Transform[] childs = panel.gameObject.GetComponentsInChildren<Transform>(true);
                        for (int i = 0; i < childs.Length; i++)
                        {
                                GameObject obj = childs[i].gameObject;
                                if (obj.name.StartsWith("R_"))
                                {
                                        AttachWidget(obj,panel.transform,so_links);
                                }
                        }
                }
                if (GUILayout.Button("Copy All Widget Name"))
                {
                        StringBuilder content = new StringBuilder();
                        for (int i = 0; i < so_links.arraySize; i++)
                        {
                                var item = so_links.GetArrayElementAtIndex(i);
                                var name = item.FindPropertyRelative("Name");
                                string[] nameSplits = name.stringValue.Split('_');
                                content.AppendLine(string.Format("private {0} {1} = null;", nameSplits[nameSplits.Length - 1], name.stringValue));
                        }
                        content.AppendLine();
                        content.AppendLine("    public override void OnPreLoad()");
                        content.AppendLine("    {");
                        content.AppendLine("        base.OnPreLoad();");
                        for (int i = 0; i < so_links.arraySize; i++)
                        {
                                var item = so_links.GetArrayElementAtIndex(i);
                                var name = item.FindPropertyRelative("Name");
                                string[] nameSplits = name.stringValue.Split('_');

                                content.AppendLine(string.Format("    {0} = GetUIWidget<{1}>(\"{0}\");", name.stringValue, nameSplits[nameSplits.Length - 1]));
                        }
                        content.AppendLine("    }");
                        GUIUtility.systemCopyBuffer = content.ToString();
                }
                
                EditorGUILayout.Space();
                for (int i = 0; i < so_links.arraySize; i++)
                {
                        SerializedProperty item = so_links.GetArrayElementAtIndex(i);
                        if (DrawItem(item))
                        {
                                so_links.DeleteArrayElementAtIndex(i);
                                break;
                        }
                }

                so.ApplyModifiedProperties();

        }

        private void AttachWidget(GameObject go,Transform trans, SerializedProperty links)
        {
                if (go != null)
                {
                        Transform t = go.transform.parent;
                        bool inherited = false;
                        do
                        {
                                if (t == trans)
                                {
                                        inherited = true;
                                        break;
                                }

                                t = t.parent;
                        } while (t != null);

                        if (inherited)
                        {
                                links.InsertArrayElementAtIndex(links.arraySize);
                                SerializedProperty link_item = links.GetArrayElementAtIndex(links.arraySize - 1);

                                SerializedProperty id = link_item.FindPropertyRelative("Name");
                                SerializedProperty widget = link_item.FindPropertyRelative("UIWidget");

                                id.stringValue = go.name;
                                widget.objectReferenceValue = (Object) go;
                                Debug.Log(go.name);
                        }
                }
        }

        private bool DrawItem(SerializedProperty link_item)
        {
                bool to_delete = false;
                EditorGUILayout.BeginHorizontal(GUI.skin.textArea);
                if (GUILayout.Button("X",GUILayout.Width(20)))
                {
                        to_delete = true;
                }

                SerializedProperty id = link_item.FindPropertyRelative("Name");
                SerializedProperty widget = link_item.FindPropertyRelative("UIWidget");
                EditorGUILayout.LabelField(id.stringValue,GUILayout.MinWidth(20));
                EditorGUILayout.ObjectField(widget.objectReferenceValue, typeof(GameObject), true);
                
                
                EditorGUILayout.EndHorizontal();
                return to_delete;
        }
}
