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
using UnityEngine.UIElements;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public BAStoryPlayer.BAStoryPlayer storyPlayer;
    public string storyScriptName = "MS_Test";
    Action test;
    public SkeletonGraphic skelg;
    List<GameObject> ps = new List<GameObject>();

    public GameObject obj;
    public List<string> msg = new();
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
            PrintLog();

            if (GUILayout.Button("Test"))
            {
                StartCoroutine(CrtLoadCharacterSpine("yuzu"));
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
                    yield return CrtLoadBackground(rawStoryUnit.backgroundImage);
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
                    yield return CrtLoadBGM(rawStoryUnit.bgm);
                }
            }
            foreach(var characterUnit in rawStoryUnit.characters)
            {
                if (string.IsNullOrEmpty(characterUnit.name) || storyPlayer.CharacterModule.CharacterPool.ContainsKey(characterUnit.name))
                {
                    continue;
                }

                GameObject obj = storyPlayer.CharacterModule.CreateCharacterObj(characterUnit.name);
                if (obj == null)
                {
                    yield return CrtLoadCharacterSpine(characterUnit.name);
                }
                else
                {
                    obj.SetActive(false);
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
                                yield return CrtLoadBackground(unit.backgroundImage);
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
                                yield return CrtLoadBGM(unit.bgm);
                            }
                        }

                        foreach(var chrUnit in unit.characters)
                        {
                            if (string.IsNullOrEmpty(chrUnit.name) || storyPlayer.CharacterModule.CharacterPool.ContainsKey(chrUnit.name))
                            {
                                continue;
                            }

                            GameObject obj = storyPlayer.CharacterModule.CreateCharacterObj(chrUnit.name);
                            if (obj == null)
                            {
                                yield return CrtLoadCharacterSpine(chrUnit.name);
                            }
                            else
                            {
                                obj.SetActive(false);
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
    private IEnumerator CrtLoadCharacterSpine(string chrName)
    {
        string chrNameWithSuffix = $"{chrName}_spr";
        string url = $"https://yuuka.cdn.diyigemt.com/image/ba-all-data/spine/{chrNameWithSuffix}/{chrNameWithSuffix}";

        TextAsset atlas = null;
        using (UnityWebRequest atlasRequest = UnityWebRequest.Get($"{url}.atlas"))
        {
            Log($"正在下载 {chrNameWithSuffix}.atlas", Color.white);
            yield return atlasRequest.SendWebRequest();
            if (atlasRequest.result != UnityWebRequest.Result.Success)
            {
                Log($"下载 {chrNameWithSuffix}.atlas 失败", Color.red);
                yield break;
            }
            else
            {
                atlas = new TextAsset(atlasRequest.downloadHandler.text);
                atlas.name = $"{chrNameWithSuffix}";
                Log($"下载 {chrNameWithSuffix}.atlas 成功", Color.green);
            }
        }

        Texture2D texture = new Texture2D(1, 1);
        using (UnityWebRequest pngRequest = UnityWebRequest.Get($"{url}.png"))
        {
            Log($"正在下载 {chrNameWithSuffix}.png", Color.white);
            yield return pngRequest.SendWebRequest();

            if (pngRequest.result != UnityWebRequest.Result.Success)
            {
                Log($"下载 {chrNameWithSuffix}.png 失败", Color.red);
                yield break;
            }
            else
            {
                Log($"下载 {chrNameWithSuffix}.png 成功", Color.green);
                byte[] imgBytes = pngRequest.downloadHandler.data;
                texture.LoadImage(imgBytes);
                texture.name = $"{chrNameWithSuffix}";
            }
        }

        SpineAtlasAsset spineAtlasAsset =
            SpineAtlasAsset.CreateRuntimeInstance(
                atlas,
                new[] { texture },
                new Material(Resources.Load<Shader>("Shader/Spine-SkeletonGraphic")),
                true
            );

        SkeletonDataAsset runtimeSkeletonDataAsset = null;
        using (UnityWebRequest skelRequest = UnityWebRequest.Get($"{url}.skel"))
        {
            Log($"正在下载 {chrNameWithSuffix}.skel", Color.white);
            yield return skelRequest.SendWebRequest();

            if (skelRequest.result != UnityWebRequest.Result.Success)
            {
                Log($"下载 {chrNameWithSuffix}.skel 失败", Color.red);
                yield break;
            }
            else
            {
                Log($"下载 {chrNameWithSuffix}.skel 成功", Color.green);
                byte[] skelBytes = skelRequest.downloadHandler.data;

                AtlasAttachmentLoader attachmentLoader = new AtlasAttachmentLoader(
                    spineAtlasAsset.GetAtlas()
                );
                SkeletonBinary binary = new SkeletonBinary(attachmentLoader)
                {
                    Scale = 0.01f
                };
                MemoryStream skelStream = new MemoryStream(skelBytes);

                SkeletonData skeletonData = binary.ReadSkeletonData(skelStream);
                skeletonData.Name = $"{chrName}";
                AnimationStateData stateData = new AnimationStateData(skeletonData);

                runtimeSkeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();

                Type skeletonDataAssetType = runtimeSkeletonDataAsset.GetType();
                FieldInfo skeletonDataField = skeletonDataAssetType.GetField("skeletonData", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo stateDataField = skeletonDataAssetType.GetField("stateData", BindingFlags.NonPublic | BindingFlags.Instance);
                skeletonDataField.SetValue(runtimeSkeletonDataAsset, skeletonData);
                stateDataField.SetValue(runtimeSkeletonDataAsset, stateData);

                runtimeSkeletonDataAsset.atlasAssets = new[] { spineAtlasAsset };

                runtimeSkeletonDataAsset.skeletonJSON = new TextAsset("1") { name = $"{chrName}.skel" };
            }
        }

        SkeletonGraphic instance = SkeletonGraphic.NewSkeletonGraphicGameObject(
            runtimeSkeletonDataAsset, storyPlayer.transform.parent, new Material(Resources.Load<Shader>("Shader/Spine-SkeletonGraphic"))
        );

        instance.Initialize(false);
        instance.Skeleton.SetSlotsToSetupPose();

        storyPlayer.CharacterModule.AddPreloadedCharacter(instance.gameObject, chrName);
    }
    private IEnumerator CrtLoadBackground(string backgroundName)
    {

        Sprite sprite = Resources.Load<Sprite>(storyPlayer.Setting.PathBackground + backgroundName);
        if (sprite != null)
        {
            storyPlayer.BackgroundModule.PreloadedImages.Add(backgroundName, sprite);
        }
        else
        {
            string url = $"https://image.blue-archive.io/01_Background/{backgroundName}.jpg";

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                preloadingMsg.Add(new Msg()
                {
                    text = $"正在载入背景 {backgroundName}.jpg",
                    color = Color.white
                });
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    storyPlayer.BackgroundModule.PreloadedImages.Add(
                            backgroundName,
                            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero)
                        );

                    preloadingMsg.Add(new Msg()
                    {
                        text = $"载入背景 {backgroundName}.jpg 成功",
                        color = Color.green
                    });
                }
                else
                {
                    preloadingMsg.Add(new Msg()
                    {
                        text = $"载入背景 {backgroundName}.jpg 失败",
                        color = Color.red
                    });
                }
            }
        }
    }
    private IEnumerator CrtLoadBGM(string bgmName)
    {

        AudioClip clip = Resources.Load<AudioClip>(storyPlayer.Setting.PathMusic + bgmName);
        if (clip != null)
        {
            storyPlayer.AudioModule.PreloadedMusicClips.Add(bgmName, clip);
        }
        else
        {
            string url = $"https://bgm.blue-archive.io/{bgmName}.mp3";

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                preloadingMsg.Add(new Msg()
                {
                    text = $"正在载入音乐 {bgmName}.mp3",
                    color = Color.white
                });
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    clip = DownloadHandlerAudioClip.GetContent(request);
                    storyPlayer.AudioModule.PreloadedMusicClips.Add(
                            bgmName,
                            clip
                        );
                    preloadingMsg.Add(new Msg()
                    {
                        text = $"载入音乐 {bgmName}.mp3 成功",
                        color = Color.green
                    });
                }
                else
                {
                    preloadingMsg.Add(new Msg()
                    {
                        text = $"载入音乐 {bgmName}.mp3 失败",
                        color = Color.red
                    });
                }
            }
        }
    }

    private string GetTimestamp()
    {
        return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
    }
    private void Log(string msg,Color color)
    {
        preloadingMsg.Add(new Msg()
        {
            text = msg,
            color = color
        });
    }
    private void PrintLog()
    {
        if(preloadingMsg.Count <= 0)
        {
            return;
        }
        foreach(var i in preloadingMsg.Reverse<Msg>())
        {
            GUILabelWithColor(i.text,i.color);
        }
    }
    private void GUILabelWithColor(string msg,Color color)
    {
        GUILayout.Label(new GUIContent()
        {
            text = msg
        }, new GUIStyle()
        {
            font = guiSkin.font,
            normal = new GUIStyleState()
            {
                textColor = color
            }
        });
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
