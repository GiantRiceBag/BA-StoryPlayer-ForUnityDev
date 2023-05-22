using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer.UI
{
    public class Emotion : MonoBehaviour
    {
        const float TIME_TRANSITION = 0.2f;

        /// <summary>
        /// 位置初始化 以左下角顶点为锚点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        public void Initlaize(Transform parent,Vector2 pos,LocateMode locateMode)
        {
            if(locateMode == LocateMode.Auto)
            {
                var rect = GetComponent<RectTransform>();
                rect.SetParent(parent);
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.anchoredPosition = pos * parent.GetComponent<RectTransform>().sizeDelta;
            }else if (locateMode == LocateMode.Manual)
            {
                Transform locator = parent.Find("HeadLocator");
                if (locator == null && parent.childCount != 0)
                {
                    Debug.LogWarning("未找到头部定位器 已使用首个子对象作为参考");
                    locator = parent.GetChild(0);
                }
                if(locator == null)
                {
                    Debug.LogError($"角色 {parent} 不存在子对象作为定位器 请创建一个名为HeadLocator的子对象并拖动到角色脸部中央位置");
                    return;
                }

                locator.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                var rect = GetComponent<RectTransform>();
                rect.SetParent(locator);
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.anchoredPosition = pos;
            }

        }

        public void RunOnComplete()
        {
            //TODO 发送事件
            var images = GetComponentsInChildren<Image>();
            for(int i = 0; i < images.Length; i++)
            {
                if (i == images.Length - 1)
                    images[i].DoAlpha(0, TIME_TRANSITION).onComplete = () => { Destroy(gameObject); };
                else 
                    images[i].DoAlpha(0, TIME_TRANSITION);
            }
        }

    }

}
