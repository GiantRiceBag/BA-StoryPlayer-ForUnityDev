using UnityEngine;
namespace BAStoryPlayer
{
    [CreateAssetMenu(menuName = "BAStoryPlayer/���������ñ�", fileName = "PlayerSetting")]
    public class PlayerSetting : ScriptableObject
    {
        [Header("����������")]
        [Tooltip("ÿ���ı��������")]
        [SerializeField] private int num_Char_Persecond = 20;
        [Tooltip("���ı�������ʱ ��������ʱ��")]
        [SerializeField] private float time_Lock_AfterPrinting = 0.2f;
        [Tooltip("����ɫ����ִ�����ʱ ������������ʱ��")]
        [SerializeField] private float time_Lock_AfterAction = 0.2f;
        [Space]
        [Header("��������")]
        [Tooltip("����ģ��ʱ��")]
        [SerializeField] private float time_BlurBackground = 0.7f;
        [Tooltip("�����л�����ʱ��")]
        [SerializeField] private float time_SwitchBackground = 1f;
        [Space]
        [Header("��ɫ��������")]
        [Tooltip("��ɫ���뽥��ʱ��")]
        [SerializeField] private float time_Character_Fade = 0.75f;
        [Tooltip("������ɫʱ��")]
        [SerializeField] private float time_Character_Highlight = 0.2f;
        [Tooltip("��ɫգ��ʱ������Χ(��Χ�����ֵ)")]
        [SerializeField] private Vector2 time_Character_Wink = new Vector2(4, 5.5f);
        [Tooltip("��ɫ���� Hophop/��Ծ���� ����ʱ��")]
        [SerializeField] private float time_Character_Hophop = 0.5f;
        [Tooltip("��ɫ���� Shake/ҡ�� ����ʱ��")]
        [SerializeField] private float time_Character_Shake = 0.64f;
        [Tooltip("��ɫ���� Move/λ�ñ任 ����ʱ��")]
        [SerializeField] private float time_Character_Move = 0.45f;
        [Tooltip("��ɫ���� Stiff/С���Ⱦ���ҡ�� ����ʱ��")]
        [SerializeField] private float time_Character_Stiff = 0.45f;
        [Tooltip("��ɫ���� Jump/��Ծ ����ʱ��")]
        [SerializeField] private float time_Character_Jump = 0.3f;
        [Tooltip("��ɫ���� Greeting/�����ƶ������� ����ʱ��")]
        [SerializeField] private float time_Character_Greeting = 0.8f;
        [Space]
        [Header("������Ч����")]
        [Tooltip("��ЧSource���������")]
        [SerializeField] private int num_Max_AudioSource = 10;
        [Tooltip("���������л����뵭����ʱ(���뵭���ܺ�ʱΪ��ֵ����)")]
        [SerializeField] private float time_Bgm_Fade = 0.5f;
        [Space]
        [Header("��Դ·������")]
        [Tooltip("������Դ·��")]
        [SerializeField] private string path_Music = "Music/";
        [Tooltip("��Ч��Դ·��")]
        [SerializeField] private string path_Sound = "Sound/";
        [Tooltip("����ͼƬ·��")]
        [SerializeField] private string path_Background = "Background/";
        [Tooltip("��ɫSkel�Լ�Prefab·��")]
        [SerializeField] private string path_Skel = "CharacterSkel/";
        [Tooltip("����ű�·��")]
        [SerializeField] private string path_StoryScript = "StoryScript/";

        public int Num_Char_Persecond { get { return num_Char_Persecond; } }
        public float Interval_Print { get { return 1 / (float) num_Char_Persecond; } }
        public float Time_Lock_AfterPrinting { get { return time_Lock_AfterPrinting; } }
        public float Time_Lock_AfterAction { get { return time_Lock_AfterAction; } }

        public float Time_BlurBackground { get { return time_BlurBackground; } }
        public float Time_SwitchBackground { get { return time_SwitchBackground; } }
        
        public float Time_Character_Fade { get { return time_Character_Fade; } }
        public float Time_Character_Highlight { get { return time_Character_Highlight; } }
        public Vector2 Time_Character_Wink { get { return time_Character_Wink; } }
        public float Time_Character_Hophop { get { return time_Character_Hophop; } }
        public float Time_Character_Shake { get { return time_Character_Shake; } }
        public float Time_Character_Move { get { return time_Character_Move; } }
        public float Time_Character_Stiff { get { return time_Character_Stiff; } }
        public float Time_Character_Jump { get { return time_Character_Jump; } }
        public float Time_Character_Greeting { get { return time_Character_Greeting; } }

        public int Num_Max_AudioSource { get {return  num_Max_AudioSource; } }
        public float Time_Bgm_Fade { get { return time_Bgm_Fade; } }

        public string Path_Music { get { return path_Music; } }
        public string Path_Sound { get { return path_Sound; } }
        public string Path_Background { get { return path_Background; } }
        public string Path_Prefab { get { return path_Skel; } }
        public string Path_StoryScript { get { return path_StoryScript; } }
    }
}
