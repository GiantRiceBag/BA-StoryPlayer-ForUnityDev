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
using System.Reflection;

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
        GUILayout.Label("！！！！！！！！！！！！！！！！");
        if (GUILayout.Button("Test  As Story Script"))
            if (Application.isPlaying)
                BAStoryPlayerController.Instance.LoadStoryTest("demo");
        if (GUILayout.Button("Test Universal Story Script"))
        {
            var rss = Resources.Load<TextAsset>("StoryScript/UniversalStoryScriptSample");
            var ss = JsonUtility.FromJson<UniversalStoryScript>(rss.text);
            Debug.Log(ss.ToString());

            if (Application.isPlaying)
                BAStoryPlayerController.Instance.LoadStoryTest("UniversalStoryScriptSample");
        }
    }
}
