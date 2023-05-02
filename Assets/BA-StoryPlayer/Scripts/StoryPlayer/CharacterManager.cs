using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Spine.Unity;
using BAStoryPlayer.DoTweenS;
namespace BAStoryPlayer
{
    public enum TransistionType
    {
        Instant = 0,
        Smooth
    }
    public enum CharacterAction
    {
        Appear = 0,
        Disapper,
        Disapper2Left,
        Disapper2Right,
        AppearL2R,
        AppearR2L,
        Hophop,
        Greeting,
        Shake,
        Move,
        Stiff,
        Closeup,
        Jump,
        falldownR,
        Hide
    }



    public class CharacterManager : MonoBehaviour
    {
        const int NUM_CHARACTER_SLOT = 5;
        const float VALUE_CHARACTER_SCALE = 0.7f;
        const int VALUE_INTERVAL_SLOT = 320;
        // TODO ������ز��� ����ͳһ�ŵ����ñ���
        const float TIME_TRANSITION = 0.75f;
        const float TIME_HIGHLIGHT = 0.2f;
        const float TIME_HOPHOP = 0.5f;
        const float TIME_SHAKE = 0.64f;
        const float TIME_MOVE = 0.45f;
        const float TIME_STIFF = 0.45f;
        const float TIME_JUMP = 0.3f;
        const float TIME_GREETING = 0.8f;

        Vector2 INTERVAL_WINK
        {
            get
            {
                return new Vector2(4, 5.5f);
            }
        }
        Color COLOR_UNHIGHLIGHT
        {
            get
            {
                return new Color(0.6f, 0.6f, 0.6f);
            }
        }

        [SerializeField] SkeletonGraphic[] character = new SkeletonGraphic[NUM_CHARACTER_SLOT];
        Dictionary<string, GameObject> characterPool = new Dictionary<string, GameObject>();
        Dictionary<string, Coroutine> winkAction = new Dictionary<string, Coroutine>();

        public SkeletonGraphic[] Character
        {
            get
            {
                return character;
            }
        }

        BAStoryPlayer StoryPlayer
        {
            get
            {
                return BAStoryPlayerController.Instance.StoryPlayer;
            }
        }

        /// <summary>
        /// �����ɫ
        /// </summary>
        /// <param name="index">��ʼλ��/��ɫ���</param>
        /// <param name="name">��ɫ����</param>
        /// <param name="animationID">���Ŷ����ı��</param>
        /// <param name="transistion">������ʽ[���״γ�����Ч]</param>
        public void ActivateCharacter(int index,string name,string animationID,TransistionType transistion = TransistionType.Instant)
        {
            int currentIndex = CheckCharacterExist(name);
            // ��ɫ���ڳ���
            if (currentIndex == -1)
            {
                GameObject obj = null;
                characterPool.TryGetValue(name,out obj);

                // ������в����������벢��ʼ���������
                if(obj == null)
                {
                    obj = BAStoryPlayerController.Instance.LoadCharacter(name);
                    if (obj == null)
                        return;

                    obj.transform.SetParent(transform);
                    obj.name = name;
                    var rectTransform = obj.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    rectTransform.anchoredPosition = new Vector3((index + 1) * VALUE_INTERVAL_SLOT,0,0);
                    rectTransform.localScale = new Vector3(VALUE_CHARACTER_SCALE, VALUE_CHARACTER_SCALE, 1);
                    characterPool.Add(name, obj);
                }
                // TODO ����Ҫ���볡�������ͻ
                // ����ش��ڶ�Ӧ��ɫ��ֻ��Ҫ�޸�λ�ü���
                else
                {
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector3((index + 1) * VALUE_INTERVAL_SLOT, 0, 0);
                }

                // ��Ӧ��λ��������ɫ��ɾ��
                DestroyCharacter(index);
                character[index] = obj.GetComponent<SkeletonGraphic>();
                // �״γ�����ʽ
                switch (transistion)
                {
                    case TransistionType.Instant:
                        {
                            obj.SetActive(true);
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            character[index].color = Color.black;
                            character[index].DoColor(Color.white, TIME_TRANSITION);
                            break;
                        }
                    default:break;
                }

                Highlight(index);

                character[index].AnimationState.SetAnimation(0, animationID, false);
                if (animationID == "00")
                    SetWinkAction(name, true);
                character[index].AnimationState.SetAnimation(1, "Idle_01", true);
            }
            // ��ɫ�ڳ���
            else
            {
                // λ�ø��� ��Ŀ��λ����������ɫ��ֱ��ɾ��
                if(currentIndex != index)
                {
                    DestroyCharacter(index);

                    MoveCharacterTo(currentIndex, index);
                    character[index] = character[currentIndex];
                    character[currentIndex] = null;
                }

                // �����滻
                character[index].AnimationState.SetAnimation(0, animationID, true);
                SetWinkAction(name, animationID == "00" ? true : false);

                Highlight(index);
            }

            // TODO
            // ���Ž�ɫ̨�� ����Ҳ�ò���
        }
        /// <summary>
        /// ����ɫ�Ƿ���ڲ����ض�Ӧ�±� ���������򷵻�-1
        /// </summary>
        /// <param name="name">��ɫ��</param>
        /// <returns></returns>
        int CheckCharacterExist(string name)
        {
            for(int i = 0; i < NUM_CHARACTER_SLOT; i++)
            {
                if (Character[i] == null)
                    continue;
                if (Character[i].gameObject.name == name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// ����ɫ��λ�Ƿ�Ϊ��
        /// </summary>
        /// <param name="index">�±�</param>
        /// <returns></returns>
        bool CheckSlotEmpty(int index)
        {
            if (!ValidateIndex(index))
                return false;

            return (Character[index] == null);
        }

        /// <summary>
        /// ����ɾ���ڳ��ϵĽ�ɫ(��ɾ�������ȡ�����)
        /// </summary>
        /// <param name="index">�±�</param>
        /// <param name="destoryObject">��ȫɾ����ɫ</param>
        void DestroyCharacter(int index,bool destoryObject = false)
        {
            if (CheckSlotEmpty(index))
                return;

            if (destoryObject)
            {
                characterPool.Remove(character[index].name);
                Destroy(character[index].gameObject);
            }
            else
            {
                character[index].gameObject.SetActive(false);
            }

            SetWinkAction(character[index].name, false);
            character[index] = null;
           

        }

        /// <summary>
        /// ���ÿ�����ɫգ�۶���
        /// </summary>
        /// <param name="name">��ɫ��</param>
        /// <param name="enable">����գ��</param>
        void SetWinkAction(string name,bool enable)
        {
            Coroutine coroutine = null;
            winkAction.TryGetValue(name, out coroutine);

            if (enable)
            {
                if(coroutine == null)
                {
                    coroutine = StartCoroutine(CCharacterWink(name));
                    winkAction.Add(name, coroutine);
                }
            }
            else
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    winkAction.Remove(name);
                }
            }
        }
        IEnumerator CCharacterWink(string name)
        {
            int index = CheckCharacterExist(name);
            if (index == -1)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(INTERVAL_WINK.x, INTERVAL_WINK.y));
                while (true)
                {
                    character[index].AnimationState.SetAnimation(0, "Eye_Close_01", false);
                    yield return new WaitForSeconds(Random.Range(INTERVAL_WINK.x, INTERVAL_WINK.y));
                }
            }
        }

