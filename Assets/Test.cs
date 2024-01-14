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

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public BAStoryPlayer.BAStoryPlayer storyPlayer;
    public string storyScriptName = "TestScript";
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
            if (Application.isPlaying)
            {
                storyPlayer.LoadStory(storyScriptName);
            }
        }
        GUILayout.Label("！！！！！！！！！！！！！！！！");
    }
}
