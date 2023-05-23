using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer.UI
{
    public class Emotion : MonoBehaviour
    {
        const float TIME_TRANSITION = 0.2f;

        /// <summary>
        /// λ�ó�ʼ�� �����½Ƕ���Ϊê��
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
                    Debug.LogWarning("δ�ҵ�ͷ����λ�� ��ʹ���׸��Ӷ�����Ϊ�ο�");
                    locator = parent.GetChild(0);
                }
                if(locator == null)
                {
                    Debug.LogError($"��ɫ {parent} �������Ӷ�����Ϊ��λ�� �봴��һ����ΪHeadLocator���Ӷ����϶�����ɫ��������λ��");
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
            //TODO �����¼�
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
