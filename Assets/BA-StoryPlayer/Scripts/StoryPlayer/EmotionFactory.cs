using UnityEngine;
using BAStoryPlayer.UI;

namespace BAStoryPlayer
{
    public enum CharacterEmotion
    {
        Heart = 0,
        Respond,
        Music,
        Twinkle,
        Upset,
        Sweat,
        Dot,
        Chat,
        Exclaim,
        Surprise,
        Question,
        Shy,
        Angry,
        Steam,
        Sigh,
        Sad,
        Bulb,
        Zzz,
        Tear,
        Think
    }

    public enum LocateMode
    {
        Auto,
        Manual
    }

    public static class EmotionFactory
    {
        /*
       // 白子各表情坐标 自动
       Heart 301 1989
       Respond 326 1951
       Music 461 1968
       Twnikle 401 2015
       Upset 341 2044
       Sweat 484 1999
       Dot 317 2094
       Chat 414 1751
       Exclaim 372 2049
       Surprise 448 2071
       Question 405 2067
       Shy 311 2098
       Angry 453 2036
        Steam 314 1962
        Sigh 388 1745
        Sad 479 1990
        Bulb 298 2104
        Zzz 298 2061
        Tear 399 1896
       */
        /*
        // 白子各表情坐标 手动带定位器
        Heart -313 221
        Respond -268 165
        Music -157 165
        Twnikle -153 152
        Upset -319 223
        Sweat -118 158
        Dot -318 224
        Chat -244 -90
        Exclaim -199 188
        Surprise -163 151
        Question -179 165
        Shy -317 225
        Angry -120 133
        Steam -290 -20
        Sigh -199 -159
        Sad -151 149
        Bulb -313 226
        Zzz -296 208
        Tear -202 -36
        */
        static Vector2 ReferenceSize  { get { return new Vector2(1125.85f, 2271.2f); } } // 标准化参考人物尺寸（取自白子的标准大小）

        // TODO 表情全做完后可以删掉Case了
        public static void SetEmotion(Transform target,CharacterEmotion emotion,LocateMode locateMode)
        {
            switch (emotion)
            {
                case CharacterEmotion.Heart:
                case CharacterEmotion.Respond:
                case CharacterEmotion.Music:
                case CharacterEmotion.Twinkle:
                case CharacterEmotion.Upset:
                case CharacterEmotion.Sweat:
                case CharacterEmotion.Dot:
                case CharacterEmotion.Chat:
                case CharacterEmotion.Exclaim:
                case CharacterEmotion.Surprise:
                case CharacterEmotion.Question:
                case CharacterEmotion.Shy:
                case CharacterEmotion.Angry:
                case CharacterEmotion.Steam:
                case CharacterEmotion.Sigh:
                case CharacterEmotion.Sad:
                case CharacterEmotion.Bulb:
                case CharacterEmotion.Zzz:
                case CharacterEmotion.Tear:
                    {
                        var emo = GameObject.Instantiate(Resources.Load<GameObject>($"Emotions/Emotion_{emotion.ToString()}")).GetComponent<Emotion>();
                        emo.Initlaize(target, GetPos(emotion,locateMode),locateMode);
                        BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play($"Emotion/Emotion_{emotion.ToString()}");
                        break;
                    }
                case CharacterEmotion.Think:
                default:
                    {
                        Debug.Log($"情绪{emotion} 没做");
                        break;
                    }
            }
        }

        // TODO 暂时使用手动坐标
        /// <summary>
        /// 获取表情的标准化坐标
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        static Vector2 GetPos(CharacterEmotion emotion,LocateMode locateMode)
        {
            if(locateMode == LocateMode.Auto)
            {
                switch (emotion)
                {
                    case CharacterEmotion.Heart:
                        return new Vector2(301, 1989) / ReferenceSize;
                    case CharacterEmotion.Respond:
                        return new Vector2(326, 1951) / ReferenceSize;
                    case CharacterEmotion.Music:
                        return new Vector2(461, 1968) / ReferenceSize;
                    case CharacterEmotion.Twinkle:
                        return new Vector2(401, 2015) / ReferenceSize;
                    case CharacterEmotion.Upset:
                        return new Vector2(341, 2044) / ReferenceSize;
                    case CharacterEmotion.Sweat:
                        return new Vector2(484, 1999) / ReferenceSize;
                    case CharacterEmotion.Dot:
                        return new Vector2(317, 2094) / ReferenceSize;
                    case CharacterEmotion.Chat:
                        return new Vector2(400, 1751) / ReferenceSize;
                    case CharacterEmotion.Exclaim:
                        return new Vector2(372, 2049) / ReferenceSize;
                    case CharacterEmotion.Surprise:
                        return new Vector2(448, 2071) / ReferenceSize;
                    case CharacterEmotion.Question:
                        return new Vector2(405, 2067) / ReferenceSize;
                    case CharacterEmotion.Shy:
                        return new Vector2(311, 2098) / ReferenceSize;
                    case CharacterEmotion.Angry:
                        return new Vector2(453, 2036) / ReferenceSize;
                    case CharacterEmotion.Steam:
                        return new Vector2(314, 1962) / ReferenceSize;
                    case CharacterEmotion.Sigh:
                        return new Vector2(388, 1745) / ReferenceSize;
                    case CharacterEmotion.Sad:
                        return new Vector2(479, 1990) / ReferenceSize;
                    case CharacterEmotion.Bulb:
                        return new Vector2(298, 2104) / ReferenceSize;
                    case CharacterEmotion.Zzz:
                        return new Vector2(298, 2061) / ReferenceSize;
                    case CharacterEmotion.Tear:
                        return new Vector2(399, 1896) / ReferenceSize;
                    case CharacterEmotion.Think:
                    default:
                        {
                            Debug.Log($"情绪{emotion} 没做");
                            return Vector2.zero;
                        }
                }
            }
            else
            {
                switch (emotion)
                {
                    case CharacterEmotion.Heart:
                        return new Vector2(-313,221);
                    case CharacterEmotion.Respond:
                        return new Vector2(-268, 165);
                    case CharacterEmotion.Music:
                        return new Vector2(-157, 165);
                    case CharacterEmotion.Twinkle:
                        return new Vector2(-153, 152);
                    case CharacterEmotion.Upset:
                        return new Vector2(-319, 223);
                    case CharacterEmotion.Sweat:
                        return new Vector2(-118, 158);
                    case CharacterEmotion.Dot:
                        return new Vector2(-318, 224);
                    case CharacterEmotion.Chat:
                        return new Vector2(-224, -90);
                    case CharacterEmotion.Exclaim:
                        return new Vector2(-199, 188);
                    case CharacterEmotion.Surprise:
                        return new Vector2(-163, 151);
                    case CharacterEmotion.Question:
                        return new Vector2(-179, 165);
                    case CharacterEmotion.Shy:
                        return new Vector2(-317, 225);
                    case CharacterEmotion.Angry:
                        return new Vector2(-120, 133);
                    case CharacterEmotion.Steam:
                        return new Vector2(-290, -20);
                    case CharacterEmotion.Sigh:
                        return new Vector2(-199, -159);
                    case CharacterEmotion.Sad:
                        return new Vector2(-151, 149);
                    case CharacterEmotion.Bulb:
                        return new Vector2(-313, 226);
                    case CharacterEmotion.Zzz:
                        return new Vector2(-296, 208);
                    case CharacterEmotion.Tear:
                        return new Vector2(-202, -36);
                    case CharacterEmotion.Think:
                    default:
                        {
                            Debug.Log($"情绪{emotion} 没做");
                            return Vector2.zero;
                        }
                }
            }
        }
    }
}
