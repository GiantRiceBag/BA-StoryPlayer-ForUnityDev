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
using static UnityEngine.UI.CanvasScaler;
using UnityEditor;

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
    public GUISkin guiSkin;
    private bool isDrawingRemoteScript;

    private void Start()
    {
        if (Application.isPlaying)
        {
            storyPlayer.gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (guiSkin)
        {
            GUI.skin = guiSkin;
        }

        if (isDrawingRemoteScript)
        {
            DrawRemoteScript();
        }
        else
        {
            storyScriptName = GUILayout.TextField(storyScriptName, 15);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("播放剧情（本地）"))
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
            if(GUILayout.Button("播放剧情（编辑器）"))
            {
                isDrawingRemoteScript = true;

                isReflashing = true;
                scriptsUID.Clear();
                storyScripts.Clear();
                StartCoroutine(CrtReflashScripts());
            }

            DrawCuttingLine();

            if (GUILayout.Button("Test"))
            {

            }
        }
    }

    private List<string> scriptsUID = new();
    private List<UniversalStoryScript> storyScripts = new();
    private bool isReflashing = false;
    private Vector2 scriptsScrollPos = new Vector2(10, 30);
    private Vector2 msgScrollPos = new Vector2(10, 30);
    private bool isPreloadingAsset;
    private List<Msg> preloadingMsg = new();

    struct Msg
    {
        public string text;
        public Color color;
    }

    private void DrawRemoteScript()
    {
        if (GUILayout.Button("返回"))
        {
            if(isPreloadingAsset)
            {
                return;
            }

            isDrawingRemoteScript = false;
            isReflashing = false;
            StopAllCoroutines();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("刷新"))
        {
            if (isPreloadingAsset)
            {
                return;
            }

            if (!isReflashing)
            {
                isReflashing = true;
                scriptsUID.Clear();
                storyScripts.Clear();
                StartCoroutine(CrtReflashScripts());
            }
        }

        if (storyPlayer.IsPlaying)
        {
            GUILayout.BeginVertical(guiSkin.box);
            GUILayout.Label("*剧情播放中*",new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState()
                {
                    textColor = Color.white,
                    background = guiSkin.box.normal.background
                }
            });
            GUILayout.EndVertical();
            return;
        }
        GUILayout.FlexibleSpace();
        if( isReflashing )
        {
            GUILayout.BeginVertical(guiSkin.box);
            GUILayout.Label("加载编辑器剧本中...", new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState()
                {
                    textColor = Color.white,
                    background = guiSkin.box.normal.background
                }
            });
            GUILayout.EndVertical();
        }

        if (!isPreloadingAsset)
        {
            if (Application.isPlaying && storyPlayer.gameObject.activeSelf)
            {
                return;
            }


            if (scriptsUID.Count > 0)
            {
                scriptsScrollPos = GUILayout.BeginScrollView(scriptsScrollPos);
                DrawScriptButton();
                GUILayout.EndScrollView();
                
            }
        }
        else
        {
            if(preloadingMsg.Count> 0)
            {
                GUILayout.BeginVertical(guiSkin.box);
                msgScrollPos = GUILayout.BeginScrollView(msgScrollPos);
                foreach(var msg in preloadingMsg.Reverse<Msg>())
                {
                    GUILayout.Label(new GUIContent()
                    {
                        text = msg.text
                    }, new GUIStyle()
                    {
                        normal = new GUIStyleState()
                        {
                            textColor = msg.color
                        }
                    });
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }
    }
    private void DrawScriptButton()
    {
        foreach (var script in storyScripts)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新"))
            {
                if (isReflashing)
                {
                    return;
                }

                isReflashing = true;
                int index = storyScripts.IndexOf(script);
                StartCoroutine(CrtGetScript(script.uuid, data =>
                {
                    storyScripts[index] = data;
                    isReflashing = false;
                }));
            }
            if (GUILayout.Button(script.serial))
            {
                if (Application.isPlaying)
                {
                    if (isReflashing)
                    {
                        isReflashing = false;
                        StopAllCoroutines();
                    }

                    StartCoroutine(CrtPreloadAssetAndPlayStory(script));
                }
                else
                {
                    Debug.Log(JsonUtility.ToJson(script,true));
                }
            }
            GUILayout.EndHorizontal();
        }
    }
    private void DrawCuttingLine()
    {
        GUILayout.Label("————————————");
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
                    yield return StartCoroutine(CrtGetScript(i, data =>
                    {
                        storyScripts.Add(data);
                    }));
                }
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
            }
        }
        isReflashing = false;
    }
    private IEnumerator CrtGetScript(string uid,Action<UniversalStoryScript> action = null)
    {
        string url = $"https://api.blue-archive.io/story/{uid}?t={GetTimestamp()}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ResponseScriptData data = JsonUtility.FromJson<ResponseScriptData>(request.downloadHandler.text);
                action?.Invoke(data.data);
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
            }
        }
    }
    private IEnumerator CrtPreloadAssetAndPlayStory(UniversalStoryScript storyScript)
    {
        if (isPreloadingAsset || storyPlayer.gameObject.activeSelf)
        {
            yield break;
        }
        storyPlayer.ClearPreloadAsset();
        isPreloadingAsset = true;

        foreach (var rawStoryUnit in storyScript.content)
        {
            if (!string.IsNullOrEmpty(rawStoryUnit.backgroundImage))
            {
                if (storyPlayer.BackgroundModule.PreloadedImages.ContainsKey(rawStoryUnit.backgroundImage))
                {
                    continue;
                }

                Sprite sprite = Resources.Load<Sprite>(storyPlayer.Setting.PathBackground + rawStoryUnit.backgroundImage);
                if (sprite != null)
                {
                    storyPlayer.BackgroundModule.PreloadedImages.Add(rawStoryUnit.backgroundImage, sprite);
                }
                else
                {
                    string url = $"https://image.blue-archive.io/01_Background/{rawStoryUnit.backgroundImage}.jpg";

                    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                    {
                        preloadingMsg.Add(new Msg()
                        {
                            text = $"正在载入背景 {rawStoryUnit.backgroundImage}.jpg",
                            color = Color.white
                        });
                        yield return request.SendWebRequest();

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(request);
                            storyPlayer.BackgroundModule.PreloadedImages.Add(
                                    rawStoryUnit.backgroundImage,
                                    Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero)
                                );

                            preloadingMsg.Add(new Msg()
                            {
                                text = $"载入背景 {rawStoryUnit.backgroundImage}.jpg 成功",
                                color = Color.green
                            });
                        }
                        else
                        {
                            preloadingMsg.Add(new Msg()
                            {
                                text = $"载入背景 {rawStoryUnit.backgroundImage}.jpg 失败",
                                color = Color.red
                            });
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(rawStoryUnit.bgm))
            {
                if (storyPlayer.AudioModule.PreloadedMusicClips.ContainsKey(rawStoryUnit.bgm))
                {
                    continue;
                }

                AudioClip clip = Resources.Load<AudioClip>(storyPlayer.Setting.PathMusic + rawStoryUnit.bgm);
                if (clip != null)
                {
                    storyPlayer.AudioModule.PreloadedMusicClips.Add(rawStoryUnit.bgm, clip);
                }
                else
                {
                    string url = $"https://bgm.blue-archive.io/{rawStoryUnit.bgm}.mp3";

                    using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
                    {
                        preloadingMsg.Add(new Msg()
                        {
                            text = $"正在载入音乐 {rawStoryUnit.bgm}.mp3",
                            color = Color.white
                        });
                        yield return request.SendWebRequest();

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            clip = DownloadHandlerAudioClip.GetContent(request);
                            storyPlayer.AudioModule.PreloadedMusicClips.Add(
                                    rawStoryUnit.bgm,
                                    clip
                                );
                            preloadingMsg.Add(new Msg()
                            {
                                text = $"载入音乐 {rawStoryUnit.bgm}.mp3 成功",
                                color = Color.green
                            });
                        }
                        else
                        {
                            preloadingMsg.Add(new Msg()
                            {
                                text = $"载入音乐 {rawStoryUnit.bgm}.mp3 失败",
                                color = Color.red
                            });
                        }
                    }     
                }
            }

            if(rawStoryUnit.type == "select")
            {
                foreach(var selection in rawStoryUnit.selectionGroups)
                {
                    foreach(var unit in selection.content)
                    {
                        if (!string.IsNullOrEmpty(unit.backgroundImage))
                        {
                            if (storyPlayer.BackgroundModule.PreloadedImages.ContainsKey(unit.backgroundImage))
                            {
                                continue;
                            }

                            Sprite sprite = Resources.Load<Sprite>(storyPlayer.Setting.PathBackground+ unit.backgroundImage);
                            if (sprite != null)
                            {
                                storyPlayer.BackgroundModule.PreloadedImages.Add(unit.backgroundImage, sprite);
                            }
                            else
                            {
                                string url = $"https://image.blue-archive.io/01_Background/{unit.backgroundImage}.jpg";

                                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                                {
                                    preloadingMsg.Add(new Msg()
                                    {
                                        text = $"正在载入背景 {unit.backgroundImage}.jpg",
                                        color = Color.white
                                    });
                                    yield return request.SendWebRequest();

                                    if (request.result == UnityWebRequest.Result.Success)
                                    {
                                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                                        storyPlayer.BackgroundModule.PreloadedImages.Add(
                                                unit.backgroundImage,
                                                Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero)
                                            );
                                        preloadingMsg.Add(new Msg()
                                        {
                                            text = $"载入背景 {unit.backgroundImage}.jpg 成功",
                                            color = Color.green
                                        });
                                    }
                                    else
                                    {
                                        preloadingMsg.Add(new Msg()
                                        {
                                            text = $"载入背景 {unit.backgroundImage}.jpg 失败",
                                            color = Color.red
                                        });
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(unit.bgm))
                        {
                            if (storyPlayer.AudioModule.PreloadedMusicClips.ContainsKey(unit.bgm))
                            {
                                continue;
                            }

                            AudioClip clip = Resources.Load<AudioClip>(storyPlayer.Setting.PathMusic + unit.bgm);
                            if (clip != null)
                            {
                                storyPlayer.AudioModule.PreloadedMusicClips.Add(unit.bgm, clip);
                            }
                            else
                            {
                                string url = $"https://bgm.blue-archive.io/{unit.bgm}.mp3";

                                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
                                {
                                    preloadingMsg.Add(new Msg()
                                    {
                                        text = $"正在载入音乐 {unit.bgm}.mp3",
                                        color = Color.white
                                    });
                                    yield return request.SendWebRequest();

                                    if (request.result == UnityWebRequest.Result.Success)
                                    {
                                        AudioClip aclip = DownloadHandlerAudioClip.GetContent(request);
                                        storyPlayer.AudioModule.PreloadedMusicClips.Add(
                                                unit.bgm,
                                                aclip
                                            );
                                        preloadingMsg.Add(new Msg()
                                        {
                                            text = $"载入音乐 {unit.bgm}.mp3 成功",
                                            color = Color.green
                                        });
                                    }
                                    else
                                    {
                                        preloadingMsg.Add(new Msg()
                                        {
                                            text = $"载入音乐 {unit.bgm}.mp3 失败",
                                            color = Color.red
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        isPreloadingAsset = false;
        preloadingMsg.Clear() ;

        storyPlayer.LoadStory(storyScript);
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