        /// <summary>
        /// �����ɫλ�õ����ı�����
        /// </summary>
        /// <param name="currentIndex">��ɫ���</param>
        /// <param name="targetIndex">Ŀ��λ�ñ��</param>
        /// <param name="transition">���ɷ�ʽ</param>
        void MoveCharacterTo(int currentIndex,int targetIndex,TransistionType transition = TransistionType.Instant)
        {
            if (!ValidateIndex(currentIndex) || !ValidateIndex(targetIndex))
                return;

            switch (transition)
            {
                case TransistionType.Instant:
                    {
                        character[currentIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2((targetIndex+1) * VALUE_INTERVAL_SLOT, 0);
                        break;
                    }
                case TransistionType.Smooth:
                    {
                        character[currentIndex].transform.DoMove_Anchored(new Vector2((targetIndex + 1) * VALUE_INTERVAL_SLOT, 0), TIME_MOVE);
                        break;
                    }
            default:break;
            }
        }
        void MoveCharacterTo(string name, int index, TransistionType transition = TransistionType.Instant)
        {
            int currentIndex = CheckCharacterExist(name);
            if (currentIndex == -1)
                return;
            MoveCharacterTo(currentIndex, index, transition);
        }
        void MoveCharacterTo(int currentIndex,Vector2 pos,TransistionType transition = TransistionType.Instant)
        {
            if (!ValidateIndex(currentIndex))
                return;

            switch (transition)
            {
                case TransistionType.Instant:
                    {
                        character[currentIndex].GetComponent<RectTransform>().anchoredPosition = pos;
                        break;
                    }
                case TransistionType.Smooth:
                    {
                        character[currentIndex].transform.DoMove_Anchored(pos, TIME_MOVE);
                        break;
                    }
                default: break;
            }
        }
        void MoveCharacterTo(string name, Vector2 pos, TransistionType transition = TransistionType.Instant)
        {
            int currentIndex = CheckCharacterExist(name);
            if (currentIndex == -1)
                return;
            MoveCharacterTo(currentIndex, pos, transition);
        }

        bool ValidateIndex(int index)
        {
            if (index >= 0 && index < NUM_CHARACTER_SLOT)
                return true;
            Debug.LogError($"��ɫ��λ�±� {index} ������Χ");
            return false;
        }

        /// <summary>
        /// ʹ��ɫִ��ĳ�ֶ���
        /// </summary>
        /// <param name="index">���</param>
        /// <param name="action">����</param>
        /// <param name="arg">����(��ѡ)</param>
        public void SetAction(int index,CharacterAction action,int arg = -1)
        {
            if (CheckSlotEmpty(index))
                return;

            switch (action)
            {
                case CharacterAction.Appear:
                    {
                        character[index].DoColor(Color.white, TIME_TRANSITION);
                        break;
                    }
                case CharacterAction.Disapper:
                    {
                        character[index].DoColor(Color.black, TIME_TRANSITION);
                        break;
                    }
                case CharacterAction.Disapper2Left:
                    {
                        MoveCharacterTo(index, new Vector2(-500, 0), TransistionType.Smooth);
                        break;
                    }
                case CharacterAction.Disapper2Right:
                    {
                        MoveCharacterTo(index, new Vector2(2420, 0), TransistionType.Smooth);
                        break;
                    }
                case CharacterAction.AppearL2R:
                    {
                        character[index].color = Color.white;
                        MoveCharacterTo(index, new Vector2(-500, 0));
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        break;
                    }
                case CharacterAction.AppearR2L:
                    {
                        character[index].color = Color.white;
                        MoveCharacterTo(index, new Vector2(2420, 0));
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        break;
                    }
                case CharacterAction.Hophop:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, 50), TIME_HOPHOP, 2);
                        break;
                    }
                case CharacterAction.Greeting:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, -70), TIME_GREETING);
                        break;
                    }
                case CharacterAction.Shake:
                    {
                        character[index].transform.DoShakeX(40, TIME_SHAKE, 2);
                        break;
                    }
                case CharacterAction.Move:
                    {
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        break;
                    }
                case CharacterAction.Stiff:
                    {
                        character[index].transform.DoShakeX(10, TIME_STIFF, 4);
                        break;
                    }
                case CharacterAction.Closeup:
                    {
                        // TODO
                        Debug.Log($"{action}û��");
                        break;
                    }
                case CharacterAction.Jump:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, 70), TIME_JUMP);
                        break;
                    }
                case CharacterAction.falldownR:
                    {
                        // TODO
                        Debug.Log($"{action}û��");
                        break;
                    }
                case CharacterAction.Hide:
                    {
                        character[index].DoColor(Color.black, TIME_TRANSITION).onComplete = () => { DestroyCharacter(index); };
                    break;
                }
                default:return;
            }
        }

        /// <summary>
        /// ���ý�ɫ����emoji
        /// </summary>
        /// <param name="index">�±�</param>
        /// <param name="emotion">����</param>
        public void SetEmotion(int index,CharacterEmotion emotion)
        {
            if (CheckSlotEmpty(index))
                return;

            EmotionFactory.SetEmotion(character[index].transform, emotion);
        }

        /// <summary>
        /// ����ĳ����ɫ
        /// </summary>
        /// <param name="index">��ɫ�±�</param>
        /// <param name="transition">���ɷ�ʽ</param>
        public void Highlight(int index,TransistionType transition = TransistionType.Instant)
        {
            if (CheckSlotEmpty(index))
                return;

            character[index].color = Color.white;
            foreach(var i in character)
            {
                if (i == character[index] || i == null)
                    continue;
                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            i.color = COLOR_UNHIGHLIGHT;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            i.DoColor(COLOR_UNHIGHLIGHT, TIME_HIGHLIGHT);
                            break;
                        }
                    default: return;
                }
            }
        }
        /// <summary>
        /// �����Ƿ�������н�ɫ
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="transition"></param>
        public void SetHighlightAll(bool enable,TransistionType transition = TransistionType.Instant)
        {
            foreach (var i in character)
            {
                if (i == null)
                    continue;

                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            i.color = COLOR_UNHIGHLIGHT;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            i.DoColor(COLOR_UNHIGHLIGHT, TIME_HIGHLIGHT);
                            break;
                        }
                    default: return;
                }
            }
        }

        // TODO TEST
        string[] testAni = { "00", "01", "02","03","04" };
        int index = 0;
        int index2 = 0;
        public void TEST()
        {
            ActivateCharacter(index, "hoshino", testAni[index],TransistionType.Smooth);
            int next = Random.Range(0, 5);
            while(index == next)
            {
                next = Random.Range(0, 5);
            }
            index = next;
        }
        public void TEST2()
        {
            DestroyCharacter(2,true);
            ActivateCharacter(2, "shiroko", testAni[0],TransistionType.Smooth);
        }
        public void TestTween()
        {
            character[2].DoColor(Color.black, 0.5f).onComplete = () => { DestroyCharacter(2, true); };
        }
        public void TestAction()
        {
            SetAction(2, (CharacterAction)index, 2);
            index++;
            index %= 14;
        }
        public void TestActionWithArg(int index)
        {
            SetAction(2, (CharacterAction)index, 2);
        }
        public void TestEmotion()
        {
            //SetEmotion(0, (CharacterEmotion)index);
            SetEmotion(2, (CharacterEmotion)index);
            SetAction(2, (CharacterAction)index, 2);
            //SetEmotion(4, (CharacterEmotion)index++);
            index++;
            index %= 13;
        }
    }
}

