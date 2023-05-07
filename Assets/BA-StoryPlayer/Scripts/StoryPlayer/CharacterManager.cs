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
        /// 激活角色(若有动作,一定要先于动作前调用)
        /// </summary>
        /// <param name="index">初始位置/角色编号</param>
        /// <param name="name">角色姓名(罗马音)</param>
        /// <param name="animationID">播放动画的编号</param>
        /// <param name="transistion">出场方式[仅首次出场有效]</param>
        public void ActivateCharacter(int index,string name,string animationID,TransistionType transistion = TransistionType.Instant)
        {
            int currentIndex = CheckCharacterExist(name);
            // 角色不在场上
            if (currentIndex == -1)
            {
                GameObject obj = null;
                characterPool.TryGetValue(name,out obj);

                // 对象池中不存在则载入并初始化相关数据
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
                else
                {
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector3((index + 1) * VALUE_INTERVAL_SLOT, 0, 0);
                }

                // 对应槽位有其他角色则删除
                DestroyCharacter(index);
                character[index] = obj.GetComponent<SkeletonGraphic>();
                // 首次出场方式
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

                character[index].AnimationState.SetAnimation(0, animationID, false);
                if (animationID == "00")
                    SetWinkAction(name, true);
                character[index].AnimationState.SetAnimation(1, "Idle_01", true);
            }
            // 角色在场上
            else
            {
                // 位置更替 若目标位置有其他角色则直接删除
                if(currentIndex != index)
                {
                    DestroyCharacter(index);

                    MoveCharacterTo(currentIndex, index);
                    character[index] = character[currentIndex];
                    character[currentIndex] = null;
                }

                // 动作替换
                character[index].AnimationState.SetAnimation(0, animationID, true);
                SetWinkAction(name, animationID == "00" ? true : false);

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
        /// 检查角色是否存在并返回对应下标 若不存在则返回-1
        /// </summary>
        /// <param name="name">角色名</param>
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
        /// 检查角色槽位是否为空
        /// </summary>
        /// <param name="index">下标</param>
        /// <returns></returns>
        bool CheckSlotEmpty(int index)
        {
            if (!ValidateIndex(index))
                return false;

            return (Character[index] == null);
        }

        /// <summary>
        /// 尝试删除在场上的角色(不删除对象仅取消编号)
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="destoryObject">完全删除角色</param>
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
        /// 设置开启角色眨眼动画
        /// </summary>
        /// <param name="name">角色名</param>
        /// <param name="enable">开启眨眼</param>
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
                yield return new WaitForSeconds(Random.Range(BAStoryPlayerController.Instance.Setting.Time_Character_Wink.x, BAStoryPlayerController.Instance.Setting.Time_Character_Wink.y));
                while (true)
                {
                    character[index].AnimationState.SetAnimation(0, "Eye_Close_01", false);
                    yield return new WaitForSeconds(Random.Range(BAStoryPlayerController.Instance.Setting.Time_Character_Wink.x, BAStoryPlayerController.Instance.Setting.Time_Character_Wink.y));
                }
            }
        }

        /// <summary>
        /// 变更角色位置但不改变其编号
        /// </summary>
        /// <param name="currentIndex">角色编号</param>
        /// <param name="targetIndex">目标位置编号</param>
        /// <param name="transition">过渡方式</param>
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
            Debug.LogError($"角色槽位下标 {index} 超出范围[0-4]");
            return false;
        }

        /// <summary>
        /// 使角色执行某种动作
        /// </summary>
        /// <param name="index">编号</param>
        /// <param name="action">动作</param>
        /// <param name="arg">参数(可选)</param>
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
                        character[index].DoColor(Color.black, BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
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
                        Debug.Log($"{action}没做");
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
                        Debug.Log($"{action}没做");
                        break;
                    }
                case CharacterAction.Hide:
                    {
                        // TODO 立即删除还是渐变退出 先暂时立即删除
                        //character[index].DoColor(Color.black, TIME_TRANSITION).onComplete = () => { DestroyCharacter(index); };
                        DestroyCharacter(index);
                        OnAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        break;
                }
                default:return;
            }
        }


        /// <summary>
        /// 立即隐藏场上角色
        /// </summary>
        public void HideAll()
        {
            for(int i = 0; i < NUM_CHARACTER_SLOT; i++)
            {
                SetAction(i, CharacterAction.Hide);
            }
        }

        /// <summary>
        /// 完全删除角色
        /// </summary>
        public void ClearAll()
        {
            for (int i = 0; i < NUM_CHARACTER_SLOT; i++)
            {
                DestroyCharacter(i, true);
            }
        }

        /// <summary>
        /// 设置角色情绪emoji
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="emotion">表情</param>
        public void SetEmotion(int index,CharacterEmotion emotion)
        {
            if (CheckSlotEmpty(index))
                return;

            EmotionFactory.SetEmotion(character[index].transform, emotion);
        }

        /// <summary>
        /// 高亮某个角色
        /// </summary>
        /// <param name="index">角色下标</param>
        /// <param name="transition">过渡方式</param>
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
                            i.DoColor(COLOR_UNHIGHLIGHT, BAStoryPlayerController.Instance.Setting.Time_Character_Highlight);
                            break;
                        }
                    default: return;
                }
            }
        }
        /// <summary>
        /// 设置是否高亮所有角色
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
    }
}

