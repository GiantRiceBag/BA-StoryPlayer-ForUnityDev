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
using System.Linq;
using System.IO;
using BAStoryPlayer.Event;
using System.Reflection;
using BAStoryPlayer.Utility;
using Unity.Burst.Intrinsics;
using static UnityEngine.Video.VideoPlayer;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public BAStoryPlayer.BAStoryPlayer storyPlayer;
    public string storyScriptName = "MS_Test";
    Action test;
    public SkeletonGraphic skelg;
    List<GameObject> ps = new List<GameObject>();

    public GameObject obj;

    private void OnGUI()
    {
        storyScriptName = GUILayout.TextField(storyScriptName,15);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Play"))
        {
            Dictionary<string, int> flags = new();
            flags.Add("TestFlag1", 2);
            if (Application.isPlaying)
            {
                storyPlayer.LoadStory(storyScriptName, flags);
                storyPlayer.OnStoryPlayerClosed += (scripts, flags) =>
                {
                    foreach(var i in scripts)
                    {
                        Debug.Log($"script:{i}");
                    }
                    foreach (var i in flags)
                    {
                        Debug.Log($"flag:{i}");
                    }
                };
            }
        }
        GUILayout.Label("！！！！！！！！！！！！！！！！");
        if (GUILayout.Button("Test Something"))
        {

        }
    }
}
