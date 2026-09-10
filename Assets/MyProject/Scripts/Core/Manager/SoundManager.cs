using UnityEngine;

namespace Core.Mananger
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance {get; private set;}
        
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        
        [Header("音量設定")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float seVolume = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        // ==========================================
        // ▼▼▼ ここから下が今日の課題です！ ▼▼▼
        // ==========================================

        /// <summary>
        /// BGMを再生する
        /// </summary>
        public void PlayBgm(AudioClip clip)
        {
            // 課題①：ここにBGMを再生するプログラムを書こう！
            if (clip != null)
            {
                UpdateVolumes();
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }

        /// <summary>
        /// SE（効果音）を再生する
        /// </summary>
        public void PlaySe(AudioClip clip)
        {
            // 課題②：ここにSEを再生するプログラムを書こう！
            if (clip != null)
            {
                UpdateVolumes();
                seSource.clip = clip;
                seSource.Play();
            }
        }

        /// <summary>
        /// 音量設定が変更されたときに、実際のスピーカーの音量を更新する
        /// </summary>
        public void UpdateVolumes()
        {
            // 課題③：ここにBGMのスピーカーの音量を更新するプログラムを書こう！
            bgmSource.volume = masterVolume * bgmVolume;
            seSource.volume = masterVolume * seVolume;
        }
    }
}