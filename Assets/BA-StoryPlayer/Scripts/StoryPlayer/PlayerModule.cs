using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BAStoryPlayer
{
    public abstract class PlayerModule : MonoBehaviour
    {
        [Header("Base References")]
        [SerializeField] protected BAStoryPlayer storyPlayer;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if(storyPlayer == null)
                {
                    if(transform.TryGetComponent(out BAStoryPlayer attachment))
                    {
                        if (attachment != null)
                        {
                            storyPlayer = attachment;
                            return storyPlayer;
                        }
                    }

                    foreach (var player in FindObjectsOfType<BAStoryPlayer>())
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
            if(storyPlayer == null)
            {
                if (transform.TryGetComponent(out BAStoryPlayer attachment))
                {
                    if (attachment != null)
                    {
                        storyPlayer = attachment;
                    }
                }

                if(storyPlayer == null)
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
}