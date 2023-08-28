using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer
{
    public class AudioManager : MonoBehaviour
    {
        private int playerID = 0;

        private List<AudioSource> playingPool = new List<AudioSource>();
        private Queue<AudioSource> sourcePool = new Queue<AudioSource>();

        private AudioSource sourceBGM;
        private AudioSource SourceBGM
        {
            get
            {
                if (sourceBGM == null)
                {
                    sourceBGM = gameObject.AddComponent<AudioSource>();
                    sourceBGM.loop = true;
                    sourceBGM.volume = Volume_Music;
                    sourceBGM.spatialBlend = 0;
                }

                return sourceBGM;
            }
        }

        [SerializeField,Range(0, 1f)] private float mVolume_Master = 1;
        [SerializeField,Range(0, 1f)] private float mVolume_Music = 0.6f;
        [SerializeField,Range(0, 1f)] private float mVolume_Sound = 1;

        [SerializeField] private bool isMute = false;
        protected bool IsMute
        {
            set
            {
                if (isMute != value)
                {
                    isMute = value;
                }
            }

            get
            {
                return isMute;
            }
        }

        public float Volume_Master
        {
            set
            {
                mVolume_Master = Mathf.Clamp(value,0,1);

                Volume_Music = mVolume_Music;
                Volume_Sound = mVolume_Sound;
            }
            get
            {
                return mVolume_Master;
            }
        }
        public float Volume_Music
        {
            set
            {
                mVolume_Music = Mathf.Clamp(value, 0, 1);

                SourceBGM.volume = Volume_Music;
            }
            get
            {
                return mVolume_Music * mVolume_Master * (IsMute ? 0 : 1);
            }
        }
        public float Volume_Sound
        {
            set
            {
                mVolume_Sound = Mathf.Clamp(value, 0, 1);

                foreach (var i in playingPool)
                {
                    i.volume = Volume_Sound;
                }
            }
            get
            {
                return mVolume_Sound * mVolume_Master * (IsMute ? 0 : 1);
            }
        }

        public void PlayBGM(string audioURL, bool fade = true, float fadeScale = 2)
        {
            AudioClip clip = Resources.Load<AudioClip>(BAStoryPlayerController.Instance.Setting.Path_Music + audioURL);
            if (clip == null)
                Debug.LogError($"未能在路径 [{BAStoryPlayerController.Instance.Setting.Path_Music + audioURL}] 找到AudioClip");
            PlayBGM(clip, fade, fadeScale);
        }
        public void PlayBGM(AudioClip audioClip, bool fade = true, float fadeScale = 2)
        {
            if (fade)
            {
                if (SourceBGM.clip == null)
                {
                    SourceBGM.Stop();
                    SourceBGM.volume = 0;
                    SourceBGM.clip = audioClip;
                    SourceBGM.Play();
                    SourceBGM.DoVolume(Volume_Music, BAStoryPlayerController.Instance.Setting.Time_Bgm_Fade * fadeScale);
                }
                else
                {
                    SourceBGM.DoVolume(0, BAStoryPlayerController.Instance.Setting.Time_Bgm_Fade * fadeScale).onComplete = () =>
                    {
                        SourceBGM.Stop();
                        SourceBGM.clip = audioClip;
                        SourceBGM.Play();
                        SourceBGM.DoVolume(Volume_Music, BAStoryPlayerController.Instance.Setting.Time_Bgm_Fade * fadeScale);
                    };
                }
            }
            else
            {
                SourceBGM.clip = audioClip;
                SourceBGM.volume = Volume_Music;
                SourceBGM.Play();
            }
        }
        public void PauseBGM(bool fade = true, float fadeScale = 2)
        {
            if (!SourceBGM.isPlaying)
                return;

            if (fade)
            {
                SourceBGM.DoVolume(0, BAStoryPlayerController.Instance.Setting.Time_Bgm_Fade * fadeScale).onComplete = () =>
                {
                    SourceBGM.Pause();
                };
            }
            else
            {
                SourceBGM.Pause();
            }
        }

        public int Play(string audioURL, bool isOneShot = true, float scale = 1)
        {
            AudioClip clip = Resources.Load<AudioClip>(BAStoryPlayerController.Instance.Setting.Path_Sound + audioURL);
            if (clip == null)
                Debug.LogError($"未能在路径 [{BAStoryPlayerController.Instance.Setting.Path_Sound + audioURL}] 找到AudioClip");
            return Play(clip, isOneShot, scale);
        }
        public int Play(AudioClip audioClip, bool isOneShot = true, float scale = 1)
        {
            int id = playerID++;
            AudioSource source = GetSource();

            //source.volume = isOneShot ? Volume_Sound : Volume_Music;
            source.volume = Volume_Sound;
            source.name = id.ToString();
            source.loop = !isOneShot;

            if (isOneShot)
            {
                source.PlayOneShot(audioClip, scale); ;
                ReleaseSource(source, audioClip.length);
            }
            else
            {
                source.clip = audioClip;
                source.Play();
            }

            return id;
        }

        private void ReleaseSource(AudioSource source, float time)
        {
            StartCoroutine(CReleaseSource(source, time));
        }
        private IEnumerator CReleaseSource(AudioSource source, float time)
        {
            yield return new WaitForSeconds(time);
            ReleaseSource(source);
        }
        private void ReleaseSource(AudioSource source)
        {
            if (sourcePool.Count >= BAStoryPlayerController.Instance.Setting.Num_Max_AudioSource)
            {
                Destroy(source.gameObject);
            }
            else
            {
                source.clip = null;
                sourcePool.Enqueue(source);
            }

            playingPool.Remove(source);
        }

        private AudioSource GetSource(int playerID)
        {
            return transform.Find(playerID.ToString()).GetComponent<AudioSource>();
        }
        private AudioSource GetSource()
        {
            AudioSource source;

            if (sourcePool.Count != 0)
            {
                source = sourcePool.Dequeue();
            }
            else
            {
                GameObject go = new GameObject("AudioSource");
                go.hideFlags = HideFlags.HideInHierarchy;
                go.transform.SetParent(transform);

                source = go.AddComponent<AudioSource>();
                source.spatialBlend = 0;
            }

            playingPool.Add(source);
            return source;
        }

        public void ClearAll()
        {
            for(int i = playingPool.Count - 1; i >= 0; i--)
            {
                Destroy(playingPool[i].gameObject);
            }
            SourceBGM.clip = null;
        }
    }
}
