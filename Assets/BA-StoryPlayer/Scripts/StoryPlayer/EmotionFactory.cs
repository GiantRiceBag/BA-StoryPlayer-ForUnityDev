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

    public static class EmotionFactory
    {
        /*
        // 白子各表情坐标 定位器下
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
        private static  Vector3 FacePositionOffset =>new Vector3(0,-35f,0);
        private static System.Collections.Generic.Dictionary<CharacterEmotion, GameObject> emotionCache= new System.Collections.Generic.Dictionary<CharacterEmotion, GameObject>();
        
        // TODO 表情全做完后可以删掉Case了
        public static void SetEmotion(Transform target,CharacterEmotion emotion,ref float time)
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
                        Transform HeadLocator = target.Find("HeadLocator");
                        if (HeadLocator == null)
                            HeadLocator = GenerateHeadLocator(target).transform;
                        if (HeadLocator == null) throw new System.NullReferenceException($"Can't locate face position of character[{target.name}]");

                        try
                        {
                            if (!emotionCache.ContainsKey(emotion))
                                emotionCache.Add(emotion, Resources.Load<GameObject>($"Emotions/Emotion_{emotion.ToString()}"));

                            var emo = GameObject.Instantiate(emotionCache[emotion]).GetComponent<Emotion>();
                            emo.Initlaize(HeadLocator, GetPos(emotion));
                            time = emo.GetClipLength();
                            BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play($"Emotion/Emotion_{emotion.ToString()}");
                        }
                        catch
                        {
                            return;
                        }
                        break;
                    }
                case CharacterEmotion.Think:
                default:
                    {
                        Debug.Log($"Emoji {emotion} 没做");
                        break;
                    }
            }
        }

        /// <summary>
        /// 获取表情在脸部位置的相对坐标
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        private static Vector2 GetPos(CharacterEmotion emotion)
        {
            switch (emotion)
            {
                case CharacterEmotion.Heart:
                    return new Vector2(-313, 221);
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

        private static GameObject GenerateHeadLocator(Transform target)
        {
            string indexName = target.name;
            CharacterDataUnit chrData = BAStoryPlayerController.Instance.CharacterDataTable[indexName];
            if (chrData.loadType == LoadType.Prefab)
                return null;

            GameObject obj = new GameObject("HeadLocator",typeof(RectTransform));
            RectTransform rectOfParent = target.GetComponent<RectTransform>();
            RectTransform rect = obj.GetComponent<RectTransform>();
            obj.transform.SetParent(target);
            obj.transform.localScale = Vector3.one;
            rect.anchorMin = rect.anchorMax = rectOfParent.pivot;
            rect.anchoredPosition = chrData.facePosition;

            return obj;
        }

        public static void ClearCache()
        {
            emotionCache.Clear();
        }
    }
}
