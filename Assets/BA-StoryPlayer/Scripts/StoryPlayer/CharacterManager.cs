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
        const float SCALE_CHARACTER = 0.7f;
        const float INTERVAL_SLOT_NORMAL = 0.1666667f;
        float INTERVAL_SLOT => StoryPlayer.CanvasRect.sizeDelta.x * INTERVAL_SLOT_NORMAL;

        Color COLOR_UNHIGHLIGHT { get { return new Color(0.6f, 0.6f, 0.6f);  } }

        [SerializeField] SkeletonGraphic[] character = new SkeletonGraphic[NUM_CHARACTER_SLOT];
        Dictionary<string, GameObject> characterPool = new Dictionary<string, GameObject>();
        Dictionary<string, Coroutine> winkAction = new Dictionary<string, Coroutine>();

        public SkeletonGraphic[] Character { get { return character; } }
        BAStoryPlayer StoryPlayer { get { return BAStoryPlayerController.Instance.StoryPlayer; } }

        [HideInInspector] public UnityEngine.Events.UnityEvent<float> onAnimateCharacter;

        /// <summary>
        /// 激活角色(若有动作,一定要先于动作前调用)
        /// </summary>
        /// <param name="index">初始位置/角色编号</param>
        /// <param name="indexName">角色索引名</param>
        /// <param name="animationID">播放动画的编号</param>
        /// <param name="transistion">出场方式[仅首次出场有效]</param>
        public void ActivateCharacter(int index,string indexName,string animationID,TransistionType transistion = TransistionType.Instant)
        {
            int currentIndex = CheckCharacterExist(indexName);
            // 角色不在场上
            if (currentIndex == -1)
            {
                GameObject obj = null;
                characterPool.TryGetValue(indexName,out obj);

                // 对象池中不存在则载入预制体并初始化相关数据
                if(obj == null)
                {
                    try
                    {
                        obj = CreateCharacterObj(indexName, index);
                    }
                    catch(System.NullReferenceException)
                    {
                        Debug.LogError($"无法在 Resoucres文件夹中的路径 {BAStoryPlayerController.Instance.Setting.Path_Prefab} 找到对应角色预制体");
                        return;
                    }
                }
                else
                {
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector3((index + 1) * INTERVAL_SLOT, 0, 0);
                    obj.transform.rotation = Quaternion.Euler(0, 0, 0);
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

                // 默认 动画01 启用眨眼动画 （用Contains是因为有些动画有前后缀）
                try
                {
                    if (animationID.Contains("01"))
                        SetWinkAction(indexName, true);

                    character[index].AnimationState.SetAnimation(0, animationID, false); // 差分动画
                    character[index].AnimationState.SetAnimation(1, "Idle_01", true); // 呼吸轨道
                }
                catch { }

            }
            // 角色在场上
            else
            {
                // 位置更替 若目标位置有其他角色则直接删除
                if(currentIndex != index)
                {
                    // 先停掉角色的眨眼动画在变换位置
                    SetWinkAction(indexName, false);

                    DestroyCharacter(index);

                    MoveCharacterTo(currentIndex, index);
                    character[index] = character[currentIndex];
                    character[currentIndex] = null;
                }

                try
                {
                    character[index].AnimationState.SetAnimation(0, animationID, true);
                    SetWinkAction(indexName, animationID.Contains("01") ? true : false);
                }
                catch { }

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
                    if (coroutine != null)
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

            string ani_EyeClose_Name = "";
            // 寻找眨眼动画的名字 一般以e开头
            foreach (var i in character[index].SkeletonData.Animations)
            {
                if(i.Name[0] == 'E' || i.Name[0] == 'e')
                {
                    ani_EyeClose_Name = i.Name;
                    break;
                }
            }
            if(ani_EyeClose_Name == "")
            {
                yield break;
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
                        character[currentIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2((targetIndex+1) * INTERVAL_SLOT, 0);
                        break;
                    }
                case TransistionType.Smooth:
                    {
                        character[currentIndex].transform.DoMove_Anchored(new Vector2((targetIndex + 1) * INTERVAL_SLOT, 0), BAStoryPlayerController.Instance.Setting.Time_Character_Move);
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
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        break;
                    }
                case CharacterAction.Disapper:
                    {
                        character[index].color = Color.white;
                        character[index].DoColor(Color.black, BAStoryPlayerController.Instance.Setting.Time_Character_Fade).onComplete=()=>{
                            // 退场动画自动销毁角色 不过好像和hide指令有些功能上的重叠
                            DestroyCharacter(index);
                        };
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                        break;
                    }
                case CharacterAction.Disapper2Left:
                    {
                        MoveCharacterTo(index, new Vector2(-500, 0), TransistionType.Smooth);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.Disapper2Right:
                    {
                        MoveCharacterTo(index, new Vector2(2420, 0), TransistionType.Smooth);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.AppearL2R:
                    {
                        character[index].color = Color.white;
                        MoveCharacterTo(index, new Vector2(-500, 0));
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.AppearR2L:
                    {
                        character[index].color = Color.white;
                        MoveCharacterTo(index, new Vector2(2420, 0));
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.Hophop:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, 50), BAStoryPlayerController.Instance.Setting.Time_Character_Hophop, 2);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Hophop);
                        break;
                    }
                case CharacterAction.Greeting:
                    {
                        character[index].transform.DoBound_Anchored_Relative(new Vector2(0, -70), BAStoryPlayerController.Instance.Setting.Time_Character_Greeting);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Greeting);
                        break;
                    }
                case CharacterAction.Shake:
                    {
                        character[index].transform.DoShakeX(40, BAStoryPlayerController.Instance.Setting.Time_Character_Shake, 2);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Shake);
                        break;
                    }
                case CharacterAction.Move:
                    {
                        MoveCharacterTo(index, arg, TransistionType.Smooth);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                case CharacterAction.Stiff:
                    {
                        character[index].transform.DoShakeX(10, BAStoryPlayerController.Instance.Setting.Time_Character_Stiff, 4);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Stiff);
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
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Jump);
                        break;
                    }
                case CharacterAction.falldownR:
                    {
                        TweenSequence sequence_Rotation = new TweenSequence();
                        sequence_Rotation.Append(character[index].transform.DoEuler(new Vector3(0, 0, -10), 0.3f));
                        sequence_Rotation.Append(character[index].transform.DoEuler(new Vector3(0, 0, 5), 0.5f));
                        sequence_Rotation.Wait(0.3f);
                        sequence_Rotation.Append(character[index].transform.DoLocalMove(character[index].transform.localPosition - new Vector3(0, 1500, 0), 0.3f));
                        sequence_Rotation.Append(() => { DestroyCharacter(index); });

                        TweenSequence sequence_Position = new TweenSequence();
                        sequence_Position.Append(character[index].transform.DoLocalMove(character[index].transform.localPosition + new Vector3(15, 0, 0), 0.3f));
                        sequence_Position.Append(character[index].transform.DoLocalMove(character[index].transform.localPosition - new Vector3(30, 0, 0), 0.5f));

                        onAnimateCharacter?.Invoke(1.4f);
                        break;
                    }
                case CharacterAction.Hide:
                    {
                        // TODO 立即删除还是渐变退出 先暂时立即删除 与Disappear功能有些重叠
                        //character[index].DoColor(Color.black, TIME_TRANSITION).onComplete = () => { DestroyCharacter(index); };
                        DestroyCharacter(index);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
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
        /// 设置角色情绪emoji
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="emotion">表情</param>
        public void SetEmotion(int index,CharacterEmotion emotion)
        {
            if (CheckSlotEmpty(index))
                return;

            float time = 1;
            EmotionFactory.SetEmotion(character[index].transform, emotion,ref time);

            onAnimateCharacter?.Invoke(time);
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
                if (i.color == Color.black) // 执行过退场动画的角色不在点亮
                    continue;

                // 角色置顶
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

        /// <summary>
        /// 创建角色并初始化位置
        /// </summary>
        /// <param name="indexName">索引名</param>
        /// <param name="index">角色下标</param>
        /// <returns></returns>
        GameObject CreateCharacterObj(string indexName,int index)
        {
            GameObject obj;
            obj = BAStoryPlayerController.Instance.LoadCharacterPrefab(indexName);

            obj.transform.SetParent(transform);
            obj.name = indexName;
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = new Vector3((index + 1) * INTERVAL_SLOT, 0, 0);
            rectTransform.localScale = new Vector3(SCALE_CHARACTER, SCALE_CHARACTER, 1);

            obj.GetComponent<SkeletonGraphic>().MatchRectTransformWithBounds();

            characterPool.Add(indexName, obj);

            return obj;
        }

        public void ClearAllObject()
        {
            characterPool.Clear();
            transform.ClearAllChild();
        }
    }
}

