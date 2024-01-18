using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using BAStoryPlayer;
using Spine.Unity.AttachmentTools;
using Spine;
using BAStoryPlayer.DoTweenS;
using System;
using System.Linq;
using System.IO;
using BAStoryPlayer.Event;
using System.Reflection;
using BAStoryPlayer.Utility;
using Unity.Burst.Intrinsics;
using static UnityEngine.Video.VideoPlayer;
using Unity.VisualScripting;
using Unity.Mathematics;
using UnityEngine.Networking;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public BAStoryPlayer.BAStoryPlayer storyPlayer;
    public string storyScriptName = "MS_Test";
    Action test;
    public SkeletonGraphic skelg;
    List<GameObject> ps = new List<GameObject>();

    public GameObject obj;
    public string msg = string.Empty;

    private bool isDrawingRemoteScript;
    private void OnGUI()
    {
        if (isDrawingRemoteScript)
        {
            DrawRemoteScript();
        }
        else
        {
            storyScriptName = GUILayout.TextField(storyScriptName, 15);
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
                        foreach (var i in scripts)
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
            if(GUILayout.Button("Play Remote Scripts"))
            {
                isDrawingRemoteScript = true;

                isReflashing = true;
                scriptsUID.Clear();
                storyScripts.Clear();
                StartCoroutine(CrtReflashScripts());
            }
            GUILayout.Label("！！！！！！！！！！！！！！！！");
            if (GUILayout.Button("Test Something"))
            {

            }
        }


    }

    private List<string> scriptsUID = new();
    private List<UniversalStoryScript> storyScripts = new();
    private bool isReflashing = false;
    private Vector2 scrollPos = new Vector2(10, 30);

    private void DrawRemoteScript()
    {
        if (GUILayout.Button("Exit"))
        {
            isDrawingRemoteScript = false;
            isReflashing = false;
            StopAllCoroutines();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reflash"))
        {
            if(!isReflashing)
            {
                isReflashing = true;
                scriptsUID.Clear();
                storyScripts.Clear();
                StartCoroutine(CrtReflashScripts());
            }
        }
        GUILayout.FlexibleSpace();
        if (scriptsUID.Count > 0)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            DrawScriptButton();
            GUILayout.EndScrollView();
        }
    }

    private void DrawScriptButton()
    {
        foreach(var script in storyScripts)
        {
            if (GUILayout.Button(script.serial))
            {
                if (Application.isPlaying)
                {
                    storyPlayer.LoadStory(script);
                }
                else
                {
                    Debug.Log(JsonUtility.ToJson(script,true));
                }
                
            }
        }
    }
    private IEnumerator CrtReflashScripts()
    {
        string url = $"https://api.blue-archive.io/storys?t={GetTimestamp()}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                scriptsUID = data.data;

                foreach(var i in scriptsUID)
                {
                    yield return StartCoroutine(CrtGetScript(i));
                }
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
            }
        }
        isReflashing = true;
    }
    private IEnumerator CrtGetScript(string uid)
    {
        string url = $"https://api.blue-archive.io/story/{uid}?t={GetTimestamp()}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ResponseScriptData data = JsonUtility.FromJson<ResponseScriptData>(request.downloadHandler.text);
                storyScripts.Add(data.data);
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
            }
        }
    }
    private string GetTimestamp()
    {
        return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
    }

    public class ResponseData
    {
        public List<string> data;
    }
    public class ResponseScriptData
    {
        public UniversalStoryScript data;
    }
}
