using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using BAStoryPlayer;
using Spine.Unity.AttachmentTools;
using Spine;
using BAStoryPlayer.DoTweenS;
using UnityEditor;
using System;
using BAStoryPlayer.AsScriptParser;
using System.Linq;
using System.IO;
using BAStoryPlayer.Event;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public string storyScriptName = "TestScript";
    Action test;
    public SkeletonGraphic skelg;
    List<GameObject> ps = new List<GameObject>();

    private void OnGUI()
    {
        storyScriptName = GUILayout.TextField(storyScriptName,15);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Play"))
        {
            if(Application.isPlaying)
                BAStoryPlayerController.Instance.LoadStoryTest(storyScriptName);
        }
        GUILayout.Label("！！！！！！！！");
        if (GUILayout.Button("demo1"))
            if (Application.isPlaying)
                BAStoryPlayerController.Instance.LoadStoryTest("demo");
        if (GUILayout.Button("Test2"))
        {
            if(ps.Count != 0)
            {
                for(int i = ps.Count - 1; i >= 0; i--)
                {
                    Destroy(ps[i]);
                }
                ps.Clear();
            }

            if (skelg != null)
            {
                int count = 0;
                Vector2 sum = Vector2.zero ;

                float scale = 1f / skelg.SkeletonDataAsset.scale;
                Vector2 rootPos = new Vector2(skelg.Skeleton.RootBone.X, skelg.Skeleton.RootBone.Y) * scale;

                foreach (var kv in skelg.Skeleton.Skin.Attachments)
                {
                    var slot = skelg.Skeleton.Slots.Items[kv.Key.SlotIndex];
                    if (slot.Data.Name != ("00") && slot.Data.Name != ("00_default") && slot.Data.Name != ("defalt")) // Fuck nexon
                        continue;
                    
                    float scaleX = slot.Bone.Data.ScaleX;
                    float scaleY = slot.Bone.Data.ScaleY;

                    Vector2 offset = rootPos + new Vector2(slot.Bone.Data.X * scale, slot.Bone.Data.Y * scale);
                        Debug.Log($"{slot.Bone.Data.Name} | {offset}");
                    Vector2[] buf = new Vector2[((VertexAttachment)kv.Value).WorldVerticesLength];
                    foreach (var v2 in ((VertexAttachment)kv.Value).GetLocalVertices(skelg.Skeleton.Slots.Items[kv.Key.SlotIndex], buf))
                    {
                        if (v2 == Vector2.zero)
                            continue;

                        GameObject obj = new GameObject($"{kv.Key.Attachment.Name}|{v2 * scale + offset}", typeof(Image));
                        obj.transform.SetParent(skelg.transform);
                        obj.transform.localScale = Vector3.one * 0.15f;

                        var rect = obj.GetComponent<RectTransform>();
                        rect.anchorMin = rect.anchorMax = skelg.transform.GetComponent<RectTransform>().pivot;
                        rect.anchoredPosition = v2 * scale * scaleX + offset;
                        sum += rect.anchoredPosition;
                        count++;
                        ps.Add(obj);
                    }
                }

                Vector2 avg = sum / count;
                GameObject face = new GameObject("face", typeof(Image));
                face.GetComponent<Image>().color = Color.red;
                face.transform.SetParent(skelg.transform);
                face.transform.localScale = Vector3.one * 0.2f;
                var rectf = face.GetComponent<RectTransform>();
                rectf.anchorMin = rectf.anchorMax = skelg.transform.GetComponent<RectTransform>().pivot;
                rectf.anchoredPosition = avg;
                ps.Add(face);
            }
        }
        if (GUILayout.Button("TestEvent"))
        {
            
        }
    }

}
