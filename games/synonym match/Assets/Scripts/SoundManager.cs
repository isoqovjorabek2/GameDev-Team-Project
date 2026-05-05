using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource _sfx;
    private AudioSource _music;

    private AudioClip _cFlip, _cMatch, _cMiss, _cVictory, _cGameOver, _cClick, _cCombo;

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;

        _sfx            = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;

        _music            = gameObject.AddComponent<AudioSource>();
        _music.playOnAwake = false;
        _music.loop        = true;
        _music.volume      = 0.22f;

        BuildClips();
        _music.clip = BuildMusic();
        _music.Play();
    }

    // ── Playback ────────────────────────────────────────────────────────
    public void PlayFlip()     => _sfx.PlayOneShot(_cFlip,     0.60f);
    public void PlayMatch()    => _sfx.PlayOneShot(_cMatch,    0.70f);
    public void PlayMiss()     => _sfx.PlayOneShot(_cMiss,     0.60f);
    public void PlayVictory()  => _sfx.PlayOneShot(_cVictory,  0.80f);
    public void PlayGameOver() => _sfx.PlayOneShot(_cGameOver, 0.70f);
    public void PlayClick()    => _sfx.PlayOneShot(_cClick,    0.50f);
    public void PlayCombo()    => _sfx.PlayOneShot(_cCombo,    0.65f);

    // ── Clip Construction ───────────────────────────────────────────────
    private void BuildClips()
    {
        _cClick    = Sine(880f, 0.05f, 10f, 0.40f);
        _cFlip     = Chord(new[] { 440f, 554f }, 0.11f, 7f, 0.50f);
        _cMatch    = Chord(new[] { 523f, 659f, 784f }, 0.40f, 4f, 0.60f);
        _cCombo    = Chord(new[] { 659f, 784f, 988f, 1047f }, 0.45f, 3.5f, 0.65f);
        _cMiss     = SquareChord(new[] { 200f, 165f }, 0.32f, 5f, 0.45f);
        _cVictory  = Melody(new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.13f, false);
        _cGameOver = Melody(new[] { 440f, 392f, 330f, 261f }, 0.22f, true);
    }

    private static AudioClip Sine(float freq, float dur, float decay, float vol)
    {
        const int sr = 44100;
        int n = (int)(sr * dur);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * decay) * vol;
        }
        return Clip("s", d);
    }

    private static AudioClip Chord(float[] freqs, float dur, float decay, float vol)
    {
        const int sr = 44100;
        int n = (int)(sr * dur);
        var d = new float[n];
        foreach (float f in freqs)
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                d[i] += Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * decay) * (vol / freqs.Length);
            }
        return Clip("c", d);
    }

    private static AudioClip SquareChord(float[] freqs, float dur, float decay, float vol)
    {
        const int sr = 44100;
        int n = (int)(sr * dur);
        var d = new float[n];
        foreach (float f in freqs)
            for (int i = 0; i < n; i++)
            {
                float t    = (float)i / sr;
                float wave = Mathf.Sin(2f * Mathf.PI * f * t) >= 0f ? 1f : -1f;
                d[i] += wave * Mathf.Exp(-t * decay) * (vol / freqs.Length);
            }
        return Clip("sq", d);
    }

    private static AudioClip Melody(float[] notes, float noteDur, bool square)
    {
        const int sr  = 44100;
        int       spn = (int)(sr * noteDur);
        var       d   = new float[spn * notes.Length];
        for (int ni = 0; ni < notes.Length; ni++)
        {
            float f     = notes[ni];
            int   start = ni * spn;
            for (int i = 0; i < spn; i++)
            {
                float t    = (float)i / sr;
                float env  = Mathf.Exp(-t * 6f) * 0.55f;
                float wave = square
                    ? (Mathf.Sin(2f * Mathf.PI * f * t) >= 0f ? 1f : -1f)
                    : Mathf.Sin(2f * Mathf.PI * f * t);
                d[start + i] = wave * env;
            }
        }
        return Clip("m", d);
    }

    private static AudioClip BuildMusic()
    {
        const int sr   = 44100;
        float     bpm  = 108f;
        float     beat = 60f / bpm;

        // Two-bar pentatonic arpeggio loop (C major pentatonic)
        float[] notes = {
            261.63f, 329.63f, 392f, 440f, 523.25f, 440f, 392f, 329.63f,
            261.63f, 311.13f, 392f, 466.16f, 523.25f, 466.16f, 392f, 311.13f,
        };

        float noteDur = beat * 0.5f;
        int   total   = (int)(sr * noteDur * notes.Length);
        var   d       = new float[total];

        for (int ni = 0; ni < notes.Length; ni++)
        {
            float f     = notes[ni];
            int   start = (int)(ni * noteDur * sr);
            int   len   = (int)(noteDur * 0.8f * sr);
            int   end   = Mathf.Min(start + len, total);
            for (int i = start; i < end; i++)
            {
                float t   = (float)(i - start) / sr;
                float env = Mathf.Exp(-t * 3f) * 0.13f;
                d[i] += Mathf.Sin(2f * Mathf.PI * f * t) * env;
                d[i] += Mathf.Sin(2f * Mathf.PI * f * 2f * t) * env * 0.25f;
            }
        }
        return Clip("bgm", d);
    }

    private static AudioClip Clip(string name, float[] data)
    {
        var c = AudioClip.Create(name, data.Length, 1, 44100, false);
        c.SetData(data, 0);
        return c;
    }
}
