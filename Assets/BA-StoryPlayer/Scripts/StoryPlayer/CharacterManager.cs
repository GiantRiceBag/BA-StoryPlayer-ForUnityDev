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

    public class CharacterManager : MonoBehaviour
    {
        const int NUM_CHARACTER_SLOT = 5;
        const float VALUE_CHARACTER_SCALE = 0.7f;
        const int VALUE_INTERVAL_SLOT = 320;
        const float TIME_TRANSITION = 1;
        Vector2 INTERVAL_WINK
        {
            get
            {
                return new Vector2(4, 5.5f);
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
        /// 激活角色
        /// </summary>
        /// <param name="index">初始位置/角色编号</param>
        /// <param name="name">角色姓名</param>
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
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    rectTransform.anchoredPosition = new Vector3((index + 1) * VALUE_INTERVAL_SLOT,0,0);
                    rectTransform.localScale = new Vector3(VALUE_CHARACTER_SCALE, VALUE_CHARACTER_SCALE, 1);
                    characterPool.Add(name, obj);
                }
                // 对象池存在对应角色则只需要修改位置即可
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
                            character[index].DoColor(Color.white, TIME_TRANSITION);
                            break;
                        }
                    default:break;
                }

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

                    MoveCharacterTo(currentIndex, index,TransistionType.Smooth);
                    character[index] = character[currentIndex];
                    character[currentIndex] = null;
                }

                // 动作替换
                character[index].AnimationState.SetAnimation(0, animationID, true);
                SetWinkAction(name, animationID == "00" ? true : false);
            }

            // TODO
            // 播放角色台词
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
                yield return new WaitForSeconds(Random.Range(INTERVAL_WINK.x, INTERVAL_WINK.y));
                while (true)
                {
                    character[index].AnimationState.SetAnimation(0, "Eye_Close_01", false);
                    yield return new WaitForSeconds(Random.Range(INTERVAL_WINK.x, INTERVAL_WINK.y));
                }
            }
        }

        /// <summary>
        /// 变更角色位置但不改变其编号
        /// </summary>
        /// <param name="currentIndex">角色编号</param>
        /// <param name="index">目标位置编号</param>
        /// <param name="transition">过渡方式</param>
        void MoveCharacterTo(int currentIndex,int index,TransistionType transition = TransistionType.Instant)
        {
            if (!ValidateIndex(currentIndex) || !ValidateIndex(index))
                return;

            switch (transition)
            {
                case TransistionType.Instant:
                    {
                        character[currentIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2((index+1) * VALUE_INTERVAL_SLOT, 0);
                        break;
                    }
                case TransistionType.Smooth:
                    {
                        // TODO 插值移动
                        character[currentIndex].transform.DMove_Anchored(new Vector2((index + 1) * VALUE_INTERVAL_SLOT, 0),0.3f).onComplete = ()=> { Debug.Log("完成!"); };
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

        bool ValidateIndex(int index)
        {
            if (index >= 0 && index < NUM_CHARACTER_SLOT)
                return true;
            Debug.LogError($"角色槽位下标 {index} 超出范围");
            return false;
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
    }
}

