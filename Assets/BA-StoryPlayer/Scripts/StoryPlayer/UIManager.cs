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
    public enum TextType
    {
        mainText = 0,
        speaker
    }

    public class UIManager : MonoBehaviour
    {
        const int NUM_CHAR_PERSECOND = 20; // ÿ���ӡ����
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

            onFinishedPrinting += () => { gameObject_Continued.SetActive(true); };
        }

        /// <summary>
        /// ����˵������Ϣ
        /// </summary>
        /// <param name="romaji">˵����������/��Ϊ������ʾ˵������Ϣ</param>
        public void UpdateSpeaker(string romaji = null)
        {
            if (currentSpeaker == romaji)
                return;

            // ��ɫ˵��
            if(romaji != null)
            {
                var data = BAStoryPlayerController.Instance.CharacterDataTable[romaji];
                string result = $"{data.name} <color=#9CD7EF><size=39>{data.affiliation}</size></color>";
                text_Speaker.text = result;
            }
            // �Ա�
            else
            {
                text_Speaker = null;
            }

            currentSpeaker = romaji;
        }
        /// <summary>
        /// ������ı�
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
        /// �����ı�
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

        public void SetBlurBackgroup(bool enable)
        {
            image_BlurLayer.DoFloat("_Size", enable ? 3 : 0, TIME_BLUR_BACKGROUP);
        }

        public void SetBackgroup(Sprite sprite)
        {
            image_Backgroup.sprite = sprite;
        }

        public void ShowTitle(string title,string subtitle)
        {
            GameObject obj = Instantiate( Resources.Load("UI/Title") as GameObject);
            obj.transform.SetParent(transform);
            obj.GetComponent<Title>().Initialize(title, subtitle);

        }
        public void ShowOption(List<OptionData> dates)
        {
            GameObject obj = Instantiate(Resources.Load("UI/OptionManager") as GameObject);
            obj.transform.SetParent(StoryPlayer.transform);
            obj.GetComponent<OptionManager>().AddOptions(dates);
        }

        // TODO TEST
        public void TestPrint()
        {
            Print("��� �Һ� ��Һ� ��� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ���� �Һ� ��Һ�");
            UpdateSpeaker("hoshino");
        }
        public void TestTitle()
        {
            ShowTitle("Ұ���ȱ��ĵ���","������");
        }
        public void TestOption()
        {
            List<OptionData> dats = new List<OptionData>();
            dats.Add(new OptionData(1, "����1ѡ��"));
            dats.Add(new OptionData(2, "����2ѡ��"));
            dats.Add(new OptionData(3, "����3ѡ��"));
            ShowOption(dats);
        }
    }
}

