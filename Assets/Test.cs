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

    public GameObject obj;

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

        if(GUILayout.Button("Test Sequence"))
        {
            ASD();
        }
        if (GUILayout.Button("Test Ease"))
        {
            obj.transform.DoLocalMove(obj.transform.position + Vector3.right * 5, 1).SetEase(Ease.InOutCubic);
        }
    }

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        ASD();
    }

    void ASD()
    {
        TweenSequence sequence_Rotation = new TweenSequence();
        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, -10), 0.3f).SetEase(Ease.OutCubic));
        sequence_Rotation.Wait(0.1f);
        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, 5), 0.5f).SetEase(Ease.OutCubic));
        sequence_Rotation.Wait(0.3f);
        sequence_Rotation.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(0, 1500, 0), 0.3f).SetEase(Ease.InCirc));

        TweenSequence sequence_Position = new TweenSequence();
        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition + new Vector3(30, 0, 0), 0.3f).SetEase(Ease.OutCubic));
        sequence_Position.Wait(0.1f);
        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(60, 0, 0), 0.5f).SetEase(Ease.OutCubic));
    }
}
