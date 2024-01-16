using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using BAStoryPlayer;

namespace BAStoryPlayer.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(CharacterDataTable))]
    public class CharacterDataTableDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            CharacterDataTable table = target as CharacterDataTable;
            ICharacterDataTableInternal tableInternal = table;
            serializedObject.Update();

            if (GUILayout.Button("Load from SkeletonData folder"))
            {
                EditorUtility.SetDirty(table);
                string selectedFolderPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    tableInternal.Clear();

                    string relativePath = "Assets" + selectedFolderPath.Substring(Application.dataPath.Length);
                    string[] guids = AssetDatabase.FindAssets("",new string[] { relativePath});

                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path.EndsWith(".asset"))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);

                            if(obj != null)
                            {
                                tableInternal.Add(new CharacterDataUnit()
                                {
                                    indexName = obj.name,
                                    skeletonDataUrl = obj.name
                                });
                            }
                        }
                        else if (path.EndsWith(".prefab"))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                            if (obj != null)
                            {
                                tableInternal.Add(new CharacterDataUnit()
                                {
                                    indexName = obj.name,
                                    skeletonDataUrl = obj.name,
                                    loadType = LoadType.Prefab
                                });
                            }
                        }
                    }
                    tableInternal.Reflash();
                    AssetDatabase.SaveAssetIfDirty(target);
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();

                    EditorUtility.ClearDirty(table);
                }
            }

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }


    }
}