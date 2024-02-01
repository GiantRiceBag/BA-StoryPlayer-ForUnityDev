using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer
{
    public class AudioManager : PlayerModule
    {
        private int _playerID = 0;

        private List<AudioSource> _playingPool = new List<AudioSource>();
        private Queue<AudioSource> _sourcePool = new Queue<AudioSource>();

        private AudioSource _sourceBGM;
        private AudioSource SourceBGM
        {
            get
            {
                if (_sourceBGM == null)
                {
                    _sourceBGM = gameObject.AddComponent<AudioSource>();
                    _sourceBGM.loop = true;
                    _sourceBGM.volume = VolumeMusic;
                    _sourceBGM.spatialBlend = 0;
                }

                return _sourceBGM;
            }
        }

        [SerializeField,Range(0, 1f)] private float _volumeMaster = 1;
        [SerializeField,Range(0, 1f)] private float _volumeMusic = 0.6f;
        [SerializeField,Range(0, 1f)] private float _volumeSound = 1;

        [SerializeField] private bool _isMute = false;
        protected bool IsMute
        {
            set
            {
                if (_isMute != value)
                {
                    _isMute = value;
                }
            }

            get
            {
                return _isMute;
            }
        }

        private Dictionary<string, AudioClip> _preloadedMusicClips = new();

        public float VolumeMaster
        {
            set
            {
                _volumeMaster = Mathf.Clamp(value,0,1);

                VolumeMusic = _volumeMusic;
                VolumeSound = _volumeSound;
            }
            get
            {
                return _volumeMaster;
            }
        }
        public float VolumeMusic
        {
            set
            {
                _volumeMusic = Mathf.Clamp(value, 0, 1);

                SourceBGM.volume = VolumeMusic;
            }
            get
            {
                return _volumeMusic * _volumeMaster * (IsMute ? 0 : 1);
            }
        }
        public float VolumeSound
        {
            set
            {
                _volumeSound = Mathf.Clamp(value, 0, 1);

                foreach (var i in _playingPool)
                {
                    i.volume = VolumeSound;
                }
            }
            get
            {
                return _volumeSound * _volumeMaster * (IsMute ? 0 : 1);
            }
        }

        public Dictionary<string, AudioClip> PreloadedMusicClips => _preloadedMusicClips;

        public void PlayBGM(string clipName, bool fade = true, float fadeScale = 2)
        {
            if (PreloadedMusicClips.ContainsKey(clipName))
            {
                PlayBGM(PreloadedMusicClips[clipName],fade,fadeScale);
            }
            else
            {
                AudioClip clip = Resources.Load<AudioClip>(StoryPlayer.Setting.PathMusic(clipName));
                if (clip == null)
                {
                    Debug.LogError($"未能在路径 [{StoryPlayer.Setting.PathMusic(clipName)}] 找到AudioClip");
                    return;
                }
                PreloadedMusicClips.Add(clipName, clip);
                PlayBGM(clip, fade, fadeScale);
            }
        }
        private void PlayBGM(AudioClip audioClip, bool fade, float fadeScale)
        {
            if(SourceBGM.isPlaying && audioClip == SourceBGM.clip)
            {
                return;
            }

            if (fade)
            {
                if (SourceBGM.clip == null)
                {
                    SourceBGM.Stop();
                    SourceBGM.volume = 0;
                    SourceBGM.clip = audioClip;
                    SourceBGM.Play();
                    SourceBGM.DoVolume(VolumeMusic, StoryPlayer.Setting.TimeBgmFade * fadeScale);
                }
                else
                {
                    SourceBGM.DoVolume(0, StoryPlayer.Setting.TimeBgmFade * fadeScale).onCompleted = () =>
                    {
                        SourceBGM.Stop();
                        SourceBGM.clip = audioClip;
                        SourceBGM.Play();
                        SourceBGM.DoVolume(VolumeMusic, StoryPlayer.Setting.TimeBgmFade * fadeScale);
                    };
                }
            }
            else
            {
                SourceBGM.clip = audioClip;
                SourceBGM.volume = VolumeMusic;
                SourceBGM.Play();
            }
        }
        public void PauseBGM(bool fade = true, float fadeScale = 2)
        {
            if (!SourceBGM.isPlaying)
                return;

            if (fade)
            {
                SourceBGM.DoVolume(0, StoryPlayer.Setting.TimeBgmFade * fadeScale).onCompleted = () =>
                {
                    SourceBGM.Pause();
                };
            }
            else
            {
                SourceBGM.Pause();
            }
        }

        public int Play(string clipName, bool isOneShot = true, float scale = 1)
        {
            AudioClip clip = Resources.Load<AudioClip>(StoryPlayer.Setting.PathSound(clipName));
            if (clip == null)
            {
                Debug.LogError($"未能在路径 [{StoryPlayer.Setting.PathSound(clipName)}] 找到AudioClip");
                return -1;
            }
                
            return Play(clip, isOneShot, scale);
        }
        public int Play(AudioClip audioClip, bool isOneShot = true, float scale = 1)
        {
            int id = _playerID++;
            AudioSource source = GetSource();

            //source.volume = isOneShot ? Volume_Sound : Volume_Music;
            source.volume = VolumeSound;
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
            if (_sourcePool.Count >= StoryPlayer.Setting.NumMaxAudioSource)
            {
                Destroy(source.gameObject);
            }
            else
            {
                source.clip = null;
                _sourcePool.Enqueue(source);
            }

            _playingPool.Remove(source);
        }

        private AudioSource GetSource(int playerID)
        {
            return transform.Find(playerID.ToString()).GetComponent<AudioSource>();
        }
        private AudioSource GetSource()
        {
            AudioSource source;

            if (_sourcePool.Count != 0)
            {
                source = _sourcePool.Dequeue();
            }
            else
            {
                GameObject go = new GameObject("AudioSource");
                go.hideFlags = HideFlags.HideInHierarchy;
                go.transform.SetParent(transform);

                source = go.AddComponent<AudioSource>();
                source.spatialBlend = 0;
            }

            _playingPool.Add(source);
            return source;
        }

        public void ClearAll()
        {
            for(int i = _playingPool.Count - 1; i >= 0; i--)
            {
                Destroy(_playingPool[i].gameObject);
            }
            SourceBGM.clip = null;
        }
        public void ClearPreloadedMusicClips()
        {
            PreloadedMusicClips.Clear();
        }

        public void PlaySoundButtonClick()
        {
            Play("Button_Click");
        }
    }
}
