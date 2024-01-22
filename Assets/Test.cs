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
    public GUISkin guiSkin;

    private CustomGUI guiHomePage;
    private CustomGUI guiRemoteScriptsPage;

    public CustomGUI HomePage
    {
        get
        {
            if(guiHomePage == null)
            {
                guiHomePage = new GUIHomePage(this);
            }
            return guiHomePage;
        }
    }
    public CustomGUI RemoteScriptsPage
    {
        get
        {
            if(guiRemoteScriptsPage == null)
            {
                guiRemoteScriptsPage = new GUIRemoteScriptsPage(this);
            }
            return guiRemoteScriptsPage;
        }
    }

    private CustomGUI CurrentPage;

    private void Start()
    {
        if (Application.isPlaying)
        {
            storyPlayer.gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if(guiSkin == null)
        {
            return;
        }
        if(CurrentPage == null)
        {
            CurrentPage = HomePage;
            CurrentPage.OnStart();
        }
        GUI.skin = guiSkin;
        CustomGUI.guiSkin = guiSkin;
        CurrentPage.OnGUI();
    }

    public void SwitchPage(CustomGUI gui)
    {
        CurrentPage = gui;
        CurrentPage.OnStart();
    }

    private class ResponseDataScriptsUID
    {
        public List<string> data;
    }
    private class ResponseDataStoryScripts
    {
        public UniversalStoryScript data;
    }
    public struct GUIMessage
    {
        public string text;
        public Color color;
    }

    public abstract class CustomGUI
    {
        public Vector2 messageScrollPos = new Vector2(10, 30);
        public static List<GUIMessage> messages = new();
        public static GUISkin guiSkin;
        public Test testor;

        public static string GetTimestamp()
        {
            return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        }
        public static void AddMessage(string msg, Color color)
        {
            messages.Add(new GUIMessage()
            {
                text = msg,
                color = color
            });
        }
        public static void PrintMessages()
        {
            if (messages.Count <= 0)
            {
                return;
            }
            foreach (var i in messages.Reverse<GUIMessage>())
            {
                GUILabelWithColor(i.text, i.color);
            }
        }
        public static void ClearMessages()
        {
            messages.Clear();
        }
        private static void GUILabelWithColor(string msg, Color color)
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
        public static void DrawCuttingLine()
        {
            GUILayout.Label("————————————");
        }

        public CustomGUI(Test testor)
        {
            this.testor = testor;
        }
        public abstract void OnGUI();
        public virtual void OnStart() { }

        protected void DrawPlayerDetailsWhilePlaying()
        {
            GUILayout.BeginVertical(guiSkin.box);
            GUIStyle style = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState()
                {
                    textColor = Color.white,
                    background = guiSkin.box.normal.background
                }
            };

            GUILayout.Label("*剧情播放中*", style);
            GUILayout.Label($"进度：{testor.storyPlayer.CurrentUnitIndex}/{testor.storyPlayer.UnitCount}", style);
            GUILayout.Label($"分支单元：{testor.storyPlayer.IsBranchUnit}", style);
            GUILayout.EndVertical();
        }
    }
    protected class GUIHomePage : CustomGUI
    {
        public string storyScriptName = "MS_Test";

        public GUIHomePage(Test testor) : base(testor) { }

        public override void OnGUI()
        {
            storyScriptName = GUILayout.TextField(storyScriptName, 15);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("播放剧情（本地）"))
            {
                Dictionary<string, int> flags = new();
                flags.Add("TestFlag1", 2);
                if (Application.isPlaying)
                {
                    testor.storyPlayer.LoadStory(storyScriptName, flags);
                    testor.storyPlayer.OnStoryPlayerClosed += (scripts, flags) =>
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
            if (GUILayout.Button("播放剧情（编辑器）"))
            {
                testor.SwitchPage(testor.RemoteScriptsPage);
            }

            DrawCuttingLine();
            if (testor.storyPlayer.IsPlaying)
            {
                DrawPlayerDetailsWhilePlaying();
                return;
            }

            if (GUILayout.Button("Test"))
            {
                
            }
        }
    }
    protected class GUIRemoteScriptsPage : CustomGUI
    {
        private List<string> scriptsUID = new();
        private List<UniversalStoryScript> storyScripts = new();
        private bool isReflashing = false;
        private Vector2 scriptsScrollPos = new Vector2(10, 30);
        private bool isPreloadingAsset;

        private Dictionary<string, Sprite> backgroundCache = new();
        private Dictionary<string, AudioClip> bgmCache = new();
        private Dictionary<string, SkeletonDataAsset> sprCache = new();

        public GUIRemoteScriptsPage(Test testor) : base(testor) { }

        public override void OnGUI()
        {
            if (GUILayout.Button("返回"))
            {
                if (isPreloadingAsset)
                {
                    return;
                }
                isReflashing = false;
                testor.StopAllCoroutines();
                testor.SwitchPage(testor.HomePage);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("刷新"))
            {
                if (isPreloadingAsset)
                {
                    return;
                }
                testor.StartCoroutine(CrtReflashScripts());
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("清除缓存"))
            {
                if(isPreloadingAsset || testor.storyPlayer.IsPlaying)
                {
                    return;
                }

                foreach(var texKV in backgroundCache)
                {
                    DestroyImmediate(texKV.Value);
                }
                backgroundCache.Clear();

                foreach(var bgmKV in bgmCache)
                {
                    DestroyImmediate(bgmKV.Value);
                }
                bgmCache.Clear();

                foreach(var sprKV in sprCache)
                {
                    DestroyImmediate(sprKV.Value);
                }
                sprCache.Clear();
            }

            if (testor.storyPlayer.IsPlaying)
            {
                DrawPlayerDetailsWhilePlaying();
                return;
            }
            GUILayout.FlexibleSpace();
            if (isReflashing)
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
                if (Application.isPlaying && testor.storyPlayer.gameObject.activeSelf)
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
                if (messages.Count > 0)
                {
                    GUILayout.BeginVertical(guiSkin.box);
                    messageScrollPos = GUILayout.BeginScrollView(messageScrollPos);
                    foreach (var msg in messages.Reverse<GUIMessage>())
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
        public override void OnStart()
        {
            testor.StartCoroutine(CrtReflashScripts());
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
                    testor.StartCoroutine(CrtGetScript(script.uuid, data =>
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
                            testor.StopAllCoroutines();
                        }

                        testor.StartCoroutine(CrtPreloadAssetAndPlayStory(script));
                    }
                    else
                    {
                        Debug.Log(JsonUtility.ToJson(script, true));
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private IEnumerator CrtReflashScripts()
        {
            if (isReflashing)
            {
                yield break;
            }
            scriptsUID.Clear();
            storyScripts.Clear();
            isReflashing = true;
            string url = $"https://api.blue-archive.io/storys?t={GetTimestamp()}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    ResponseDataScriptsUID data = JsonUtility.FromJson<ResponseDataScriptsUID>(request.downloadHandler.text);
                    scriptsUID = data.data;

                    foreach (var i in scriptsUID)
                    {
                        yield return testor.StartCoroutine(CrtGetScript(i, data =>
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
        private IEnumerator CrtGetScript(string uid, Action<UniversalStoryScript> action = null)
        {
            string url = $"https://api.blue-archive.io/story/{uid}?t={GetTimestamp()}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    ResponseDataStoryScripts data = JsonUtility.FromJson<ResponseDataStoryScripts>(request.downloadHandler.text);
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
            if (isPreloadingAsset || testor.storyPlayer.gameObject.activeSelf)
            {
                yield break;
            }
            testor.storyPlayer.ClearPreloadAsset();
            isPreloadingAsset = true;

            foreach (var rawStoryUnit in storyScript.content)
            {
                if (!string.IsNullOrEmpty(rawStoryUnit.backgroundImage))
                {
                    if (testor.storyPlayer.BackgroundModule.PreloadedImages.ContainsKey(rawStoryUnit.backgroundImage))
                    {
                        continue;
                    }

                    Sprite sprite = Resources.Load<Sprite>(testor.storyPlayer.Setting.PathBackground + rawStoryUnit.backgroundImage);
                    if (sprite != null)
                    {
                        testor.storyPlayer.BackgroundModule.PreloadedImages.Add(rawStoryUnit.backgroundImage, sprite);
                    }
                    else
                    {
                        yield return CrtLoadBackground(rawStoryUnit.backgroundImage);
                    }
                }
                if (!string.IsNullOrEmpty(rawStoryUnit.bgm))
                {
                    if (testor.storyPlayer.AudioModule.PreloadedMusicClips.ContainsKey(rawStoryUnit.bgm))
                    {
                        continue;
                    }

                    AudioClip clip = Resources.Load<AudioClip>(testor.storyPlayer.Setting.PathMusic + rawStoryUnit.bgm);
                    if (clip != null)
                    {
                        testor.storyPlayer.AudioModule.PreloadedMusicClips.Add(rawStoryUnit.bgm, clip);
                    }
                    else
                    {
                        yield return CrtLoadBGM(rawStoryUnit.bgm);
                    }
                }
                foreach (var characterUnit in rawStoryUnit.characters)
                {
                    if (string.IsNullOrEmpty(characterUnit.name) || testor.storyPlayer.CharacterModule.CharacterPool.ContainsKey(characterUnit.name))
                    {
                        continue;
                    }

                    GameObject obj = testor.storyPlayer.CharacterModule.CreateCharacterObj(characterUnit.name);
                    if (obj == null)
                    {
                        yield return CrtLoadCharacterSpine(characterUnit.name);
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                }

                if (rawStoryUnit.type == "select")
                {
                    foreach (var selection in rawStoryUnit.selectionGroups)
                    {
                        foreach (var unit in selection.content)
                        {
                            if (!string.IsNullOrEmpty(unit.backgroundImage))
                            {
                                if (testor.storyPlayer.BackgroundModule.PreloadedImages.ContainsKey(unit.backgroundImage))
                                {
                                    continue;
                                }

                                Sprite sprite = Resources.Load<Sprite>(testor.storyPlayer.Setting.PathBackground + unit.backgroundImage);
                                if (sprite != null)
                                {
                                    testor.storyPlayer.BackgroundModule.PreloadedImages.Add(unit.backgroundImage, sprite);
                                }
                                else
                                {
                                    yield return CrtLoadBackground(unit.backgroundImage);
                                }
                            }
                            if (!string.IsNullOrEmpty(unit.bgm))
                            {
                                if (testor.storyPlayer.AudioModule.PreloadedMusicClips.ContainsKey(unit.bgm))
                                {
                                    continue;
                                }

                                AudioClip clip = Resources.Load<AudioClip>(testor.storyPlayer.Setting.PathMusic + unit.bgm);
                                if (clip != null)
                                {
                                    testor.storyPlayer.AudioModule.PreloadedMusicClips.Add(unit.bgm, clip);
                                }
                                else
                                {
                                    yield return CrtLoadBGM(unit.bgm);
                                }
                            }

                            foreach (var chrUnit in unit.characters)
                            {
                                if (string.IsNullOrEmpty(chrUnit.name) || testor.storyPlayer.CharacterModule.CharacterPool.ContainsKey(chrUnit.name))
                                {
                                    continue;
                                }

                                GameObject obj = testor.storyPlayer.CharacterModule.CreateCharacterObj(chrUnit.name);
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
            messages.Clear();

            testor.storyPlayer.LoadStory(storyScript);
        }
        private IEnumerator CrtLoadCharacterSpine(string chrName)
        {
            if(sprCache.ContainsKey(chrName))
            {
                SkeletonDataAsset skelAsset = sprCache[chrName];

                SkeletonGraphic skelGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(
                    skelAsset, testor.storyPlayer.transform.parent, new Material(Resources.Load<Shader>("Shader/Spine-SkeletonGraphic"))
                );

                skelGraphic.Initialize(false);
                skelGraphic.Skeleton.SetSlotsToSetupPose();

                testor.storyPlayer.CharacterModule.AddPreloadedCharacter(skelGraphic.gameObject, chrName);
                yield break;
            }

            string chrNameWithSuffix = $"{chrName}_spr";
            string url = $"https://yuuka.cdn.diyigemt.com/image/ba-all-data/spine/{chrNameWithSuffix}/{chrNameWithSuffix}";

            TextAsset atlas = null;
            using (UnityWebRequest atlasRequest = UnityWebRequest.Get($"{url}.atlas"))
            {
                AddMessage($"正在下载 {chrNameWithSuffix}.atlas", Color.white);
                yield return atlasRequest.SendWebRequest();
                if (atlasRequest.result != UnityWebRequest.Result.Success)
                {
                    AddMessage($"下载 {chrNameWithSuffix}.atlas 失败", Color.red);
                    yield break;
                }
                else
                {
                    atlas = new TextAsset(atlasRequest.downloadHandler.text);
                    atlas.name = $"{chrNameWithSuffix}";
                    AddMessage($"下载 {chrNameWithSuffix}.atlas 成功", Color.green);
                }
            }

            Texture2D texture = new Texture2D(1, 1);
            using (UnityWebRequest pngRequest = UnityWebRequest.Get($"{url}.png"))
            {
                AddMessage($"正在下载 {chrNameWithSuffix}.png", Color.white);
                yield return pngRequest.SendWebRequest();

                if (pngRequest.result != UnityWebRequest.Result.Success)
                {
                    AddMessage($"下载 {chrNameWithSuffix}.png 失败", Color.red);
                    yield break;
                }
                else
                {
                    AddMessage($"下载 {chrNameWithSuffix}.png 成功", Color.green);
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
                AddMessage($"正在下载 {chrNameWithSuffix}.skel", Color.white);
                yield return skelRequest.SendWebRequest();

                if (skelRequest.result != UnityWebRequest.Result.Success)
                {
                    AddMessage($"下载 {chrNameWithSuffix}.skel 失败", Color.red);
                    yield break;
                }
                else
                {
                    AddMessage($"下载 {chrNameWithSuffix}.skel 成功", Color.green);
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
                runtimeSkeletonDataAsset, testor.storyPlayer.transform.parent, new Material(Resources.Load<Shader>("Shader/Spine-SkeletonGraphic"))
            );
            sprCache.Add(chrName, runtimeSkeletonDataAsset);
            instance.Initialize(false);
            instance.Skeleton.SetSlotsToSetupPose();

            testor.storyPlayer.CharacterModule.AddPreloadedCharacter(instance.gameObject, chrName);
        }
        private IEnumerator CrtLoadBackground(string backgroundName)
        {
            if (backgroundCache.ContainsKey(backgroundName))
            {
                testor.storyPlayer.BackgroundModule.PreloadedImages.Add(
                        backgroundName,
                        backgroundCache[backgroundName]
                    );
                yield break;
            }

            Sprite sprite = Resources.Load<Sprite>(testor.storyPlayer.Setting.PathBackground + backgroundName);
            if (sprite != null)
            {
                testor.storyPlayer.BackgroundModule.PreloadedImages.Add(backgroundName, sprite);
            }
            else
            {
                string url = $"https://image.blue-archive.io/01_Background/{backgroundName}.jpg";

                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                {
                    messages.Add(new GUIMessage()
                    {
                        text = $"正在载入背景 {backgroundName}.jpg",
                        color = Color.white
                    });
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        testor.storyPlayer.BackgroundModule.PreloadedImages.Add(
                                backgroundName,
                                sprite
                            );
                        backgroundCache.Add(backgroundName, sprite);

                        messages.Add(new GUIMessage()
                        {
                            text = $"载入背景 {backgroundName}.jpg 成功",
                            color = Color.green
                        });
                    }
                    else
                    {
                        messages.Add(new GUIMessage()
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
            if (bgmCache.ContainsKey(bgmName))
            {
                testor.storyPlayer.AudioModule.PreloadedMusicClips.Add(bgmName, bgmCache[bgmName]);
                yield break;
            }

            AudioClip clip = Resources.Load<AudioClip>(testor.storyPlayer.Setting.PathMusic + bgmName);
            if (clip != null)
            {
                testor.storyPlayer.AudioModule.PreloadedMusicClips.Add(bgmName, clip);
            }
            else
            {
                string url = $"https://bgm.blue-archive.io/{bgmName}.mp3";

                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
                {
                    messages.Add(new GUIMessage()
                    {
                        text = $"正在载入音乐 {bgmName}.mp3",
                        color = Color.white
                    });
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        clip = DownloadHandlerAudioClip.GetContent(request);
                        testor.storyPlayer.AudioModule.PreloadedMusicClips.Add(
                                bgmName,
                                clip
                            );
                        bgmCache.Add(bgmName,clip);
                        messages.Add(new GUIMessage()
                        {
                            text = $"载入音乐 {bgmName}.mp3 成功",
                            color = Color.green
                        });
                    }
                    else
                    {
                        messages.Add(new GUIMessage()
                        {
                            text = $"载入音乐 {bgmName}.mp3 失败",
                            color = Color.red
                        });
                    }
                }
            }
        }
    }
}
