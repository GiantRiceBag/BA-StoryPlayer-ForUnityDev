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

        Color COLOR_UNHIGHLIGHT { get { return new Color(0.6f, 0.6f, 0.6f);  } }

        [SerializeField] SkeletonGraphic[] character = new SkeletonGraphic[NUM_CHARACTER_SLOT];
        Dictionary<string, GameObject> characterPool = new Dictionary<string, GameObject>();
        Dictionary<string, Coroutine> winkAction = new Dictionary<string, Coroutine>();

        public SkeletonGraphic[] Character { get { return character; } }
        BAStoryPlayer StoryPlayer { get { return BAStoryPlayerController.Instance.StoryPlayer; } }

        [HideInInspector] public UnityEngine.Events.UnityEvent<float> OnAnimateCharacter;

        /// <summary>
        /// �����ɫ(���ж���,һ��Ҫ���ڶ���ǰ����)
        /// </summary>
        /// <param name="index">��ʼλ��/��ɫ���</param>
        /// <param name="name">��ɫ����(������)</param>
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
                    {
                        Debug.LogError($"�޷��� Resoucres�ļ����е�·�� {BAStoryPlayerController.Instance.Setting.Path_Prefab} �ҵ���Ӧ��ɫԤ����");
                        return;
                    }

                    obj.transform.SetParent(transform);
                    obj.name = name;
                    var rectTransform = obj.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.zero;
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    rectTransform.anchoredPosition = new Vector3((index + 1) * VALUE_INTERVAL_SLOT,0,0);
                    rectTransform.localScale = new Vector3(VALUE_CHARACTER_SCALE, VALUE_CHARACTER_SCALE, 1);

                    // TODO ���鶨λ����ʱʹ��
                    //Transform locator = obj.transform.Find("HeadLocator");
                    //if (locator == null) locator = obj.transform.GetChild(0);
                    //RectTransform locatorRect = locator.GetComponent<RectTransform>();
                    //Vector2 origin = locatorRect.position;

                    obj.GetComponent<SkeletonGraphic>().MatchRectTransformWithBounds();

                    // TODO ���鶨λ����ʱʹ��
                    //locatorRect.position = origin;

                    characterPool.Add(name, obj);
                }
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
                            character[index].DoColor(Color.white, BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                            break;
                        }
                    default:break;
                }

                Highlight(index);



                // Ĭ�� ����01 ����գ�۶��� ����Contains����Ϊ��Щ������ǰ��׺��
                if (animationID.Contains("01") && BAStoryPlayerController.Instance.CharacterDataTable[name].spineEmotion)
                    SetWinkAction(name, true);

                // ������ɫ�Ƿ����ò��
                if (BAStoryPlayerController.Instance.CharacterDataTable[name].spineEmotion)
                {
                    character[index].AnimationState.SetAnimation(0, animationID, false); // ��ֶ���
                    character[index].AnimationState.SetAnimation(1, "Idle_01", true); // �������
                }

            }
            // ��ɫ�ڳ���
            else
            {
                // λ�ø��� ��Ŀ��λ����������ɫ��ֱ��ɾ��
                if(currentIndex != index)
                {
                    // ��ͣ����ɫ��գ�۶����ڱ任λ��
                    SetWinkAction(name, false);

                    DestroyCharacter(index);

                    MoveCharacterTo(currentIndex, index);
                    character[index] = character[currentIndex];
                    character[currentIndex] = null;
                }

                // �����滻�������ò�֣�
                if (BAStoryPlayerController.Instance.CharacterDataTable[name].spineEmotion)
                {
                    character[index].AnimationState.SetAnimation(0, animationID, true);
                    SetWinkAction(name, animationID.Contains("01") ? true : false);
                }

                Highlight(index);
            }
        }
        public void ActivateCharacter(int index, string name, string animationID, string lines,TransistionType transistion = TransistionType.Instant)
        {
            ActivateCharacter(index, name, animationID, transistion);
            StoryPlayer.UIModule.SetSpeaker(name);
            StoryPlayer.UIModule.PrintText(lines);
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
            // ���û���������
            if (!BAStoryPlayerController.Instance.CharacterDataTable[name].spineEmotion)
                return;

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

            string ani_EyeClose_Name = "Eye_Close_01";
            // Ѱ��գ�۶��������� һ����e��ͷ
            foreach (var i in character[index].SkeletonData.Animations)
            {
                if(i.Name[0] == 'E' || i.Name[0] == 'e')
                {
                    ani_EyeClose_Name = i.Name;
                    break;
                }
            }


            if (index == -1)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(BAStoryPlayerController.Instance.Setting.Time_Character_Wink.x, BAStoryPlayerController.Instance.Setting.Time_Character_Wink.y));
                while (true)
                {
                    character[index].AnimationState.SetAnimation(0,ani_EyeClose_Name, false);
                    yield return new WaitForSeconds(Random.Range(BAStoryPlayerController.Instance.Setting.Time_Character_Wink.x, BAStoryPlayerController.Instance.Setting.Time_Character_Wink.y));
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
                        character[currentIndex].transform.DoMove_Anchored(new Vector2((targetIndex + 1) * VALUE_INTERVAL_SLOT, 0), BAStoryPlayerController.Instance.Setting.Time_Character_Move);
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
                        character[currentIndex].transform.DoMove_Anchored(pos, BAStoryPlayerController.Instance.Setting.Time_Character_Move);
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
            Debug.LogError($"��ɫ��λ�±� {index} ������Χ[0-4]");
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
                        character[index].color = Color.black;
                        character[index].DoColor(Color.white, BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        break;
                    }
                case CharacterAction.Disapper:
                    {
                        character[index].color = Color.white;
                        character[index].DoColor(Color.black, BAStoryPlayerController.Instance.Setting.Time_Character_Fade).onComplete=()=>{
                            // �˳������Զ����ٽ�ɫ ���������hideָ����Щ�����ϵ��ص�
                            DestroyCharacter(index);
                        };
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        break;
                    }
                case CharacterAction.Disapper2Left:
                    {
                        MoveCharacterTo(index, new Vector2(-500, 0), TransistionType.Smooth);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.Disapper2Right:
                    {
                        MoveCharacterTo(index, new Vector2(2420, 0), TransistionType.Smooth);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.AppearL2R:
                    {
                        character[index].color = Color.white;
                        MoveCharacterTo(index, new Vector2(-500, 0));
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.AppearR2L:
                    {
                        character[index].color = Color.white;
                        MoveCharacterTo(index, new Vector2(2420, 0));
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.Hophop:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, 50), BAStoryPlayerController.Instance.Setting.Time_Character_Hophop, 2);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Hophop);
                        break;
                    }
                case CharacterAction.Greeting:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, -70), BAStoryPlayerController.Instance.Setting.Time_Character_Greeting);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Greeting);
                        break;
                    }
                case CharacterAction.Shake:
                    {
                        character[index].transform.DoShakeX(40, BAStoryPlayerController.Instance.Setting.Time_Character_Shake, 2);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Shake);
                        break;
                    }
                case CharacterAction.Move:
                    {
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.Stiff:
                    {
                        character[index].transform.DoShakeX(10, BAStoryPlayerController.Instance.Setting.Time_Character_Stiff, 4);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Stiff);
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
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, 70), BAStoryPlayerController.Instance.Setting.Time_Character_Jump);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Jump);
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
                        // TODO ����ɾ�����ǽ����˳� ����ʱ����ɾ�� ��Disappear������Щ�ص�
                        //character[index].DoColor(Color.black, TIME_TRANSITION).onComplete = () => { DestroyCharacter(index); };
                        DestroyCharacter(index);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        break;
                }
                default:return;
            }
        }


        /// <summary>
        /// �������س��Ͻ�ɫ
        /// </summary>
        public void HideAll()
        {
            for(int i = 0; i < NUM_CHARACTER_SLOT; i++)
            {
                SetAction(i, CharacterAction.Hide);
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
            // TODO ����ʱʹ���ֶ���λ�ķ�ʽ
            EmotionFactory.SetEmotion(character[index].transform, emotion,LocateMode.Manual);
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
                if (i.color == Color.black) // ִ�й��˳������Ľ�ɫ���ڵ���
                    continue;

                // ��ɫ�ö�
                character[index].transform.SetSiblingIndex(transform.childCount - 1);

                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            i.color = COLOR_UNHIGHLIGHT;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            i.DoColor(COLOR_UNHIGHLIGHT, BAStoryPlayerController.Instance.Setting.Time_Character_Highlight);
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
                            i.DoColor(COLOR_UNHIGHLIGHT, BAStoryPlayerController.Instance.Setting.Time_Character_Highlight);
                            break;
                        }
                    default: return;
                }
            }
        }

        public void ClearAllObject()
        {
            characterPool.Clear();
            transform.ClearAllChild();
        }
    }
}

