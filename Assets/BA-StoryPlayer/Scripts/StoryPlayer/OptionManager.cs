using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public struct OptionData
    {
        public int optionID;
        public string text;

        public OptionData(int id,string text)
        {
            this.optionID = id;
            this.text = text;
        }
    }

    public class OptionManager : MonoBehaviour
    {
        private GameObject btnPrefab;
        private GameObject BtnPrefab
        {
            get
            {
                if (btnPrefab == null)
                    btnPrefab = Resources.Load<GameObject>("UI/Option");
                return btnPrefab;
            }
        }

        private GameObject block;

        private void Start()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = rect.anchoredPosition = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }

        private void AddOption(OptionData data)
        {
            GameObject obj = Instantiate(BtnPrefab);
            obj.transform.SetParent(transform);

            var option = obj.GetComponent<Button_Option>();
            option.Initialize(data.optionID, $"\"{ data.text}\"");
        }
        public void AddOptions(System.Collections.Generic.List<OptionData> datas)
        {
            foreach (var i in datas)
                AddOption(i);
        }

        public void RevokeInteractablilty(Transform exception = null)
        {
            GetComponent<VerticalLayoutGroup>().enabled = false;
            CreateMouseBlock();

            foreach (var i in GetComponentsInChildren<Button>())
                if(i.transform != exception.transform)
                    i.interactable = false;
        }

       private void CreateMouseBlock()
        {
            GameObject obj = new GameObject();
            obj.name = "Block";
            obj.transform.SetParent(transform);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = rect.anchoredPosition = Vector2.zero;
            obj.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            obj.transform.localScale = Vector3.one;
            block = Instantiate(obj);
        }

        public void FinishSelecting()
        {
            if (block != null)
                Destroy(block);

            // һ����˵ѡ����Զ�ִ����һ����Ԫ
            BAStoryPlayerController.Instance.StoryPlayer.ReadyToNext(true);
            Destroy(gameObject);
        }
    }

}
