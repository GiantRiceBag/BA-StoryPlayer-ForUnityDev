using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public class OptionData
    {
        [Obsolete] public int optionID;
        public string text;
        public List<Condition> conditions;
        public List<StoryUnit> storyUnits;
        public string script;

        public bool IsConditional => conditions != null && conditions.Count > 0;

        [Obsolete]
        public OptionData(int id, string text)
        {
            this.optionID = id;
            this.text = text;
        }
        public OptionData(string text,string script,List<StoryUnit> storyUnits = null, List<Condition> conditions = null)
        {
            this.text = text;
            this.script = script;
            this.storyUnits = storyUnits;
            this.conditions = conditions;
        }
    }

    public class OptionManager : MonoBehaviour
    {
        private GameObject _block;
        private GameObject _btnPrefab;
        private GameObject BtnPrefab
        {
            get
            {
                if (_btnPrefab == null)
                {
                    _btnPrefab = Resources.Load<GameObject>("UI/Option");
                }
                return _btnPrefab;
            }
        }

        public BAStoryPlayer StoryPlayer { get;private set; }

        private void Start()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = rect.anchoredPosition = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }

        private bool AddOption(OptionData data)
        {
            foreach(var condition in data.conditions)
            {
                if (!condition.Validate(StoryPlayer.FlagTable))
                {
                    return false;
                }
            }

            GameObject obj = Instantiate(BtnPrefab);
            obj.transform.SetParent(transform);

            var option = obj.GetComponent<ButtonOption>();
            option.Initialize(this,data);

            return true;
        }
        public void AddOptions(List<OptionData> datas,BAStoryPlayer storyPlayer)
        {
            StoryPlayer = storyPlayer;
            int optionsCount = 0;
            foreach (var i in datas)
            {
                if (AddOption(i))
                {
                    optionsCount++;
                }
            }

            if(optionsCount == 0)
            {
                FinishSelecting(true);
            }
        }

        public void RevokeInteractablilty(Transform exception = null)
        {
            GetComponent<VerticalLayoutGroup>().enabled = false;
            CreateMouseBlock();

            foreach (var i in GetComponentsInChildren<Button>())
            {
                if (i.transform != exception.transform)
                {
                    i.interactable = false;
                }

            }
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
            _block = Instantiate(obj);
        }

        public void FinishSelecting(bool nextUnit = false)
        {
            if (_block != null)
            {
                Destroy(_block);
            }

            // 一般来说选完后自动执行下一个单元
            if(nextUnit)
            {
                StoryPlayer.ToNextStoryUnit();
            }
            StoryPlayer.ReadyToNext(true);
            Destroy(gameObject);
        }
    }

}
