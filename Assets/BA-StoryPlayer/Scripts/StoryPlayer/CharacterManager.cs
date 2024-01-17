using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Spine.Unity;

using Random = UnityEngine.Random;

using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.Event;
using System.Collections.ObjectModel;

namespace BAStoryPlayer
{
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
        Jump,
        falldownR,
        falldownL,
        Hide,
        Close,
        Back
    }

    public class CharacterManager : PlayerModule
    {
        private const int CharacterSlotCount = 5;
        private const float CharacterScale = 0.7f;
        private const float SlotIntervalNormal = 0.1666667f;
        private float SlotInterval => StoryPlayer.CanvasRect.sizeDelta.x * SlotIntervalNormal;

        private Color ColorUnhighlight { get { return new Color(0.6f, 0.6f, 0.6f);  } }

        [SerializeField] private SkeletonGraphic[] _character = new SkeletonGraphic[CharacterSlotCount];
        private readonly Dictionary<string, GameObject> _characterPool = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Coroutine> _winkAction = new Dictionary<string, Coroutine>();
        private EmotionFactory _emotionFactory;

        public SkeletonGraphic[] Character => _character;
        public Dictionary<string, GameObject> CharacterPool => _characterPool;
        public EmotionFactory EmotionFactory
        {
            get
            {
                if(_emotionFactory == null)
                {
                    _emotionFactory = new(StoryPlayer);
                }
                return _emotionFactory;
            }
        }

        /// <summary>
        /// 激活角色  (若有动作,一定要先于动作前调用)
        /// </summary>
        public void ActivateCharacter(int index,string indexName,string animationID)
        {
            int currentIndex = CheckIfCharacterOnSlot(indexName);
            // 角色不在场上
            if (currentIndex == -1)
            {
                GameObject obj = null;
                _characterPool.TryGetValue(indexName,out obj);

                // 对象池中不存在则载入预制体并初始化相关数据
                if(obj == null)
                {
                    obj = CreateCharacterObj(indexName, index);
                }
                else
                {
                    obj.gameObject.SetActive(true);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector3((index + 1) * SlotInterval, 0, 0);
                    obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                }

                // 对应槽位有其他角色则删除
                DestroyCharacter(index);
                _character[index] = obj.GetComponent<SkeletonGraphic>();
            }
            // 角色在场上
            else
            {
                // 位置更替 若目标位置有其他角色则直接删除
                if(currentIndex != index)
                {
                    DestroyCharacter(index);
                    MoveCharacterTo(currentIndex, index);
                    _character[index] = _character[currentIndex];
                    _character[currentIndex] = null;
                }
            }

            SetAnimation(index, animationID);
            // NOTE
            // 由于说话者和角色名脱钩 所以这里不在默认高亮角色
            // Highlight(index);
        }
        [Obsolete]
        public void ActivateCharacter(int index, string indexName, string animationID, string lines)
        {
            ActivateCharacter(index, indexName, animationID);
            StoryPlayer.UIModule.SetSpeaker(indexName);
            StoryPlayer.UIModule.PrintMainText(lines);
        }
        public void ActivateCharacter(string indexName)
        {
            if (!CheckIfCharacterInPool(indexName))
            {
                CreateCharacterObj(indexName);
            }
            _characterPool[indexName].SetActive(true);
        }

        private bool CheckIfCharacterInPool(string indexName) => _characterPool.ContainsKey(indexName) ? _characterPool[indexName] != null : false;
        private int CheckIfCharacterOnSlot(string indexName)
        {
            for(int i = 0; i < CharacterSlotCount; i++)
            {
                if (Character[i] == null)
                    continue;
                if (Character[i].gameObject.name == indexName)
                    return i;
            }

            return -1;
        }
        private bool CheckIfSlotEmpty(int index) => CheckIfIndexValid(index) ? Character[index] == null : false;
        private bool CheckIfIndexValid(int index) => (index >= 0 && index < CharacterSlotCount);

        /// <summary>
        /// 尝试删除在场上的角色(不删除对象仅取消编号)
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="destroyObject">完全删除角色</param>
        private void DestroyCharacter(int index,bool destroyObject = false)
        {
            if (CheckIfSlotEmpty(index))
                return;
            // 先停掉角色的眨眼动画在变换位置
            SetWinkAction(Character[index].name, false);
            DestroyCharacter(Character[index].gameObject);
            _character[index] = null;
        }
        void DestroyCharacter(string indexName,bool destroyObject = false)
        {
            if (!CheckIfCharacterInPool(indexName))
                return;
            Destroy(_characterPool[indexName]);
        }
        void DestroyCharacter(GameObject obj,bool destoryObject = false)
        {
            if (destoryObject)
            {
                _characterPool.Remove(obj.name);
                Destroy(obj);
            }
            else
            {
                obj.SetActive(false);
            }

            SetWinkAction(obj.name, false);
            int slotIndex = CheckIfCharacterOnSlot(obj.name);
            if (slotIndex != -1)
                _character[slotIndex] = null;
        }

        /// <summary>
        /// 设置开启角色眨眼动画
        /// </summary>
        /// <param name="indexName">角色名</param>
        /// <param name="enable">开启眨眼</param>
        void SetWinkAction(string indexName,bool enable)
        {
            Coroutine coroutine = null;
            _winkAction.TryGetValue(indexName, out coroutine);
            try
            {
                if (enable)
                {
                    if (coroutine == null)
                    {
                        coroutine = StartCoroutine(CCharacterWink(indexName));
                        if (coroutine != null)
                            _winkAction.Add(indexName, coroutine);
                    }
                }
                else
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        _winkAction.Remove(indexName);
                    }
                }
            }
            catch
            {
                StopCoroutine(coroutine);
            }
            
        }
        IEnumerator CCharacterWink(string indexName)
        {
            SkeletonGraphic skel = CharacterPool[indexName].GetComponent<SkeletonGraphic>();
            string ani_EyeClose_Name = "";
            // 寻找眨眼动画的名字 一般以e开头
            foreach (var i in skel.SkeletonData.Animations)
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

            yield return new WaitForSeconds(Random.Range(base.StoryPlayer.Setting.TimeRangeCharacterWink.x, base.StoryPlayer.Setting.TimeRangeCharacterWink.y));
            while (true)
            {
                skel.AnimationState.SetAnimation(0, ani_EyeClose_Name, false);
                yield return new WaitForSeconds(Random.Range(base.StoryPlayer.Setting.TimeRangeCharacterWink.x, base.StoryPlayer.Setting.TimeRangeCharacterWink.y));
            }
        }

        /// <summary>
        /// 变更角色位置但不改变其编号
        /// </summary>
        /// <param name="currentIndex">角色编号</param>
        /// <param name="targetIndex">目标位置编号</param>
        /// <param name="transition">过渡方式</param>
        void MoveCharacterTo(int currentIndex,int targetIndex,TransistionType transition = TransistionType.Immediate)
        {
            if (!CheckIfIndexValid(currentIndex) || !CheckIfIndexValid(targetIndex))
                return;
            if (CheckIfSlotEmpty(currentIndex))
                return;
            MoveCharacterTo(Character[currentIndex].gameObject, targetIndex, transition);
        }
        void MoveCharacterTo(int currentIndex,Vector2 pos,TransistionType transition = TransistionType.Immediate)
        {
            if (CheckIfSlotEmpty(currentIndex))
                return;
            MoveCharacterTo(Character[currentIndex].gameObject, pos, transition);
        }
        public void MoveCharacterTo(string indexName, int targetIndex, TransistionType transition = TransistionType.Immediate)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            _characterPool[indexName].SetActive(true);
            MoveCharacterTo(_characterPool[indexName], targetIndex, transition);
        }
        void MoveCharacterTo(string indexName, Vector2 pos, TransistionType transition = TransistionType.Immediate)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            _characterPool[indexName].SetActive(true);
            MoveCharacterTo(_characterPool[indexName], pos, transition);
        }
        void MoveCharacterTo(GameObject obj, int targetIndex, TransistionType transition = TransistionType.Immediate)
        {
            targetIndex = Mathf.Clamp(targetIndex, 0, 4);
            RectTransform rect = obj.GetComponent<RectTransform>();
            switch (transition)
            {
                case TransistionType.Immediate:
                    {
                        rect.anchoredPosition = new Vector2((targetIndex + 1) * SlotInterval, rect.anchoredPosition.y);
                        break;
                    }
                case TransistionType.Fade:
                    {
                        obj.transform.DoMove_Anchored(new Vector2((targetIndex + 1) * SlotInterval, rect.anchoredPosition.y), base.StoryPlayer.Setting.TimeCharacterMove);
                        break;
                    }
                default: break;
            }
        }
        void MoveCharacterTo(GameObject obj, Vector2 pos, TransistionType transition = TransistionType.Immediate)
        {
            switch (transition)
            {
                case TransistionType.Immediate:
                    {
                        obj.GetComponent<RectTransform>().anchoredPosition = pos;
                        break;
                    }
                case TransistionType.Fade:
                    {
                        obj.transform.DoMove_Anchored(pos, base.StoryPlayer.Setting.TimeCharacterMove);
                        EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterMove });
                        break;
                    }
                default: break;
            }
        }

        /// <summary>
        /// 使角色执行某种动作
        /// </summary>
        /// <param name="index">编号</param>
        /// <param name="action">动作</param>
        /// <param name="arg">参数(可选)</param>
        public void SetAction(int index,CharacterAction action,int arg = -1)
        {
            if (CheckIfSlotEmpty(index))
                return;
            SetAction(Character[index].gameObject, action, arg);
        }
        public void SetAction(string indexName,CharacterAction action,int arg = -1)
        {
            if (!CheckIfCharacterInPool(indexName))
            {
                CreateCharacterObj(indexName);
            }

            _characterPool[indexName].SetActive(true);
            SetAction(_characterPool[indexName], action,arg);
        }
        public void SetAction(GameObject obj,CharacterAction action,int arg = -1)
        {
            SkeletonGraphic skelGraphic = obj.GetComponent<SkeletonGraphic>();

            switch (action)
            {
                case CharacterAction.Appear: //黑色剪影渐变进场
                    skelGraphic.color = Color.black;
                    skelGraphic.DoColor(Color.white, base.StoryPlayer.Setting.TimeCharacterFade);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterFade });
                    break;
                case CharacterAction.Disapper: // 渐变至黑色剪影同时离场
                    skelGraphic.color = Color.white;
                    skelGraphic.DoColor(Color.black, base.StoryPlayer.Setting.TimeCharacterFade).OnCompleted = ()=> { SetAction(obj, CharacterAction.Hide); };
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterFade });
                    break;
                case CharacterAction.Disapper2Left:
                    MoveCharacterTo(obj, new Vector2(-500, 0), TransistionType.Fade);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterMove });
                    break;
                case CharacterAction.Disapper2Right:
                    MoveCharacterTo(obj, new Vector2(2420, 0), TransistionType.Fade);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterMove });
                    break;
                case CharacterAction.AppearL2R:
                    skelGraphic.color = Color.white;
                    MoveCharacterTo(obj, new Vector2(-500, 0));
                    MoveCharacterTo(obj, arg, TransistionType.Fade);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterMove });
                    break;
                case CharacterAction.AppearR2L:
                    skelGraphic.color = Color.white;
                    MoveCharacterTo(obj, new Vector2(2420, 0));
                    MoveCharacterTo(obj, arg, TransistionType.Fade);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterMove });
                    break;
                case CharacterAction.Hophop:
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, 50), base.StoryPlayer.Setting.TimeCharacterHophop, 2);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterHophop });
                    break;
                case CharacterAction.Greeting:
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, -70), base.StoryPlayer.Setting.TimeCharacterGreeting);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterGreeting });
                    break;
                case CharacterAction.Shake:
                    obj.transform.DoShakeX(40, base.StoryPlayer.Setting.TimeCharacterShake, 2);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterShake });
                    break;
                case CharacterAction.Move:
                    MoveCharacterTo(obj, arg, TransistionType.Fade);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterMove });
                    break;
                case CharacterAction.Stiff:
                    obj.transform.DoShakeX(10, base.StoryPlayer.Setting.TimeCharacterStiff, 4);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterStiff });
                    break;
                case CharacterAction.Jump:
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, 70), base.StoryPlayer.Setting.TimeCharacterJump);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = base.StoryPlayer.Setting.TimeCharacterJump });
                    break;
                case CharacterAction.falldownR:
                    {
                        TweenSequence sequence_Rotation = new TweenSequence();
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, -10), 0.3f).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.1f);
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, 5), 0.5f).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.3f);
                        sequence_Rotation.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(0, 1500, 0), 0.3f).SetEase(Ease.InCirc));

                        TweenSequence sequence_Position = new TweenSequence();
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition + new Vector3(30, 0, 0), 0.3f).SetEase(Ease.OutCubic));
                        sequence_Position.Wait(0.1f);
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(60, 0, 0), 0.5f).SetEase(Ease.OutCubic));

                        EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = 1.6f });
                        break;
                    }
                case CharacterAction.falldownL:
                    {
                        TweenSequence sequence_Rotation = new TweenSequence();
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, 5), 0.3f).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.1f);
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, -10), 0.5f).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.3f);
                        sequence_Rotation.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(0, 1500, 0), 0.3f).SetEase(Ease.InCirc));

                        TweenSequence sequence_Position = new TweenSequence();
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition + new Vector3(30, 0, 0), 0.3f).SetEase(Ease.OutCubic));
                        sequence_Position.Wait(0.1f);
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(60, 0, 0), 0.5f).SetEase(Ease.OutCubic));

                        EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = 1.6f });
                        break;
                    }
                case CharacterAction.Hide: // 立即让角色离场
                    DestroyCharacter(obj);
                    break;
                case CharacterAction.Close:
                    obj.transform.localScale = Vector3.one;
                    obj.GetComponent<RectTransform>().anchoredPosition = obj.GetComponent<RectTransform>().anchoredPosition * Vector2.right - Vector2.up * 350;
                    MoveCharacterTo(obj, 2);
                    break;
                case CharacterAction.Back:
                    obj.transform.localScale = Vector3.one * 0.7f;
                    obj.GetComponent<RectTransform>().anchoredPosition = obj.GetComponent<RectTransform>().anchoredPosition * Vector2.right;
                    break;
                default: return;
            }
        }

        /// <summary>
        /// 立即隐藏场上角色
        /// </summary>
        public void HideAll()
        {
            for(int i = 0; i < CharacterSlotCount; i++)
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
            if (CheckIfSlotEmpty(index))
                return;
            SetEmotion(Character[index].gameObject,emotion);
        }
        public void SetEmotion(string indexName, CharacterEmotion emotion)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            _characterPool[indexName].SetActive(true);
            SetEmotion(_characterPool[indexName], emotion);
        }
        public void SetEmotion(GameObject obj,CharacterEmotion emotion)
        {
            float time = 1;
            EmotionFactory.SetEmotion(obj.transform, emotion, base.StoryPlayer.CharacterDataTable[obj.name], time);
            EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = time });
        }

        /// <summary>
        /// 高亮并置顶某个角色
        /// </summary>
        public void Highlight(int index,TransistionType transition = TransistionType.Immediate)
        {
            if (CheckIfSlotEmpty(index))
                return;
            Highlight(Character[index].gameObject, transition);
        }
        public void Highlight(string indexName, TransistionType transition = TransistionType.Immediate)
        {
            if (!CheckIfCharacterInPool(indexName))
            {
                // CreateCharacterObj(indexName);
                return;
            }
            _characterPool[indexName].SetActive(true);
            Highlight(_characterPool[indexName], transition);
        }
        public void Highlight(GameObject obj, TransistionType transition = TransistionType.Immediate)
        {
            SkeletonGraphic skelGraphic = obj.GetComponent<SkeletonGraphic>();

            skelGraphic.color = Color.white;
            obj.transform.SetSiblingIndex(transform.childCount - 1);

            foreach (var chr in _characterPool.Values)
            {
                if (chr == obj || !chr.activeSelf)
                    continue;
                SkeletonGraphic skg = chr.GetComponent<SkeletonGraphic>();
                if (skg.color == Color.black)
                    continue;

                switch (transition)
                {
                    case TransistionType.Immediate:
                        {
                            skg.color = ColorUnhighlight;
                            break;
                        }
                    case TransistionType.Fade:
                        {
                            skg.DoColor(ColorUnhighlight, base.StoryPlayer.Setting.TimeCharacterHighlight);
                            break;
                        }
                    default: return;
                }
            }
        }

        /// <summary>
        /// 高亮所有角色
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="transition"></param>
        public void HighlightAll(TransistionType transition = TransistionType.Immediate)
        {
            foreach (var chr in _characterPool.Values)
            {
                if (!chr.activeSelf)
                    continue;

                SkeletonGraphic skelGraphic = chr.GetComponent<SkeletonGraphic>();
                switch (transition)
                {
                    case TransistionType.Immediate:
                        {
                            skelGraphic.color = ColorUnhighlight;
                            break;
                        }
                    case TransistionType.Fade:
                        {
                            skelGraphic.DoColor(ColorUnhighlight, base.StoryPlayer.Setting.TimeCharacterHighlight);
                            break;
                        }
                    default: return;
                }
            }
        }

        /// <summary>
        /// 创建角色并初始化
        /// </summary>
        /// <param name="indexName">索引名</param>
        /// <param name="index">角色下标</param>
        /// <returns></returns>
        private GameObject CreateCharacterObj(string indexName,int index = 2)
        {
            GameObject obj;
            obj = LoadCharacterPrefab(indexName);

            obj.transform.SetParent(transform);
            obj.name = indexName;
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = new Vector3((index + 1) * SlotInterval, 0, 0);
            rectTransform.localScale = new Vector3(CharacterScale, CharacterScale, 1);

            obj.GetComponent<SkeletonGraphic>().MatchRectTransformWithBounds();

            SetAnimation(obj.GetComponent<SkeletonGraphic>(), "Idle_01", 1, true); // 呼吸轨道

            _characterPool.Remove(indexName);
            _characterPool.Add(indexName, obj);

            return obj;
        }

        /// <summary>
        /// 加载角色实体
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public GameObject LoadCharacterPrefab(string indexName)
        {
            GameObject prefab = null;

            try
            {
                switch (base.StoryPlayer.CharacterDataTable[indexName].loadType)
                {
                    case LoadType.Prefab:
                        {
                            prefab = Instantiate(Resources.Load(base.StoryPlayer.Setting.PathCharacterSkeletonData + base.StoryPlayer.CharacterDataTable[indexName].skeletonDataUrl) as GameObject);
                            break;
                        }
                    case LoadType.SkeletonData:
                        {
                            SkeletonDataAsset skelData =
                                Resources.Load<SkeletonDataAsset>(base.StoryPlayer.Setting.PathCharacterSkeletonData
                                    + base.StoryPlayer.CharacterDataTable[indexName].skeletonDataUrl);

                            Shader shader = Shader.Find("Spine/SkeletonGraphic");
                            if (shader == null)
                            {
                                shader = Resources.Load<Shader>("Shader/Spine-SkeletonGraphic");
                            }
                            Material mat = new Material(shader);
                            UnityEngine.Rendering.LocalKeyword keyword = new UnityEngine.Rendering.LocalKeyword(mat.shader, "_STRAIGHT_ALPHA_INPUT");
                            mat.SetKeyword(keyword, true);
                            prefab = SkeletonGraphic.NewSkeletonGraphicGameObject(skelData, transform, mat).gameObject;
                            break;
                        }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }

            prefab.name = indexName;
            return prefab;
        }

        /// <summary>
        /// 设置Spine动画
        /// </summary>
        public void SetAnimation(int index, string animationID, int track = 0, bool loop = true)
        {
            if (CheckIfSlotEmpty(index))
                return;
            SetAnimation(Character[index], animationID, track, loop);
            SetWinkAction(Character[index].name, animationID.Contains("01") ? true : false);
        }
        public void SetAnimation(string indexName,string animationID, int track = 0, bool loop = true)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            _characterPool[indexName].SetActive(true);
            SetAnimation(_characterPool[indexName].GetComponent<SkeletonGraphic>(),animationID,track,loop);
            SetWinkAction(indexName, animationID.Contains("01") ? true : false);
        }
        public void SetAnimation(SkeletonGraphic skel,string animationID,int track = 0,bool loop = true)
        {
            try
            {
                skel.AnimationState.SetAnimation(track, animationID, loop);
            }
            catch { }
        }

        public void ClearAllObject()
        {
            foreach(var i in _winkAction.Values)
            {
                StopCoroutine(i);
            }
            _winkAction.Clear();

            _characterPool.Clear();
            _character = new SkeletonGraphic[CharacterSlotCount];
            transform.ClearAllChild();
        }
    }
}

