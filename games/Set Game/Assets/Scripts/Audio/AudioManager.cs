using System.Collections;
using UnityEngine;
using SetGame.Core;

namespace SetGame.Audio
{
    /// <summary>
    /// Synthesizes all sound effects procedurally — no audio files required.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Range(0, 1)] public float MasterVolume = 0.7f;
        [Range(0, 1)] public float MusicVolume  = 0.3f;
        [Range(0, 1)] public float SFXVolume    = 0.8f;

        AudioSource _musicSource;
        AudioSource _sfxSource;

        // Synth constants
        const int   SAMPLE_RATE = 44100;
        const float TAU         = Mathf.PI * 2f;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop   = true;
            _musicSource.volume = MusicVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.volume = SFXVolume;
        }

        void OnEnable()
        {
            GameEvents.OnGameStarted        += OnGameStarted;
            GameEvents.OnGameOver           += OnGameOver;
            GameEvents.OnValidSetFound      += _ => PlaySuccess();
            GameEvents.OnInvalidSetAttempt  += _ => PlayError();
            GameEvents.OnCardSelected       += _ => PlayClick();
            GameEvents.OnHighScoreBeaten    += _ => PlayFanfare();
        }

        void OnDisable()
        {
            GameEvents.OnGameStarted        -= OnGameStarted;
            GameEvents.OnGameOver           -= OnGameOver;
            GameEvents.OnValidSetFound      -= _ => PlaySuccess();
            GameEvents.OnInvalidSetAttempt  -= _ => PlayError();
            GameEvents.OnCardSelected       -= _ => PlayClick();
            GameEvents.OnHighScoreBeaten    -= _ => PlayFanfare();
        }

        void OnGameStarted() => StartAmbientMusic();
        void OnGameOver()    => StartCoroutine(FadeOutMusic(1.5f));

        // ─── Music ────────────────────────────────────────────────────────────────

        void StartAmbientMusic()
        {
            _musicSource.clip   = CreateAmbientLoop();
            _musicSource.volume = MusicVolume * MasterVolume;
            _musicSource.Play();
        }

        // Generates a gentle arpeggiated ambient loop (Am pentatonic)
        AudioClip CreateAmbientLoop()
        {
            float bpm      = 72f;
            float beatDur  = 60f / bpm;
            float totalDur = beatDur * 32f;
            int   samples  = (int)(SAMPLE_RATE * totalDur);
            var   data     = new float[samples];

            float[] noteFreqs = { 220f, 261.63f, 293.66f, 329.63f, 392f, 440f, 523.25f };
            int noteIdx = 0;
            float nextNoteTime = 0;

            for (int s = 0; s < samples; s++)
            {
                float t = s / (float)SAMPLE_RATE;
                if (t >= nextNoteTime)
                {
                    noteIdx = (noteIdx + 1) % noteFreqs.Length;
                    nextNoteTime = t + beatDur * 0.5f;
                }

                float freq = noteFreqs[noteIdx];
                float env  = Mathf.Exp(-((t - nextNoteTime + beatDur * 0.5f) / (beatDur * 0.3f)));
                data[s] += Mathf.Sin(TAU * freq * t) * env * 0.12f;

                // Soft pad undertone
                data[s] += Mathf.Sin(TAU * 110f * t) * 0.03f;
                data[s] += Mathf.Sin(TAU * 165f * t) * 0.02f;
            }

            NormalizeAudio(data, 0.4f);
            var clip = AudioClip.Create("Ambient", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        IEnumerator FadeOutMusic(float dur)
        {
            float startVol = _musicSource.volume;
            float t = 0;
            while (t < dur)
            {
                _musicSource.volume = Mathf.Lerp(startVol, 0, t / dur);
                t += Time.deltaTime;
                yield return null;
            }
            _musicSource.Stop();
        }

        // ─── SFX ──────────────────────────────────────────────────────────────────

        public void PlayClick()
        {
            PlaySFX(CreateToneClip(600f, 0.06f, WaveType.Triangle, 0.3f));
        }

        public void PlaySuccess()
        {
            // Ascending major arpeggio: C E G high-C
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            StartCoroutine(PlayArpeggio(notes, 0.08f, 0.18f));
        }

        public void PlayError()
        {
            PlaySFX(CreateToneClip(150f, 0.22f, WaveType.Sawtooth, 0.4f, pitchBend: -0.3f));
        }

        public void PlayFanfare()
        {
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
            StartCoroutine(PlayArpeggio(notes, 0.07f, 0.22f));
        }

        public void PlayGameOver()
        {
            float[] notes = { 440f, 392f, 349.23f, 293.66f };
            StartCoroutine(PlayArpeggio(notes, 0.12f, 0.3f));
        }

        IEnumerator PlayArpeggio(float[] freqs, float step, float noteDur)
        {
            foreach (float f in freqs)
            {
                PlaySFX(CreateToneClip(f, noteDur, WaveType.Triangle, 0.35f));
                yield return new WaitForSeconds(step);
            }
        }

        void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            _sfxSource.volume = SFXVolume * MasterVolume;
            _sfxSource.PlayOneShot(clip);
        }

        // ─── Synthesis ────────────────────────────────────────────────────────────

        enum WaveType { Sine, Triangle, Sawtooth, Square }

        AudioClip CreateToneClip(float freq, float dur, WaveType wave, float volume,
                                  float pitchBend = 0f)
        {
            int   n    = (int)(SAMPLE_RATE * dur);
            var   data = new float[n];
            float attack  = 0.01f;
            float release = 0.3f;

            for (int i = 0; i < n; i++)
            {
                float t     = i / (float)SAMPLE_RATE;
                float phase = TAU * freq * t * (1f + pitchBend * t / dur);

                float sample = wave switch
                {
                    WaveType.Triangle  => Mathf.PingPong(phase / Mathf.PI, 1f) * 2f - 1f,
                    WaveType.Sawtooth  => ((phase % TAU) / TAU) * 2f - 1f,
                    WaveType.Square    => Mathf.Sign(Mathf.Sin(phase)),
                    _                  => Mathf.Sin(phase)
                };

                float env = t < attack ? t / attack
                           : t > dur - release ? (dur - t) / release : 1f;

                data[i] = sample * env * volume;
            }

            NormalizeAudio(data, 0.7f);
            var clip = AudioClip.Create($"SFX_{freq:F0}", n, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        static void NormalizeAudio(float[] data, float target)
        {
            float peak = 0;
            foreach (float s in data) if (Mathf.Abs(s) > peak) peak = Mathf.Abs(s);
            if (peak > 0.0001f)
            {
                float scale = target / peak;
                for (int i = 0; i < data.Length; i++) data[i] *= scale;
            }
        }
    }
}
