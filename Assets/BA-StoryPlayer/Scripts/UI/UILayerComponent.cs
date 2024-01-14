using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer.UI
{
    public abstract class UILayerComponent : MonoBehaviour
    {
        [Header("Base References")]
        [SerializeField] protected BAStoryPlayer storyPlayer;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (storyPlayer == null)
                {
                    foreach(var player in FindObjectsOfType<BAStoryPlayer>())
                    {
                        if (transform.IsDescendantOf(player.transform))
                        {
                            storyPlayer = player;
                            break;
                        }
                    }
                }
                return storyPlayer;
            }
        }

        protected virtual void OnValidate()
        {
            if (storyPlayer == null)
            {
                foreach (var player in FindObjectsOfType<BAStoryPlayer>())
                {
                    if (transform.IsDescendantOf(player.transform))
                    {
                        storyPlayer = player;
                        break;
                    }
                }
            }
        }
    }
}