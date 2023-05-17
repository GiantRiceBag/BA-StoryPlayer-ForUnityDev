using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using BAStoryPlayer;
using Spine.Unity.AttachmentTools;
using Spine;
public class Test : MonoBehaviour
{
    public int index = 0;
    private void Start()
    {
        //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(2, "shiroko", "01");
        //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(1, "azusa", "01");
        //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(0, "hoshino", "01");
        //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(3, "hihumi", "01");
        //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(4, "aru", "01");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(0, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(1, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(2, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(3, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(4, (CharacterEmotion)index);
            index++;
            index %= 19;
        }
        if (Input.GetMouseButtonDown(1))
        {
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(0, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(1, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(2, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(3, (CharacterEmotion)index);
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(4, (CharacterEmotion)index);
            index %= 19;
        }
    }
}
