using UnityEngine;

/// <summary>
/// Plays one-shot sound effects. Attach to any scene GameObject.
/// Drag AudioClip assets into the Inspector fields — every field is optional;
/// missing clips are simply skipped so the game never crashes without audio.
///
/// Free SFX sources: freesound.org, OpenGameArt, Pixabay.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum SFX { Correct, Wrong, Boost, BoostEnd, GameStart, GameOver }

    [Header("Answer SFX")]
    [Tooltip("Short chime — played when a player picks the correct gate.")]
    public AudioClip correctClip;

    [Tooltip("Buzzer / negative sting — wrong gate.")]
    public AudioClip wrongClip;

    [Header("Speed SFX")]
    [Tooltip("Swoosh / acceleration — speed boost begins.")]
    public AudioClip boostClip;

    [Tooltip("Low-down whoosh — boost expires.")]
    public AudioClip boostEndClip;

    [Header("Game State SFX")]
    [Tooltip("Short countdown beep / start jingle.")]
    public AudioClip gameStartClip;

    [Tooltip("Fanfare or dramatic sting — game over.")]
    public AudioClip gameOverClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume    = 1f;

    AudioSource _sfxSource;
    AudioSource _loopSource; // reserved for future looping music

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _sfxSource  = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _loopSource = gameObject.AddComponent<AudioSource>();
        _loopSource.playOnAwake = false;
        _loopSource.loop        = true;
    }

    public void Play(SFX sfx)
    {
        AudioClip clip = sfx switch
        {
            SFX.Correct   => correctClip,
            SFX.Wrong     => wrongClip,
            SFX.Boost     => boostClip,
            SFX.BoostEnd  => boostEndClip,
            SFX.GameStart => gameStartClip,
            SFX.GameOver  => gameOverClip,
            _             => null
        };

        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    /// <summary>Play a clip at a specific world position (e.g. a gate).</summary>
    public void PlayAtPoint(SFX sfx, Vector3 worldPos)
    {
        AudioClip clip = sfx switch
        {
            SFX.Correct   => correctClip,
            SFX.Wrong     => wrongClip,
            SFX.Boost     => boostClip,
            SFX.BoostEnd  => boostEndClip,
            SFX.GameStart => gameStartClip,
            SFX.GameOver  => gameOverClip,
            _             => null
        };

        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPos, masterVolume * sfxVolume);
    }

    /// <summary>Start a looping background track (pass null to stop).</summary>
    public void PlayLoop(AudioClip music)
    {
        if (music == null) { _loopSource.Stop(); return; }
        if (_loopSource.clip == music && _loopSource.isPlaying) return;
        _loopSource.clip = music;
        _loopSource.volume = masterVolume * 0.4f;
        _loopSource.Play();
    }
}
