using UnityEngine;
using BAStoryPlayer.NexonScriptParser;
using BAStoryPlayer.UniversaScriptParser;
using BAStoryPlayer.AsScriptParser;
using BAStoryPlayer.Event;

namespace BAStoryPlayer
{
    public enum StoryScriptType
    {
        Universal,
        Nexon,
        As
    }

    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        private const string Path_Setting = "Setting/";
        private const string Path_StoryPlayer = "StoryPlayer";

       [Header("References")]
       [SerializeField] private CharacterData characterDataTable;
       [SerializeField] private PlayerSetting playerSetting;
       [SerializeField] private BAStoryPlayer storyPlayer;

        private bool isPlaying = false;

        public bool IsPlaying => isPlaying;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (storyPlayer == null)
                {
                    storyPlayer = FindObjectOfType<BAStoryPlayer>();

                    if (storyPlayer == null)
                    {
                        storyPlayer = Instantiate(Resources.Load(Path_StoryPlayer) as GameObject).GetComponent<BAStoryPlayer>();
                        storyPlayer.transform.SetParent(transform);
                        storyPlayer.name = "StoryPlayer";
                        storyPlayer.transform.localPosition = Vector3.zero;
                        storyPlayer.transform.localScale = Vector3.one;
                    }
                }

                return storyPlayer;
            }
        }
        public CharacterData CharacterDataTable
        {
            get
            {
                if (characterDataTable == null)
                {
                    // TODO �պ������Ϊȫ���ѧ�����ݱ� --> ���� �����P ����ô���� �������
                    characterDataTable = Resources.Load<CharacterData>(Path_Setting + "CharacterDataTable");
                    Debug.LogWarning($"δ���ý�ɫ��Ϣ��!");
                }

                return characterDataTable;
            }
        }
        public PlayerSetting Setting
        {
            get
            {
                if (playerSetting == null)
                {
                    if (Application.isEditor)
                        return ScriptableObject.CreateInstance<PlayerSetting>();
                    Debug.LogWarning($"δ���ò������趨�� ��ʹ��Ĭ�ϱ�");
                    playerSetting = ScriptableObject.CreateInstance<PlayerSetting>();
                }

                return playerSetting;
            }
        }

        public BAStoryPlayer LoadStory(string url,StoryScriptType type = StoryScriptType.Universal)
        {
            if (isPlaying)
            {
                Debug.Log("���鲥����");
                return null;
            }

            // TODO ��ɾ��������ѡ��
            StoryPlayer.gameObject.SetActive(true);

            
            var textAsset = Resources.Load<TextAsset>(Setting.Path_StoryScript + url);
            if(textAsset == null)
            {
                Debug.LogError($"δ���� {Setting.Path_StoryScript + url } �ҵ�����ű�");
                return null;
            }

            ICommandParser parser = null;
            switch (type)
            {
                case StoryScriptType.Universal:
                    parser = new UniversalCommandParser();
                    break;
                case StoryScriptType.Nexon:
                    parser = new NexonCommandParser();
                    break;
                case StoryScriptType.As:
                    parser = new AsCommandParser();
                    break;
            }

            StoryPlayer.LoadUnits(0, parser.Parse(textAsset));
            StoryPlayer.ReadyToNext();
            StoryPlayer.Next();

            isPlaying = true;
            StoryPlayer.gameObject.SetActive(true);

            EventBus<OnClosedStoryPlayer>.ClearCallback();

            // ���Ĳ��Ž����¼�
            EventBus<OnClosedStoryPlayer>.AddCallback(() =>
            {
                isPlaying = false;
            });

            return StoryPlayer;
        }
    }

    public static class ExtensionMethod
    {
        public static void ClearAllChild(this Transform transform)
        {
            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
