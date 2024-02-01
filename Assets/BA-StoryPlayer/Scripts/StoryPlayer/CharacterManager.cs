using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Spine.Unity;
using Random = UnityEngine.Random;

namespace BAStoryPlayer
{
    using DoTweenS;
    using Event;

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
        private Vector2 ScaleOffset => Vector2.up * 350; // 角色放大后的偏移量
        private Vector2 StandByPositionLeft => new Vector2(-SlotInterval * 2.5f, 0);
        private Vector2 StandByPositionRight => new Vector2(StoryPlayer.CanvasRect.sizeDelta.x + SlotInterval * 2.5f, 0);

        private Color ColorUnhighlight { get { return new Color(0.6f, 0.6f, 0.6f);  } }

        [SerializeField] private SkeletonGraphic[] _characters = new SkeletonGraphic[CharacterSlotCount];
        private readonly Dictionary<string, GameObject> _characterPool = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Coroutine> _winkAction = new Dictionary<string, Coroutine>();
        private EmotionFactory _emotionFactory;

        public SkeletonGraphic[] Characters => _characters;
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
                CharacterPool.TryGetValue(indexName,out obj);

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
                Characters[index] = obj.GetComponent<SkeletonGraphic>();
            }
            // 角色在场上
            else
            {
                // 位置更替 若目标位置有其他角色则直接删除
                if(currentIndex != index)
                {
                    DestroyCharacter(index);
                    MoveCharacterTo(currentIndex, index,0);
                    Characters[index] = Characters[currentIndex];
                    Characters[currentIndex] = null;
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
        [Obsolete]
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
                if (Characters[i] == null)
                    continue;
                if (Characters[i].gameObject.name == indexName)
                    return i;
            }

            return -1;
        }
        private bool CheckIfSlotEmpty(int index) => CheckIfIndexValid(index) ? Characters[index] == null : false;
        private bool CheckIfIndexValid(int index) => (index >= 0 && index < CharacterSlotCount);

        private void DestroyCharacter(int index,bool destroyObject = false)
        {
            if (CheckIfSlotEmpty(index))
                return;
            // 先停掉角色的眨眼动画在变换位置
            SetWinkAction(Characters[index].name, false);
            DestroyCharacter(Characters[index].gameObject);
            _characters[index] = null;
        }
        private void DestroyCharacter(string indexName,bool destroyObject = false)
        {
            if (!CheckIfCharacterInPool(indexName))
                return;
            Destroy(_characterPool[indexName]);
        }
        private void DestroyCharacter(GameObject obj,bool destoryObject = false)
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
                _characters[slotIndex] = null;
        }

        private void SetWinkAction(string indexName,bool enable)
        {
            Coroutine coroutine = null;
            _winkAction.TryGetValue(indexName, out coroutine);
            try
            {
                if (enable)
                {
                    if (coroutine == null)
                    {
                        coroutine = StartCoroutine(CrtCharacterWink(indexName));
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
        private IEnumerator CrtCharacterWink(string indexName)
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

            yield return new WaitForSeconds(Random.Range(StoryPlayer.Setting.DefaultTimeRangeCharacterWink.x, StoryPlayer.Setting.DefaultTimeRangeCharacterWink.y));
            while (true)
            {
                skel.AnimationState.SetAnimation(0, ani_EyeClose_Name, false);
                yield return new WaitForSeconds(Random.Range(StoryPlayer.Setting.DefaultTimeRangeCharacterWink.x, StoryPlayer.Setting.DefaultTimeRangeCharacterWink.y));
            }
        }

        private void MoveCharacterTo(int currentIndex,int targetIndex, float time)
        {
            if (!CheckIfIndexValid(currentIndex) || !CheckIfIndexValid(targetIndex))
                return;
            if (CheckIfSlotEmpty(currentIndex))
                return;
            MoveCharacterTo(Characters[currentIndex].gameObject, targetIndex, time);
        }
        private void MoveCharacterTo(int currentIndex,Vector2 pos, float time)
        {
            if (CheckIfSlotEmpty(currentIndex))
                return;
            MoveCharacterTo(Characters[currentIndex].gameObject, pos, time);
        }
        [Obsolete]
        public void MoveCharacterTo(string indexName, int targetIndex, float time)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            _characterPool[indexName].SetActive(true);
            MoveCharacterTo(_characterPool[indexName], targetIndex, time);
        }
        [Obsolete]
        private void MoveCharacterTo(string indexName, Vector2 pos, float time)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            CharacterPool[indexName].SetActive(true);
            MoveCharacterTo(CharacterPool[indexName], pos, time);
        }
        private void MoveCharacterTo(GameObject obj, int targetIndex, float time)
        {
            targetIndex = Mathf.Clamp(targetIndex, 0, 5);
            RectTransform rect = obj.GetComponent<RectTransform>();
            MoveCharacterTo(obj, new Vector2((targetIndex + 1) * SlotInterval, rect.anchoredPosition.y), time);
        }
        private void MoveCharacterTo(GameObject obj, Vector2 pos, float time)
        {
            obj.transform.localRotation = Quaternion.identity;

            if (time == 0)
            {
                obj.GetComponent<RectTransform>().anchoredPosition = pos;
            }
            else if (time > 0)
            {
                obj.transform.DoMove_Anchored(pos, time);
                EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = time });
            }
            else
            {
                obj.transform.DoMove_Anchored(pos, StoryPlayer.Setting.DefaultTimeCharacterMove);
                EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = StoryPlayer.Setting.DefaultTimeCharacterMove });
            }
        }

        public void SetAction(int index, CharacterAction action,  float time)
        {
            SetAction(index, action, -1, time);
        }
        public void SetAction(int index,CharacterAction action,int arg = -1, float time = 0)
        {
            if (CheckIfSlotEmpty(index))
                return;
            SetAction(Characters[index].gameObject, action, arg, time);
        }
        [Obsolete]
        public void SetAction(string indexName,CharacterAction action,int arg = -1, float time = 0)
        {
            if (!CheckIfCharacterInPool(indexName))
            {
                CreateCharacterObj(indexName);
            }

            CharacterPool[indexName].SetActive(true);
            SetAction(CharacterPool[indexName], action,arg,time);
        }
        public void SetAction(GameObject obj,CharacterAction action,int arg = -1, float time = 0)
        {
            SkeletonGraphic skelGraphic = obj.GetComponent<SkeletonGraphic>();
            float actionTime;

            switch (action)
            {
                case CharacterAction.Appear: //黑色剪影渐变进场
                    skelGraphic.color = Color.black;
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterFade : time;
                    skelGraphic.DoColor(Color.white, actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Disapper: // 渐变至黑色剪影同时离场
                    skelGraphic.color = Color.white;
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterFade : time;
                    skelGraphic.DoColor(Color.black, actionTime).onCompleted = ()=> { SetAction(obj, CharacterAction.Hide); };
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Disapper2Left:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterMove : time;
                    MoveCharacterTo(obj,StandByPositionLeft , actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Disapper2Right:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterMove : time;
                    MoveCharacterTo(obj,StandByPositionRight , actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.AppearL2R:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterMove : time;
                    skelGraphic.color = Color.white;
                    MoveCharacterTo(obj, StandByPositionLeft, 0);
                    MoveCharacterTo(obj, arg, actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.AppearR2L:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterMove : time;
                    skelGraphic.color = Color.white;
                    MoveCharacterTo(obj, StandByPositionRight,0);
                    MoveCharacterTo(obj, arg, actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Hophop:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterHophop : time;
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, 50), actionTime, 2);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Greeting:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterGreeting : time;
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, -70), actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Shake:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterShake : time;
                    obj.transform.DoShakeX(40, actionTime, 2);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Move:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterMove : time;
                    MoveCharacterTo(obj, arg, actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Stiff:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterStiff : time;
                    obj.transform.DoShakeX(10, actionTime, 4);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.Jump:
                    actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeCharacterJump : time;
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, 70), actionTime);
                    EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                    break;
                case CharacterAction.falldownR:
                    {
                        int index = CheckIfCharacterOnSlot(obj.name);
                        MoveCharacterTo(obj, index, 0);
                        var rect = obj.GetComponent<RectTransform>();
                        rect.anchoredPosition = new Vector2(
                                rect.anchoredPosition.x,
                                obj.transform.localScale.x == 1 ? -350 : 0
                            );

                        actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeFalldown : time;

                        TweenSequence sequence_Rotation = new TweenSequence();
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, -10), 0.1875f * actionTime).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.0625f * actionTime);
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, 5), 0.3125f * actionTime).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.1875f * actionTime);
                        sequence_Rotation.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(0, 1500, 0), 0.1875f * actionTime).SetEase(Ease.InCirc));

                        TweenSequence sequence_Position = new TweenSequence();
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition + new Vector3(30, 0, 0), 0.1875f * actionTime).SetEase(Ease.OutCubic));
                        sequence_Position.Wait(0.0625f * actionTime);
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(60, 0, 0), 0.3125f * actionTime).SetEase(Ease.OutCubic));

                        EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                        break;
                    }
                case CharacterAction.falldownL:
                    {
                        int index = CheckIfCharacterOnSlot(obj.name);
                        MoveCharacterTo(obj, index, 0);
                        var rect = obj.GetComponent<RectTransform>();
                        rect.anchoredPosition = new Vector2(
                                rect.anchoredPosition.x,
                                obj.transform.localScale.x == 1 ? -350 : 0
                            );

                        actionTime = time <= 0 ? StoryPlayer.Setting.DefaultTimeFalldown : time;

                        TweenSequence sequence_Rotation = new TweenSequence();
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, 5), 0.1875f * actionTime).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.0625f * actionTime);
                        sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, -10), 0.3125f * actionTime).SetEase(Ease.OutCubic));
                        sequence_Rotation.Wait(0.1875f * actionTime);
                        sequence_Rotation.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(0, 1500, 0), 0.1875f * actionTime).SetEase(Ease.InCirc));

                        TweenSequence sequence_Position = new TweenSequence();
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition + new Vector3(30, 0, 0), 0.1875f * actionTime).SetEase(Ease.OutCubic));
                        sequence_Position.Wait(0.0625f * actionTime);
                        sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(60, 0, 0), 0.3125f * actionTime).SetEase(Ease.OutCubic));

                        EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = actionTime });
                        break;
                    }
                case CharacterAction.Hide: // 立即让角色离场
                    DestroyCharacter(obj);
                    break;
                case CharacterAction.Close:
                    obj.transform.localScale = Vector3.one;
                    obj.GetComponent<RectTransform>().anchoredPosition = obj.GetComponent<RectTransform>().anchoredPosition * Vector2.right - ScaleOffset;
                    break;
                case CharacterAction.Back:
                    obj.transform.localScale = Vector3.one * 0.7f;
                    obj.GetComponent<RectTransform>().anchoredPosition = obj.GetComponent<RectTransform>().anchoredPosition * Vector2.right;
                    break;
                default: return;
            }
        }

        public void HideAll()
        {
            for(int i = 0; i < CharacterSlotCount; i++)
            {
                SetAction(i, CharacterAction.Hide);
            }
        }

        public void SetEmotion(int index,CharacterEmotion emotion)
        {
            if (CheckIfSlotEmpty(index))
                return;
            SetEmotion(Characters[index].gameObject,emotion);
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
            EmotionFactory.SetEmotion(obj.transform, emotion, StoryPlayer.CharacterDataTable[obj.name], time);
            EventBus<OnSetCharacterAction>.Raise(new OnSetCharacterAction() { time = time });
        }

        public void Highlight(int index, float time = 0)
        {
            if (CheckIfSlotEmpty(index))
                return;
            Highlight(Characters[index].gameObject, time);
        }
        [Obsolete]
        public void Highlight(string indexName, float time = 0)
        {
            if (!CheckIfCharacterInPool(indexName))
            {
                // CreateCharacterObj(indexName);
                return;
            }
            _characterPool[indexName].SetActive(true);
            Highlight(_characterPool[indexName], time);
        }
        public void Highlight(GameObject obj, float time = 0)
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

                if (time == 0)
                {
                    skg.color = ColorUnhighlight;
                }
                else
                {
                    skg.DoColor(
                        ColorUnhighlight, 
                        time > 0 ? time :  StoryPlayer.Setting.DefaultTimeCharacterHighlight
                        );
                }
            }
        }

        public void HighlightAll(float time = 0)
        {
            foreach (var chr in _characterPool.Values)
            {
                if (!chr.activeSelf)
                    continue;

                SkeletonGraphic skelGraphic = chr.GetComponent<SkeletonGraphic>();
                if(time == 0)
                {
                    skelGraphic.color = ColorUnhighlight;
                }
                else
                {
                    skelGraphic.DoColor(
                        ColorUnhighlight,
                        time > 0 ? time : StoryPlayer.Setting.DefaultTimeCharacterHighlight
                        );
                }
            }
        }

        public GameObject CreateCharacterObj(string indexName,int initialPosIndex = 2,bool createErrorChrIfNull = true)
        {
            GameObject obj;
            obj = LoadCharacterFromSkeletonData(indexName);
            if(obj == null)
            {
                return createErrorChrIfNull ? CreateErrorCharacter(indexName, initialPosIndex) : null;
            }

            obj.transform.SetParent(transform);
            obj.name = indexName;
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = new Vector3((initialPosIndex + 1) * SlotInterval, 0, 0);
            rectTransform.localScale = new Vector3(CharacterScale, CharacterScale, 1);

            obj.GetComponent<SkeletonGraphic>().MatchRectTransformWithBounds();

            SetAnimation(obj.GetComponent<SkeletonGraphic>(), "Idle_01", 1, true); // 呼吸轨道

            CharacterPool.Remove(indexName);
            CharacterPool.Add(indexName, obj);

            return obj;
        }
        private GameObject LoadCharacterFromSkeletonData(string indexName)
        {
            if (!StoryPlayer.CharacterDataTable.ContainsKey(indexName))
            {
                return null;
            }

            GameObject prefab = null;

            switch (StoryPlayer.CharacterDataTable[indexName].loadType)
            {
                case LoadType.Prefab:
                    {
                        prefab = Instantiate(Resources.Load(StoryPlayer.Setting.PathCharacterSkeletonData(StoryPlayer.CharacterDataTable[indexName].skeletonDataUrl)) as GameObject);
                        break;
                    }
                case LoadType.SkeletonData:
                    {
                        SkeletonDataAsset skelData =
                            Resources.Load<SkeletonDataAsset>(StoryPlayer.Setting.PathCharacterSkeletonData(StoryPlayer.CharacterDataTable[indexName].skeletonDataUrl));

                        Shader shader = Shader.Find("Spine/SkeletonGraphic");
                        if (shader == null)
                        {
                            shader = Resources.Load<Shader>("Shader/Spine-SkeletonGraphic");
                        }
                        Material mat = new Material(shader);
                        //UnityEngine.Rendering.LocalKeyword keyword = new UnityEngine.Rendering.LocalKeyword(mat.shader, "_STRAIGHT_ALPHA_INPUT");
                        //mat.SetKeyword(keyword, true);
                        prefab = SkeletonGraphic.NewSkeletonGraphicGameObject(skelData, transform, mat).gameObject;
                        break;
                    }
            }

            prefab.name = indexName;
            return prefab;
        }
        private GameObject CreateErrorCharacter(string indexName,int initialPosIndex)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("ErrorCharacter/ErrorChr"));
            AddPreloadedCharacter(obj, indexName,initialPosIndex);
            obj.SetActive(true);
            return obj;
        }
        public void AddPreloadedCharacter(GameObject obj,string indexName,int initialPosIndex = 2)
        {
            if (!StoryPlayer.CharacterDataTable.ContainsKey(indexName))
            {
                StoryPlayer.CharacterDataTable.AddRuntimeUnit(
                    indexName,
                    obj.GetComponent<SkeletonGraphic>()
                );
            }

            obj.SetActive(false);

            obj.transform.SetParent(transform);
            obj.name = indexName;
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = new Vector3((initialPosIndex + 1) * SlotInterval, 0, 0);
            rectTransform.localScale = new Vector3(CharacterScale, CharacterScale, 1);

            SkeletonGraphic skeletonGraphic = obj.GetComponent<SkeletonGraphic>();
            skeletonGraphic.MatchRectTransformWithBounds();

            //UnityEngine.Rendering.LocalKeyword keyword = new UnityEngine.Rendering.LocalKeyword(skeletonGraphic.material.shader, "_STRAIGHT_ALPHA_INPUT");
            //skeletonGraphic.material.SetKeyword(keyword, true);
            SetAnimation(obj.GetComponent<SkeletonGraphic>(), "Idle_01", 1, true); // 呼吸轨道

            CharacterPool.Remove(indexName);
            CharacterPool.Add(indexName, obj);
        }

        public void SetAnimation(int index, string animationID, int track = 0, bool loop = true)
        {
            if (CheckIfSlotEmpty(index))
                return;
            SetAnimation(Characters[index], animationID, track, loop);
            SetWinkAction(Characters[index].name, animationID.Contains("01") ? true : false);
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

            CharacterPool.Clear();
            _characters = new SkeletonGraphic[CharacterSlotCount];
            transform.ClearAllChild();
        }
    }
}

