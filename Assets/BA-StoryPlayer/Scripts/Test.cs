using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using BAStoryPlayer;
using Spine.Unity.AttachmentTools;
using Spine;
using BAStoryPlayer.DoTweenS;
public class Test : MonoBehaviour
{
    public List<SkeletonGraphic> skels;
    public GameObject prefab;
    public SkeletonGraphic skel;
    public Vector3 TestV3;

    public int index = 0;
    //private void Start()
    //{
    //    //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(2, "shiroko", "01");
    //    //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(1, "azusa", "01");
    //    //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(0, "hoshino", "01");
    //    //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(3, "hihumi", "01");
    //    //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(4, "aru", "01");

    //    //var slot = skel.Skeleton.FindSlot("eyeclose");

    //    foreach (var skel in skels)
    //    {
    //        skel.UpdateMesh(true);
    //        Vector3 sum = Vector3.zero;
    //        int count = 0;
    //        Debug.Log($"{skel.name} size {skel.GetLastMesh().bounds.size}");
    //        Debug.Log($"{skel.name} center {skel.GetLastMesh().bounds.center}");

    //        GameObject pivot = Instantiate(prefab, skel.transform);
    //        pivot.GetComponent<RectTransform>().anchorMin = pivot.GetComponent<RectTransform>().anchorMax = Vector2.zero;
    //        pivot.transform.localPosition = skel.GetLastMesh().GetSubMesh(0).bounds.center;
    //        pivot.name = "pivot";

    //        float heightAbovePivot = skel.GetLastMesh().bounds.size.y - pivot.GetComponent<RectTransform>().anchoredPosition.y;

    //        float arg = 254f / 1327f;
    //        float xRange = 300;
    //        foreach (var i in skel.GetLastMesh().vertices)
    //        {
    //            if (i.y < 700 || i.y > heightAbovePivot - heightAbovePivot * arg || Mathf.Abs(i.x) > xRange)
    //                continue;
    //            Debug.Log(i);
    //            GameObject point = Instantiate(prefab, skel.transform);
    //            point.GetComponent<RectTransform>().anchorMin = point.GetComponent<RectTransform>().anchorMax = skel.GetComponent<RectTransform>().pivot;
    //            point.transform.localPosition = i;
    //            point.name = count.ToString();
    //            point.GetComponent<Image>().color = Color.blue;
    //            sum += i;
    //            count++;
    //        }

    //        GameObject faceCenter = Instantiate(prefab, skel.transform);
    //        faceCenter.GetComponent<RectTransform>().anchorMin = faceCenter.GetComponent<RectTransform>().anchorMax = skel.GetComponent<RectTransform>().pivot;
    //        faceCenter.name = "faceCenter";
    //        faceCenter.GetComponent<Image>().color = Color.green;
    //        faceCenter.transform.localPosition = sum / count;

    //    }
    //}

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(0, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(1, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(2, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(3, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(4, (CharacterEmotion)index);
        //    index++;
        //    index %= 19;
        //}
        //if (Input.GetMouseButtonDown(1))
        //{
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(0, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(1, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(2, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(3, (CharacterEmotion)index);
        //    BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(4, (CharacterEmotion)index);
        //    index %= 19;
        //}

        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach(var i in skels)
            {
                EmotionFactory.SetEmotion(i.transform, (CharacterEmotion)index);
                index++;
                index %= 19;
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(2, CharacterAction.falldownR);

        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            skel.transform.rotation = oq;
            BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(2, "shiroko", "01");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            BAStoryPlayerController.Instance.StoryPlayer.CloseStoryPlayer();
        }


    }

    private void Start()
    {
        //BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(2, "shiroko", "01");
    }
    Quaternion oq;
    Vector3 op;
    Coroutine co;
}
