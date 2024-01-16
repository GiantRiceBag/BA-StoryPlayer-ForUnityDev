using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;
using BAStoryPlayer.Event;

namespace BAStoryPlayer
{
    public class UIManager : PlayerModule
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _txtSpeaker;
        [SerializeField] private TextMeshProUGUI _txtMain;
        [SerializeField] private TextMeshProUGUI _txtTitle;
        [SerializeField] private TextMeshProUGUI _txtSynopsis;
        [Space]
        [SerializeField] private GameObject _objTextArea;
        [SerializeField] private GameObject _objContinued;
        [SerializeField] private GameObject _objSubPanelSynopsis;
        [SerializeField] private Button _btnAuto;
        [SerializeField] private Button _btnMenu;

        private Coroutine _crtPrint;
        private Coroutine _crtNext;

        private string _currentSpeaker = null;
        private string _mainTextBuffer = null;
        private bool _isPrintingText = false;

        public bool IsPrintingText => _isPrintingText;

        private void Start()
        {
            if (_txtSpeaker == null)
                _txtSpeaker = transform.Find("TextArea").Find("Text_Speaker").GetComponent<TextMeshProUGUI>();
            if (_txtMain == null)
                _txtSpeaker = transform.Find("TextArea").Find("Text_Main").GetComponent<TextMeshProUGUI>();

            if (_objTextArea == null)
                _objTextArea = transform.Find("TextArea").gameObject;
            if (_objContinued == null)
                _objContinued = transform.Find("Image_Continued").gameObject;
            if (_objSubPanelSynopsis == null)
                _objSubPanelSynopsis = transform.Find("SubPanel_Synopsis").gameObject;
            if (_btnAuto == null)
                _btnAuto = transform.Find("Button_Auto").GetComponent<Button>();
            if (_btnMenu == null)
                _btnMenu = transform.Find("Button_Menu").GetComponent<Button>();
        }

        private void OnEnable()
        {
            EventBus<OnFinishedPrintingMainText>.Binding.Add(OnFinishedPringtingMainTextEventHandler);
            EventBus<OnCanceledAuto>.Binding.Add(OnCanceldAutoEventHandler);
        }
        private void OnDisable()
        {
            EventBus<OnFinishedPrintingMainText>.Binding.Remove(OnFinishedPringtingMainTextEventHandler);
            EventBus<OnCanceledAuto>.Binding.Remove(OnCanceldAutoEventHandler);
        }

        [System.Obsolete]
        public void SetSpeaker(string indexName = null)
        {
            if (_currentSpeaker == indexName && indexName != null)
                return;

            // 角色说话
            if(indexName != null)
            {
                var data = StoryPlayer.CharacterDataTable[indexName];
                SetSpeaker(data.firstName, data.affiliation);
            }
            // 旁白
            else
            {
                _txtSpeaker.text = null;
            }

            _currentSpeaker = indexName;
        }
        public void SetSpeaker(string speakerName = null,string affiliation = null)
        {
            _txtSpeaker.text = $"{speakerName} <color=#9CD7EF><size=39>{affiliation}</size></color>";
        }

        public void PrintMainText(string text)
        {
            EventBus<OnStartPrintingMainText>.Raise();

            _txtMain.text = null;
            _objContinued.SetActive(false);
            SetActiveTextArea();
            SetActiveButton();

            _isPrintingText = true;
            _mainTextBuffer = text;
            if (_crtPrint != null)
                StopCoroutine(_crtPrint);
            _crtPrint = StartCoroutine(CrtPrint());
        }
        IEnumerator CrtPrint()
        {
            for(int i = 0; i < _mainTextBuffer.Length; i++)
            {
                _txtMain.text += _mainTextBuffer[i];
                yield return new WaitForSeconds(StoryPlayer.Setting.IntervalPrint);
            }
            _crtPrint = null;
            _isPrintingText = false;
            _mainTextBuffer = null;

            EventBus<OnFinishedPrintingMainText>.Raise();
        }

        public void Skip()
        {
            if (!_isPrintingText)
            {
                return;
            }

            StopCoroutine(_crtPrint);
            _txtMain.text = _mainTextBuffer;
            _mainTextBuffer = null;
            _isPrintingText = false;

            EventBus<OnFinishedPrintingMainText>.Raise();
        }

        public void ClearText()
        {
            _txtMain.text = null;
            _txtSpeaker.text = null;
        }

        public void SetActiveButton(bool enable = true)
        {
            _btnAuto.gameObject.SetActive(enable);
            _btnMenu.gameObject.SetActive(enable);
        }
        public void SetActiveTextArea(bool enable = true)
        {
            _objTextArea.SetActive(enable);
            if (!enable)
            {
                _objContinued.SetActive(enable);
            }
        }

        public void HideAllUI()
        {
            SetActiveButton(false);
            SetActiveTextArea(false);
            _objSubPanelSynopsis.SetActive(false);
        }

        /// <summary>
        /// 显示标题并关闭所有UI
        /// </summary>
        public void ShowTitle(string subtitle,string title)
        {
            GameObject obj = Instantiate(Resources.Load("UI/Title") as GameObject);
            obj.transform.SetParent(transform);
            obj.GetComponent<Title>().Initialize(StoryPlayer,title, subtitle);

            HideAllUI();
        }
        public void ShowOptions(List<OptionData> dates)
        {
            GameObject obj = Instantiate(Resources.Load("UI/OptionManager") as GameObject);
            obj.transform.SetParent(StoryPlayer.transform);
            obj.GetComponent<OptionManager>().AddOptions(dates,StoryPlayer);
        }
        public void ShowVenue(string venue)
        {
            GameObject obj = Instantiate(Resources.Load("UI/Venue") as GameObject);
            obj.transform.SetParent(StoryPlayer.transform);
            obj.GetComponent<Venue>().SetText(venue);
        }

        public void SetSynopsis(string synopsis = "")
        {
            _txtSynopsis.text = synopsis;
        }
        public void SetTitle(string title = "") 
        {
            _txtTitle.text = title;
        }

        private void OnFinishedPringtingMainTextEventHandler(OnFinishedPrintingMainText data)
        {
            _objContinued.SetActive(true);

            // 若Auto则延缓两秒后继续
            if (StoryPlayer.IsAuto)
            {
                _crtNext = this.Delay(() => { StoryPlayer.ReadyToExecute(); }, 2);
            }
            else
            {
                StoryPlayer.ReadyToExecute();
            }
        }
        private void OnCanceldAutoEventHandler(OnCanceledAuto data)
        {
            if (_crtNext != null)
            {
                StopCoroutine(_crtNext);
                _crtNext = null;
                StoryPlayer.ReadyToExecute();
            }
        }
    }
}

