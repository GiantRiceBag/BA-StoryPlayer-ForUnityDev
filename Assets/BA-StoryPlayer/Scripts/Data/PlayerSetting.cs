using System.IO;
using UnityEngine;
namespace BAStoryPlayer
{
    [CreateAssetMenu(menuName = "BAStoryPlayer/����������", fileName = "PlayerSetting")]
    public class PlayerSetting : ScriptableObject
    {
        [Header("����������")]
        [Tooltip("ÿ���ı��������")]
        [SerializeField] private int _numCharPerSecond = 20;
        [Tooltip("���ı�������ʱ ��������ʱ��")]
        [SerializeField] private float _timeLockAfterPrinting = 0.2f;
        [Tooltip("����ɫ����ִ�����ʱ ������������ʱ��")]
        [SerializeField] private float _timeLockAfterAction = 0.2f;
        [Space]
        [Header("��������")]
        [Tooltip("����ģ��ʱ��")]
        [SerializeField] private float _timeBlurBackground = 0.7f;
        [Tooltip("�����л�����ʱ��")]
        [SerializeField] private float _timeSwitchBackground = 1f;
        [Space]
        [Header("��ɫ��������")]
        [Tooltip("��ɫ���뽥��ʱ��")]
        [SerializeField] private float _timeCharacterFade = 0.75f;
        [Tooltip("������ɫʱ��")]
        [SerializeField] private float _timeCharacterHighlight = 0.2f;
        [Tooltip("��ɫգ��ʱ������Χ(��Χ�����ֵ)")]
        [SerializeField] private Vector2 _timeRangeCharacterWink = new Vector2(4, 5.5f);
        [Tooltip("��ɫ���� Hophop/��Ծ���� ����ʱ��")]
        [SerializeField] private float _timeCharacterHophop = 0.5f;
        [Tooltip("��ɫ���� Shake/ҡ�� ����ʱ��")]
        [SerializeField] private float _timeCharacterShake = 0.64f;
        [Tooltip("��ɫ���� Move/λ�ñ任 ����ʱ��")]
        [SerializeField] private float _timeCharacterMove = 0.45f;
        [Tooltip("��ɫ���� Stiff/С���Ⱦ���ҡ�� ����ʱ��")]
        [SerializeField] private float _timeCharacterStiff = 0.45f;
        [Tooltip("��ɫ���� Jump/��Ծ ����ʱ��")]
        [SerializeField] private float _timeCharacterJump = 0.3f;
        [Tooltip("��ɫ���� Greeting/�����ƶ������� ����ʱ��")]
        [SerializeField] private float _timeCharacterGreeting = 0.8f;
        [Space]
        [Header("������Ч����")]
        [Tooltip("��ЧSource���������")]
        [SerializeField] private int _numMaxAudioSource = 10;
        [Tooltip("���������л����뵭����ʱ(���뵭���ܺ�ʱΪ��ֵ����)")]
        [SerializeField] private float _timeBgmFade = 0.5f;
        [Space]
        [Header("��Դ·������")]
        [Tooltip("������Դ·��"), HideInInspector]
        [SerializeField] private string _pathMusic = "Music/";
        [Tooltip("��Ч��Դ·��"), HideInInspector]
        [SerializeField] private string _pathSound = "Sound/";
        [Tooltip("����ͼƬ·��"), HideInInspector]
        [SerializeField] private string _pathBackground = "Background/";
        [Tooltip("��ɫSkel�Լ�Prefab·��"), HideInInspector]
        [SerializeField] private string _pathCharacterSkeletonData = "CharacterSkeletonData/";
        [Tooltip("����ű�·��"), HideInInspector]
        [SerializeField] private string _pathStoryScript = "StoryScript/";

        public int NumCharPerSecond => _numCharPerSecond;
        public float IntervalPrint => 1 / (float)_numCharPerSecond;
        public float TimeLockAfterPrinting => _timeLockAfterPrinting;
        public float TimeLockAfterAction => _timeLockAfterAction;

        public float TimeBlurBackground => _timeBlurBackground;
        public float TimeSwitchBackground => _timeSwitchBackground;

        public float TimeCharacterFade => _timeCharacterFade;
        public float TimeCharacterHighlight => _timeCharacterHighlight;
        public Vector2 TimeRangeCharacterWink => _timeRangeCharacterWink;
        public float TimeCharacterHophop => _timeCharacterHophop;
        public float TimeCharacterShake => _timeCharacterShake;
        public float TimeCharacterMove => _timeCharacterMove;
        public float TimeCharacterStiff => _timeCharacterStiff;
        public float TimeCharacterJump => _timeCharacterJump;
        public float TimeCharacterGreeting => _timeCharacterGreeting;

        public int NumMaxAudioSource => _numMaxAudioSource;
        public float TimeBgmFade => _timeBgmFade;

        public string PathMusic => _pathMusic;
        public string PathSound => _pathSound;
        public string PathBackground => _pathBackground;
        public string PathCharacterSkeletonData => _pathCharacterSkeletonData;
        public string PathStoryScript => _pathStoryScript;
    }
}
