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

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public string storyScriptName = "TestScript";
    Action test;
    private void Start()
    {
        Dictionary<string, string> www = new Dictionary<string, string>();
        www.Add("shiroko", "spr");
        test += () => { Debug.Log(www["shiroko"]); };
    }

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
        if (GUILayout.Button("demo2"))
        {

        }
        if (GUILayout.Button("Test3"))
        {
            TextAsset demo = Resources.Load("demo") as TextAsset;
            List<string> AsCommandList = new List<string>(demo.ToString().Split('\n'));
            AsCommandList.RemoveAll(string.IsNullOrEmpty);
            AsCommandList = AsCommandList.Where(item => !item.StartsWith("//")).ToList();
            foreach (var i in AsCommandList)
            {
                Debug.Log(i+"#"+i.Length);
            }
        }
    }
}
