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
        [Header("����ʱ������")]
        [Tooltip("����ģ��ʱ��")]
        [SerializeField] private float _timeBlurBackground = 0.7f;
        [Tooltip("�����л�����ʱ��")]
        [SerializeField] private float _timeSwitchBackground = 1f;
        [Space]
        [Header("��ɫ��������")]
        [Tooltip("��ɫ���뽥��ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterFade = 0.75f;
        [Tooltip("������ɫʱ��")]
        [SerializeField] private float _defaultTimeCharacterHighlight = 0.2f;
        [Tooltip("��ɫգ��ʱ������Χ(��Χ�����ֵ)")]
        [SerializeField] private Vector2 _defaultTimeRangeCharacterWink = new Vector2(4, 5.5f);
        [Tooltip("��ɫ���� Hophop/��Ծ���� ����ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterHophop = 0.5f;
        [Tooltip("��ɫ���� Shake/ҡ�� ����ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterShake = 0.64f;
        [Tooltip("��ɫ���� Move/λ�ñ任 ����ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterMove = 0.45f;
        [Tooltip("��ɫ���� Stiff/С���Ⱦ���ҡ�� ����ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterStiff = 0.45f;
        [Tooltip("��ɫ���� Jump/��Ծ ����ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterJump = 0.3f;
        [Tooltip("��ɫ���� Greeting/�����ƶ������� ����ʱ��"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterGreeting = 0.8f;
        [Tooltip("��ɫ���� Falldown/����ҡ�ں�ˤ�� ����ʱ��"), HideInInspector]
        [SerializeField] private float _timeFalldown = 1.6f;
        [Space]
        [Header("������Ч����")]
        [Tooltip("��ЧSource���������")]
        [SerializeField] private int _numMaxAudioSource = 10;
        [Tooltip("���������л����뵭����ʱ(���뵭���ܺ�ʱΪ��ֵ����)")]
        [SerializeField] private float _timeBgmFade = 0.5f;
        [Space]
        [Header("��Դ·������")]
        [Tooltip("������Դ·��"), HideInInspector]
        [SerializeField] private string _pathMusic = "Music";
        [Tooltip("��Ч��Դ·��"), HideInInspector]
        [SerializeField] private string _pathSound = "Sound";
        [Tooltip("����ͼƬ·��"), HideInInspector]
        [SerializeField] private string _pathBackground = "Background";
        [Tooltip("��ɫSkel�Լ�Prefab·��"), HideInInspector]
        [SerializeField] private string _pathCharacterSkeletonData = "CharacterSkeletonData";
        [Tooltip("����ű�·��"), HideInInspector]
        [SerializeField] private string _pathStoryScript = "StoryScript";

        public int NumCharPerSecond => _numCharPerSecond;
        public float IntervalPrint => 1 / (float)_numCharPerSecond;
        public float TimeLockAfterPrinting => _timeLockAfterPrinting;
        public float TimeLockAfterAction => _timeLockAfterAction;

        public float TimeBlurBackground => _timeBlurBackground;
        public float TimeSwitchBackground => _timeSwitchBackground;

        public float DefaultTimeCharacterFade => _defaultTimeCharacterFade;
        public float DefaultTimeCharacterHighlight => _defaultTimeCharacterHighlight;
        public Vector2 DefaultTimeRangeCharacterWink => _defaultTimeRangeCharacterWink;
        public float DefaultTimeCharacterHophop => _defaultTimeCharacterHophop;
        public float DefaultTimeCharacterShake => _defaultTimeCharacterShake;
        public float DefaultTimeCharacterMove => _defaultTimeCharacterMove;
        public float DefaultTimeCharacterStiff => _defaultTimeCharacterStiff;
        public float DefaultTimeCharacterJump => _defaultTimeCharacterJump;
        public float DefaultTimeCharacterGreeting => _defaultTimeCharacterGreeting;
        public float DefaultTimeFalldown => _timeFalldown;

        public int NumMaxAudioSource => _numMaxAudioSource;
        public float TimeBgmFade => _timeBgmFade;

        public string PathMusic(string file) => Path.Combine(_pathMusic,file);
        public string PathSound(string file) => Path.Combine(_pathSound, file);
        public string PathBackground(string file) => Path.Combine(_pathBackground, file);
        public string PathCharacterSkeletonData(string file) => Path.Combine(_pathCharacterSkeletonData, file);
        public string PathStoryScript(string file) => Path.Combine(_pathStoryScript, file);
    }
}
