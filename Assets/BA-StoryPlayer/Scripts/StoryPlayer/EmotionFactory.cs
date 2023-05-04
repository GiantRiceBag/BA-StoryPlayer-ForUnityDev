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
        // TODO 后期根据角色大小进行调整
        坐标暂时通用
        Heart -318  1211
        Respond -221  1156
        Music -169  1185
        Twnikle -215 1152
        Upset -307 1128
        Sweat -101 1150
        Dot -245 1125
        Chat -212 949
        Exclaim -185 1171
        Surprise -177 1112
        Question -173 1185
        Shy -266 1126
        Angry -177 1127
        */

        public static void SetEmotion(Transform target,CharacterEmotion emotion,bool sound = true)
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
                    {
                        var emo = GameObject.Instantiate(Resources.Load<GameObject>($"Emotions/Emotion_{emotion.ToString()}")).GetComponent<Emotion>();
                        emo.Initlaize(target, GetPos(emotion));
                        if (sound)
                            BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play($"Emotion/Emotion_{emotion.ToString()}");
                        break;
                    }
                case CharacterEmotion.Steam:
                case CharacterEmotion.Sigh:
                case CharacterEmotion.Sad:
                case CharacterEmotion.Bulb:
                case CharacterEmotion.Zzz:
                case CharacterEmotion.Tear:
                case CharacterEmotion.Think:
                default:
                    {
                        Debug.Log($"情绪{emotion} 没做");
                        break;
                    }
            }
        }

        // TODO 后面要删 不用通用坐标
        static Vector2 GetPos(CharacterEmotion emotion)
        {
            switch (emotion)
            {
                case CharacterEmotion.Heart:
                    return new Vector2(-318, 1211);
                case CharacterEmotion.Respond:
                    return new Vector2(-221, 1156);
                case CharacterEmotion.Music:
                    return new Vector2(-169, 1185);
                case CharacterEmotion.Twinkle:
                    return new Vector2(-215, 1152);
                case CharacterEmotion.Upset:
                    return new Vector2(-307, 1128);
                case CharacterEmotion.Sweat:
                    return new Vector2(-101, 1150);
                case CharacterEmotion.Dot:
                    return new Vector2(-245, 1125);
                case CharacterEmotion.Chat:
                    return new Vector2(-212, 949);
                case CharacterEmotion.Exclaim:
                    return new Vector2(-185, 1171);
                case CharacterEmotion.Surprise:
                    return new Vector2(-177, 1112);
                case CharacterEmotion.Question:
                    return new Vector2(-173, 1185);
                case CharacterEmotion.Shy:
                    return new Vector2(-266, 1126);
                case CharacterEmotion.Angry:
                    return new Vector2(-177, 1127);
                case CharacterEmotion.Steam:
                case CharacterEmotion.Sigh:
                case CharacterEmotion.Sad:
                case CharacterEmotion.Bulb:
                case CharacterEmotion.Zzz:
                case CharacterEmotion.Tear:
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
