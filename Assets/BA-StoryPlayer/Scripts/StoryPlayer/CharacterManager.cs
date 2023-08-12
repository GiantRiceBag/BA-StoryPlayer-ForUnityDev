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
        Jump,
        falldownR,
        Hide,
        Close,
        Back
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

        public SkeletonGraphic[] Character => character;
        public Dictionary<string, GameObject> CharacterPool => characterPool;
        BAStoryPlayer StoryPlayer => BAStoryPlayerController.Instance.StoryPlayer;

        [HideInInspector] public UnityEngine.Events.UnityEvent<float> onAnimateCharacter;

        /// <summary>
        /// �����ɫ ��Nexon��ʽ�� (���ж���,һ��Ҫ���ڶ���ǰ����)
        /// </summary>
        public void ActivateCharacter(int index,string indexName,string animationID)
        {
            int currentIndex = CheckIfCharacterOnSlot(indexName);
            // ��ɫ���ڳ���
            if (currentIndex == -1)
            {
                GameObject obj = null;
                characterPool.TryGetValue(indexName,out obj);

                // ������в�����������Ԥ���岢��ʼ���������
                if(obj == null)
                {
                    obj = CreateCharacterObj(indexName, index);
                }
                else
                {
                    obj.gameObject.SetActive(true);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector3((index + 1) * INTERVAL_SLOT, 0, 0);
                    obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                }

                // ��Ӧ��λ��������ɫ��ɾ��
                DestroyCharacter(index);
                character[index] = obj.GetComponent<SkeletonGraphic>();
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
            }

            SetAnimation(index, animationID);
            Highlight(index);
        }
        public void ActivateCharacter(int index, string indexName, string animationID, string lines)
        {
            ActivateCharacter(index, indexName, animationID);
            StoryPlayer.UIModule.SetSpeaker(indexName);
            StoryPlayer.UIModule.PrintText(lines);
        }
        public void ActivateCharacter(string indexName)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            characterPool[indexName].SetActive(true);
        }

        bool CheckIfCharacterInPool(string indexName) => characterPool.ContainsKey(indexName) ? characterPool[indexName] != null : false;
        int CheckIfCharacterOnSlot(string indexName)
        {
            for(int i = 0; i < NUM_CHARACTER_SLOT; i++)
            {
                if (Character[i] == null)
                    continue;
                if (Character[i].gameObject.name == indexName)
                    return i;
            }

            return -1;
        }
        bool CheckIfSlotEmpty(int index) => CheckIfIndexValid(index) ? Character[index] == null : false;
        bool CheckIfIndexValid(int index) => (index >= 0 && index < NUM_CHARACTER_SLOT);

        /// <summary>
        /// ����ɾ���ڳ��ϵĽ�ɫ(��ɾ�������ȡ�����)
        /// </summary>
        /// <param name="index">�±�</param>
        /// <param name="destroyObject">��ȫɾ����ɫ</param>
        void DestroyCharacter(int index,bool destroyObject = false)
        {
            if (CheckIfSlotEmpty(index))
                return;
            // ��ͣ����ɫ��գ�۶����ڱ任λ��
            SetWinkAction(Character[index].name, false);
            DestroyCharacter(Character[index].gameObject);
            character[index] = null;
        }
        void DestroyCharacter(string indexName,bool destroyObject = false)
        {
            if (!CheckIfCharacterInPool(indexName))
                return;
            Destroy(characterPool[indexName]);
        }
        void DestroyCharacter(GameObject obj,bool destoryObject = false)
        {
            if (destoryObject)
            {
                characterPool.Remove(obj.name);
                Destroy(obj);
            }
            else
            {
                obj.SetActive(false);
            }

            SetWinkAction(obj.name, false);
            int slotIndex = CheckIfCharacterOnSlot(obj.name);
            if (slotIndex != -1)
                character[slotIndex] = null;
        }

        /// <summary>
        /// ���ÿ�����ɫգ�۶���
        /// </summary>
        /// <param name="indexName">��ɫ��</param>
        /// <param name="enable">����գ��</param>
        void SetWinkAction(string indexName,bool enable)
        {
            Coroutine coroutine = null;
            winkAction.TryGetValue(indexName, out coroutine);
            try
            {
                if (enable)
                {
                    if (coroutine == null)
                    {
                        coroutine = StartCoroutine(CCharacterWink(indexName));
                        if (coroutine != null)
                            winkAction.Add(indexName, coroutine);
                    }
                }
                else
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        winkAction.Remove(indexName);
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
            // Ѱ��գ�۶��������� һ����e��ͷ
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

            yield return new WaitForSeconds(Random.Range(BAStoryPlayerController.Instance.Setting.Time_Character_Wink.x, BAStoryPlayerController.Instance.Setting.Time_Character_Wink.y));
            while (true)
            {
                skel.AnimationState.SetAnimation(0, ani_EyeClose_Name, false);
                yield return new WaitForSeconds(Random.Range(BAStoryPlayerController.Instance.Setting.Time_Character_Wink.x, BAStoryPlayerController.Instance.Setting.Time_Character_Wink.y));
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
            if (!CheckIfIndexValid(currentIndex) || !CheckIfIndexValid(targetIndex))
                return;
            if (CheckIfSlotEmpty(currentIndex))
                return;
            MoveCharacterTo(Character[currentIndex].gameObject, targetIndex, transition);
        }
        void MoveCharacterTo(int currentIndex,Vector2 pos,TransistionType transition = TransistionType.Instant)
        {
            if (CheckIfSlotEmpty(currentIndex))
                return;
            MoveCharacterTo(Character[currentIndex].gameObject, pos, transition);
        }
        public void MoveCharacterTo(string indexName, int targetIndex, TransistionType transition = TransistionType.Instant)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            characterPool[indexName].SetActive(true);
            MoveCharacterTo(characterPool[indexName], targetIndex, transition);
        }
        void MoveCharacterTo(string indexName, Vector2 pos, TransistionType transition = TransistionType.Instant)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            characterPool[indexName].SetActive(true);
            MoveCharacterTo(characterPool[indexName], pos, transition);
        }
        void MoveCharacterTo(GameObject obj, int targetIndex, TransistionType transition = TransistionType.Instant)
        {
            targetIndex = Mathf.Clamp(targetIndex, 0, 4);
            RectTransform rect = obj.GetComponent<RectTransform>();
            switch (transition)
            {
                case TransistionType.Instant:
                    {
                        rect.anchoredPosition = new Vector2((targetIndex + 1) * INTERVAL_SLOT, rect.anchoredPosition.y);
                        break;
                    }
                case TransistionType.Smooth:
                    {
                        obj.transform.DoMove_Anchored(new Vector2((targetIndex + 1) * INTERVAL_SLOT, rect.anchoredPosition.y), BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                default: break;
            }
        }
        void MoveCharacterTo(GameObject obj, Vector2 pos, TransistionType transition = TransistionType.Instant)
        {
            switch (transition)
            {
                case TransistionType.Instant:
                    {
                        obj.GetComponent<RectTransform>().anchoredPosition = pos;
                        break;
                    }
                case TransistionType.Smooth:
                    {
                        obj.transform.DoMove_Anchored(pos, BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                        break;
                    }
                default: break;
            }
        }

        /// <summary>
        /// ʹ��ɫִ��ĳ�ֶ���
        /// </summary>
        /// <param name="index">���</param>
        /// <param name="action">����</param>
        /// <param name="arg">����(��ѡ)</param>
        public void SetAction(int index,CharacterAction action,int arg = -1)
        {
            if (CheckIfSlotEmpty(index))
                return;
            SetAction(Character[index].gameObject, action, arg);
        }
        public void SetAction(string indexName,CharacterAction action,int arg = -1)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            characterPool[indexName].SetActive(true);
            SetAction(characterPool[indexName], action,arg);
        }
        public void SetAction(GameObject obj,CharacterAction action,int arg = -1)
        {
            SkeletonGraphic skelGraphic = obj.GetComponent<SkeletonGraphic>();

            switch (action)
            {
                case CharacterAction.Appear: //��ɫ��Ӱ�������
                    skelGraphic.color = Color.black;
                    skelGraphic.DoColor(Color.white, BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                    break;
                case CharacterAction.Disapper: // ��������ɫ��Ӱͬʱ�볡
                    skelGraphic.color = Color.white;
                    skelGraphic.DoColor(Color.black, BAStoryPlayerController.Instance.Setting.Time_Character_Fade).onComplete = ()=> { SetAction(obj, CharacterAction.Hide); };
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Fade);
                    break;
                case CharacterAction.Disapper2Left:
                    MoveCharacterTo(obj, new Vector2(-500, 0), TransistionType.Smooth);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                    break;
                case CharacterAction.Disapper2Right:
                    MoveCharacterTo(obj, new Vector2(2420, 0), TransistionType.Smooth);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                    break;
                case CharacterAction.AppearL2R:
                    skelGraphic.color = Color.white;
                    MoveCharacterTo(obj, new Vector2(-500, 0));
                    MoveCharacterTo(obj, arg, TransistionType.Smooth);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                    break;
                case CharacterAction.AppearR2L:
                    skelGraphic.color = Color.white;
                    MoveCharacterTo(obj, new Vector2(2420, 0));
                    MoveCharacterTo(obj, arg, TransistionType.Smooth);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                    break;
                case CharacterAction.Hophop:
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, 50), BAStoryPlayerController.Instance.Setting.Time_Character_Hophop, 2);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Hophop);
                    break;
                case CharacterAction.Greeting:
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, -70), BAStoryPlayerController.Instance.Setting.Time_Character_Greeting);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Greeting);
                    break;
                case CharacterAction.Shake:
                    obj.transform.DoShakeX(40, BAStoryPlayerController.Instance.Setting.Time_Character_Shake, 2);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Shake);
                    break;
                case CharacterAction.Move:
                    MoveCharacterTo(obj, arg, TransistionType.Smooth);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Move);
                    break;
                case CharacterAction.Stiff:
                    obj.transform.DoShakeX(10, BAStoryPlayerController.Instance.Setting.Time_Character_Stiff, 4);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Stiff);
                    break;
                case CharacterAction.Jump:
                    obj.transform.DoBound_Anchored_Relative(new Vector2(0, 70), BAStoryPlayerController.Instance.Setting.Time_Character_Jump);
                    onAnimateCharacter?.Invoke(BAStoryPlayerController.Instance.Setting.Time_Character_Jump);
                    break;
                case CharacterAction.falldownR:
                    TweenSequence sequence_Rotation = new TweenSequence();
                    sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, -10), 0.3f));
                    sequence_Rotation.Append(obj.transform.DoEuler(new Vector3(0, 0, 5), 0.5f));
                    sequence_Rotation.Wait(0.3f);
                    sequence_Rotation.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(0, 1500, 0), 0.3f));
                    sequence_Rotation.Append(() => { DestroyCharacter(obj); });

                    TweenSequence sequence_Position = new TweenSequence();
                    sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition + new Vector3(15, 0, 0), 0.3f));
                    sequence_Position.Append(obj.transform.DoLocalMove(obj.transform.localPosition - new Vector3(30, 0, 0), 0.5f));

                    onAnimateCharacter?.Invoke(1.4f);
                    break;
                case CharacterAction.Hide: // �����ý�ɫ�볡
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
            if (CheckIfSlotEmpty(index))
                return;
            SetEmotion(Character[index].gameObject,emotion);
        }
        public void SetEmotion(string indexName, CharacterEmotion emotion)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            characterPool[indexName].SetActive(true);
            SetEmotion(characterPool[indexName], emotion);
        }
        public void SetEmotion(GameObject obj,CharacterEmotion emotion)
        {
            float time = 1;
            EmotionFactory.SetEmotion(obj.transform, emotion, ref time);
            onAnimateCharacter?.Invoke(time);
        }

        /// <summary>
        /// �������ö�ĳ����ɫ
        /// </summary>
        public void Highlight(int index,TransistionType transition = TransistionType.Instant)
        {
            if (CheckIfSlotEmpty(index))
                return;
            Highlight(Character[index].gameObject, transition);
        }
        public void Highlight(string indexName, TransistionType transition = TransistionType.Instant)
        {
            if (!CheckIfCharacterInPool(indexName))
                CreateCharacterObj(indexName);
            characterPool[indexName].SetActive(true);
            Highlight(characterPool[indexName], transition);
        }
        public void Highlight(GameObject obj, TransistionType transition = TransistionType.Instant)
        {
            SkeletonGraphic skelGraphic = obj.GetComponent<SkeletonGraphic>();

            skelGraphic.color = Color.white;
            obj.transform.SetSiblingIndex(transform.childCount - 1);

            foreach (var chr in characterPool.Values)
            {
                if (chr == obj || !chr.activeSelf)
                    continue;
                SkeletonGraphic skg = chr.GetComponent<SkeletonGraphic>();
                if (skg.color == Color.black)
                    continue;

                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            skg.color = COLOR_UNHIGHLIGHT;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            skg.DoColor(COLOR_UNHIGHLIGHT, BAStoryPlayerController.Instance.Setting.Time_Character_Highlight);
                            break;
                        }
                    default: return;
                }
            }
        }

        /// <summary>
        /// �������н�ɫ
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="transition"></param>
        public void HighlightAll(TransistionType transition = TransistionType.Instant)
        {
            foreach (var chr in characterPool.Values)
            {
                if (!chr.activeSelf)
                    continue;

                SkeletonGraphic skelGraphic = chr.GetComponent<SkeletonGraphic>();
                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            skelGraphic.color = COLOR_UNHIGHLIGHT;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            skelGraphic.DoColor(COLOR_UNHIGHLIGHT, BAStoryPlayerController.Instance.Setting.Time_Character_Highlight);
                            break;
                        }
                    default: return;
                }
            }
        }

        /// <summary>
        /// ������ɫ����ʼ��
        /// </summary>
        /// <param name="indexName">������</param>
        /// <param name="index">��ɫ�±�</param>
        /// <returns></returns>
        GameObject CreateCharacterObj(string indexName,int index = 2)
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

            SetAnimation(obj.GetComponent<SkeletonGraphic>(), "Idle_01", 1, true); // �������

            characterPool.Remove(indexName);
            characterPool.Add(indexName, obj);

            return obj;
        }

        /// <summary>
        /// ����Spine����
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
            characterPool[indexName].SetActive(true);
            SetAnimation(characterPool[indexName].GetComponent<SkeletonGraphic>(),animationID,track,loop);
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
            foreach(var i in winkAction.Values)
            {
                StopCoroutine(i);
            }
            winkAction.Clear();

            characterPool.Clear();
            character = new SkeletonGraphic[NUM_CHARACTER_SLOT];
            transform.ClearAllChild();
        }
    }
}

