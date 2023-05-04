using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;

namespace BAStoryPlayer
{
    public class UIManager : MonoBehaviour
    {
        const int NUM_CHAR_PERSECOND = 20; // 每秒打印字数
        const float INTERVAL_PRINT = 1 / (float)NUM_CHAR_PERSECOND;
        const float TIME_BLUR_BACKGROUP = 0.7f;

        [Header("References")]
        [SerializeField] Image image_Backgroup;
        [SerializeField] Image image_BlurLayer;
        [Space]
        [SerializeField] TextMeshProUGUI text_Speaker;
        [SerializeField] TextMeshProUGUI text_Main;
        [Space]
        [SerializeField] GameObject gameObject_TextArea;
        [SerializeField] GameObject gameObject_Continued;
        [SerializeField] Button btn_Auto;
        [SerializeField] Button btn_Menu;

        Coroutine coroutine_Print;
        Coroutine coroutine_Next;

        string currentSpeaker = null;
        string mainTextBuffer = null;
        bool isPrinting = false;

        public bool IsPriting
        {
            get
            {
                return isPrinting;
            }
        }

        BAStoryPlayer StoryPlayer
        {
            get
            {
                return BAStoryPlayerController.Instance.StoryPlayer;
            }
        }

        [HideInInspector] public UnityEngine.Events.UnityEvent  onFinishPrinting;

        private void Start()
        {
            if(image_Backgroup == null)
                image_Backgroup = transform.parent.Find("Backgroup").GetComponent<Image>();
            if (image_BlurLayer == null)
                image_BlurLayer = image_Backgroup.transform.Find("BlurLayer").GetComponent<Image>();

            if (text_Speaker == null)
                text_Speaker = transform.Find("TextArea").Find("Text_Speaker").GetComponent<TextMeshProUGUI>();
            if(text_Main == null)
                text_Speaker = transform.Find("TextArea").Find("Text_Main").GetComponent<TextMeshProUGUI>();

            if (gameObject_TextArea == null)
                gameObject_TextArea = transform.Find("TextArea").gameObject;
            if (gameObject_Continued == null)
                gameObject_Continued = transform.Find("Image_Continued").gameObject;
            if (btn_Auto == null)
                btn_Auto = transform.Find("Button_Auto").GetComponent<Button>();
            if (btn_Menu == null)
                btn_Menu = transform.Find("Button_Menu").GetComponent<Button>();

            // 事件绑定
            onFinishPrinting.AddListener(() => { 
                gameObject_Continued.SetActive(true);

                // 若Auto则延缓两秒后继续
                if (StoryPlayer.Auto)
                    coroutine_Next = BAStoryPlayer.Delay(transform, () => { StoryPlayer.ReadyToNext(); }, 2);
                else
                    StoryPlayer.ReadyToNext();
            });

            // 若取消Auto 则删除当前执行的协程
            StoryPlayer.OnCancelAuto.AddListener(() => {
                if(coroutine_Next != null)
                {
                    StopCoroutine(coroutine_Next);
                    coroutine_Next = null;
                    StoryPlayer.ReadyToNext();
                }
                    
            });
        }

        /// <summary>
        /// 更新说话者信息
        /// </summary>
        /// <param name="romaji">说话者罗马音/若为空则不显示说话者信息</param>
        public void SetSpeaker(string romaji = null)
        {
            if (currentSpeaker == romaji)
                return;

            // 角色说话
            if(romaji != null)
            {
                var data = BAStoryPlayerController.Instance.CharacterDataTable[romaji];
                string result = $"{data.name} <color=#9CD7EF><size=39>{data.affiliation}</size></color>";
                text_Speaker.text = result;
            }
            // 旁边
            else
            {
                text_Speaker = null;
            }

            currentSpeaker = romaji;
        }
        /// <summary>
        /// 输出主文本
        /// </summary>
        /// <param name="text"></param>
        public void PrintText(string text)
        {
            text_Main.text = null;
            gameObject_Continued.SetActive(false);
            SetActive_UI_TextArea();
            SetActive_UI_Button();

            isPrinting = true;
            mainTextBuffer = text;
            if (coroutine_Print != null)
                StopCoroutine(coroutine_Print);
            coroutine_Print = StartCoroutine(CPrint());
        }
        IEnumerator CPrint()
        {
            for(int i = 0; i < mainTextBuffer.Length; i++)
            {
                text_Main.text += mainTextBuffer[i];
                yield return new WaitForSeconds(INTERVAL_PRINT);
            }
            coroutine_Print = null;
            isPrinting = false;
            mainTextBuffer = null;

            onFinishPrinting?.Invoke();
        }

        /// <summary>
        /// 跳过文本
        /// </summary>
        public void Skip()
        {
            if (!isPrinting)
                return;

            StopCoroutine(coroutine_Print);
            text_Main.text = mainTextBuffer;
            mainTextBuffer = null;
            isPrinting = false;

            onFinishPrinting?.Invoke();
        }

        public void ClearText()
        {
            text_Main.text = null;
            text_Speaker.text = null;
        }

        public void SetActive_UI_Button(bool enable = true)
        {
            btn_Auto.gameObject.SetActive(enable);
            btn_Menu.gameObject.SetActive(enable);
        }
        public void SetActive_UI_TextArea(bool enable = true)
        {
            gameObject_TextArea.SetActive(enable);
            if (!enable)
                gameObject_Continued.SetActive(enable);
        }

        public void HideAllUI()
        {
            SetActive_UI_Button(false);
            SetActive_UI_TextArea(false);
        }

        public void SetBlurBackgroup(bool enable)
        {
            image_BlurLayer.DoFloat("_Size", enable ? 3 : 0, TIME_BLUR_BACKGROUP);
        }

        public void SetBackgroup(Sprite sprite)
        {
            image_Backgroup.sprite = sprite;
        }

        /// <summary>
        /// 显示标题并关闭所有UI
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        public void ShowTitle(string title,string subtitle)
        {
            GameObject obj = Instantiate( Resources.Load("UI/Title") as GameObject);
            obj.transform.SetParent(transform);
            obj.GetComponent<Title>().Initialize(title, subtitle);

            HideAllUI();
        }
        public void ShowOption(List<OptionData> dates)
        {
            GameObject obj = Instantiate(Resources.Load("UI/OptionManager") as GameObject);
            obj.transform.SetParent(StoryPlayer.transform);
            obj.GetComponent<OptionManager>().AddOptions(dates);
        }
        public void ShowVenue(string venue)
        {
            GameObject obj = Instantiate(Resources.Load("UI/Venue") as GameObject);
            obj.transform.SetParent(StoryPlayer.transform);
            obj.GetComponent<Venue>().SetText(venue);
        }

        // TODO TEST
        public void TestPrint()
        {
            PrintText("你好 我好 大家好 你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好");
            SetSpeaker("hoshino");
        }
        public void TestTitle()
        {
            ShowTitle("野兽先辈的调教","妙妙屋");
        }
        public void TestOption()
        {
            List<OptionData> dats = new List<OptionData>();
            dats.Add(new OptionData(1, "今天要抢哪一个银行?"));
            dats.Add(new OptionData(2, "对不起 我拒绝"));
            ShowOption(dats);
        }
    }
}

