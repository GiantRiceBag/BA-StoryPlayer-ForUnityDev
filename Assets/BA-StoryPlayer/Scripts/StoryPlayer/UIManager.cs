using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BAStoryPlayer
{
    public enum TextType
    {
        mainText = 0,
        speaker
    }

    public class UIManager : MonoBehaviour
    {
        const int NUM_CHAR_PERSECOND = 20; // 每秒打印字数
        const float INTERVAL_PRINT = 1 / (float)NUM_CHAR_PERSECOND;

        [Header("References")]
        [SerializeField] TextMeshProUGUI text_Speaker;
        [SerializeField] TextMeshProUGUI text_Main;
        [Space]
        [SerializeField] GameObject gameObject_TextArea;
        [SerializeField] GameObject gameObject_Continued;
        [SerializeField] Button btn_Auto;
        [SerializeField] Button btn_Menu;

        Coroutine coroutine_Print;

        string currentSpeaker = null;
        string mainTextBuffer = null;
        bool printing = false;

        BAStoryPlayer StoryPlayer
        {
            get
            {
                return BAStoryPlayerController.Instance.StoryPlayer;
            }
        }

        public event Action onFinishedPrinting;

        private void Start()
        {
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

            onFinishedPrinting += () => { gameObject_Continued.SetActive(true); };
        }

        /// <summary>
        /// 更新说话者信息
        /// </summary>
        /// <param name="romaji">说话者罗马音/若为空则不显示说话者信息</param>
        public void UpdateSpeaker(string romaji = null)
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
        public void Print(string text)
        {
            text_Main.text = null;
            gameObject_Continued.SetActive(false);
            SetActive_UI_TextArea(true);

            printing = true;
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
            printing = false;
            mainTextBuffer = null;

            onFinishedPrinting?.Invoke();
        }

        /// <summary>
        /// 跳过文本
        /// </summary>
        public void Skip()
        {
            if (!printing)
                return;

            StopCoroutine(coroutine_Print);
            text_Main.text = mainTextBuffer;
            mainTextBuffer = null;
            printing = false;

            onFinishedPrinting?.Invoke();
        }

        public void ClearText()
        {
            text_Main.text = null;
            text_Speaker.text = null;
        }

        public void SetActive_UI_Button(bool enable)
        {
            btn_Auto.gameObject.SetActive(enable);
            btn_Menu.gameObject.SetActive(enable);
        }
        public void SetActive_UI_TextArea(bool enable)
        {
            gameObject_TextArea.SetActive(enable);
            if (!enable)
                gameObject_Continued.SetActive(enable);
        }

        // TODO TEST
        public void TestPrint()
        {
            Print("你好 我好 大家好 你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好你好 我好 大家好");
            UpdateSpeaker("hoshino");
        }
    }
}

