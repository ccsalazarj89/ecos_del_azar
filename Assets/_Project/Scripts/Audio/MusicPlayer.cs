using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcosDelAzar.Audio
{
    /// <summary>
    /// Reproduce la música de fondo del juego y hace un fundido cruzado al cambiar
    /// de escena. Vive como componente en el mismo GameObject persistente que GameManager
    /// (se expone en GameManager.Music).
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        [Serializable]
        public class SceneTrack
        {
            public string sceneName;
            public AudioClip clip;
        }

        [Header("Pistas por escena")]
        [Tooltip("Se usa si la escena activa no tiene una pista específica asignada abajo.")]
        [SerializeField] AudioClip defaultTrack;
        [SerializeField] SceneTrack[] tracks;

        [Header("Reproducción")]
        [Range(0f, 1f)]
        [SerializeField] float volume = 0.6f;
        [SerializeField] float crossfadeDuration = 1.5f;

        AudioSource sourceA;
        AudioSource sourceB;
        AudioSource activeSource;
        Coroutine fadeRoutine;
        AudioClip currentClip;

        void Awake()
        {
            sourceA = CreateSource();
            sourceB = CreateSource();
            activeSource = sourceA;
        }

        AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            return source;
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void Start()
        {
            PlayForScene(SceneManager.GetActiveScene().name, instant: true);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlayForScene(scene.name, instant: false);

        void PlayForScene(string sceneName, bool instant)
        {
            AudioClip clip = FindTrack(sceneName);
            if (clip == null) clip = defaultTrack;
            PlayTrack(clip, instant);
        }

        AudioClip FindTrack(string sceneName)
        {
            if (tracks == null) return null;

            foreach (var track in tracks)
            {
                if (track != null && track.sceneName == sceneName)
                    return track.clip;
            }

            return null;
        }

        /// <summary>
        /// Cambia (con fundido) a la pista indicada. Si ya es la pista activa, no hace nada.
        /// </summary>
        public void PlayTrack(AudioClip clip, bool instant = false)
        {
            if (clip == null || clip == currentClip) return;

            currentClip = clip;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);

            var incoming = activeSource == sourceA ? sourceB : sourceA;
            var outgoing = activeSource;

            incoming.clip = clip;
            incoming.volume = instant ? volume : 0f;
            incoming.Play();

            if (instant)
            {
                outgoing.Stop();
                outgoing.volume = 0f;
                activeSource = incoming;
                return;
            }

            activeSource = incoming;
            fadeRoutine = StartCoroutine(CrossfadeRoutine(outgoing, incoming));
        }

        IEnumerator CrossfadeRoutine(AudioSource outgoing, AudioSource incoming)
        {
            float t = 0f;
            float startOutVolume = outgoing.volume;

            while (t < crossfadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float ratio = Mathf.Clamp01(t / crossfadeDuration);

                outgoing.volume = Mathf.Lerp(startOutVolume, 0f, ratio);
                incoming.volume = Mathf.Lerp(0f, volume, ratio);

                yield return null;
            }

            outgoing.Stop();
            outgoing.volume = 0f;
            incoming.volume = volume;
        }

        public void SetVolume(float value)
        {
            volume = Mathf.Clamp01(value);
            if (activeSource != null)
                activeSource.volume = volume;
        }
    }
}
