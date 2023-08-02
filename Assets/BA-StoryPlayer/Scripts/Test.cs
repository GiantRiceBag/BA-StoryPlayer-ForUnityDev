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

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public string storyScriptName = "TestScript";

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
        if(GUILayout.Button("Test"))
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction("shiroko", CharacterAction.Appear);
        if(GUILayout.Button("Test2"))
            Debug.Log(BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.CharacterPool["shiroko"] == null);
    }
}
