using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;
using BAStoryPlayer.Utility;
using BAStoryPlayer.Event;

namespace BAStoryPlayer
{
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _imgBackground;
        [Space]
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
        private bool _isPrinting = false;

        private BAStoryPlayer StoryPlayer { get { return BAStoryPlayerController.Instance.StoryPlayer; } }

        public bool IsPrinting => _isPrinting;

        private void Start()
        {
            if (_imgBackground == null)
                _imgBackground = transform.parent.Find("Background").GetComponent<Image>();

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

            // �¼���
            EventBus<OnPrintedLine>.Binding.Add(() =>
                {
                    _objContinued.SetActive(true);

                    // ��Auto���ӻ���������
                    if (StoryPlayer.IsAuto)
                        _crtNext = Timer.Delay(transform, () => { StoryPlayer.ReadyToNext();}, 2);
                    else
                        StoryPlayer.ReadyToNext();
                });

            // ��ȡ��Auto ��ɾ����ǰִ�е�Э��
            EventBus<OnPlayerCanceledAuto>.Binding.Add(() =>
            {
                if (_crtNext != null)
                {
                    StopCoroutine(_crtNext);
                    _crtNext = null;
                    StoryPlayer.ReadyToNext();
                }
            });
        }

        /// <summary>
        /// ����˵������Ϣ
        /// </summary>
        /// <param name="indexName">˵����������/��Ϊ������ʾ˵������Ϣ</param>
        [System.Obsolete]
        public void SetSpeaker(string indexName = null)
        {
            if (_currentSpeaker == indexName && indexName != null)
                return;

            // ��ɫ˵��
            if(indexName != null)
            {
                var data = BAStoryPlayerController.Instance.CharacterDataTable[indexName];
                SetSpeaker(data.firstName, data.affiliation);
            }
            // �԰�
            else
            {
                _txtSpeaker.text = null;
            }

            _currentSpeaker = indexName;
        }
        public void SetSpeaker(string speakerName,string affiliation)
        {
            _txtSpeaker.text = $"{speakerName} <color=#9CD7EF><size=39>{affiliation}</size></color>";
        }

        /// <summary>
        /// ������ı�
        /// </summary>
        /// <param name="text"></param>
        public void PrintMainText(string text)
        {
            EventBus<OnPrintingLine>.Raise();

            _txtMain.text = null;
            _objContinued.SetActive(false);
            SetActiveTextArea();
            SetActiveButton();

            _isPrinting = true;
            _mainTextBuffer = text;
            if (_crtPrint != null)
                StopCoroutine(_crtPrint);
            _crtPrint = StartCoroutine(CPrint());
        }
        IEnumerator CPrint()
        {
            for(int i = 0; i < _mainTextBuffer.Length; i++)
            {
                _txtMain.text += _mainTextBuffer[i];
                yield return new WaitForSeconds(BAStoryPlayerController.Instance.Setting.IntervalPrint);
            }
            _crtPrint = null;
            _isPrinting = false;
            _mainTextBuffer = null;

            EventBus<OnPrintedLine>.Raise();
        }

        /// <summary>
        /// �����ı�
        /// </summary>
        public void Skip()
        {
            if (!_isPrinting)
                return;

            StopCoroutine(_crtPrint);
            _txtMain.text = _mainTextBuffer;
            _mainTextBuffer = null;
            _isPrinting = false;

            EventBus<OnPrintedLine>.Raise();
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

        public void SetBlurBackground(bool enable,BackgroundTransistionType transition = BackgroundTransistionType.Smooth)
        {
            if(transition == BackgroundTransistionType.Smooth)
                _imgBackground.DoFloat("_Weight", enable ? 1 : 0, BAStoryPlayerController.Instance.Setting.TimeBlurBackground);
            else if(transition == BackgroundTransistionType.Instant)
            {
                Material mat = new Material(_imgBackground.material);
                _imgBackground.material = mat;
                mat.SetFloat("_Weight", enable ? 1 : 0);
            }
                
        }

        /// <summary>
        /// ��ʾ���Ⲣ�ر�����UI
        /// </summary>
        public void ShowTitle(string subtitle,string title)
        {
            GameObject obj = Instantiate(Resources.Load("UI/Title") as GameObject);
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

        public void SetSynopsis(string synopsis = "")
        {
            _txtSynopsis.text = synopsis;
        }
        public void SetTitle(string title = "") 
        {
            _txtTitle.text = title;
        }
    }
}

