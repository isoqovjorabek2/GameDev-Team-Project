using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State      { Menu, Playing, GameOver, Victory }
    public enum Difficulty { Easy, Medium, Hard }

    public State      CurrentState      { get; private set; } = State.Menu;
    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Medium;

    public int   Lives        { get; private set; }
    public int   Score        { get; private set; }
    public int   Combo        { get; private set; }
    public float Timer        { get; private set; }
    public int   MatchesFound { get; private set; }
    public int   TotalPairs   { get; private set; }
    public int   BestScore    { get; private set; }

    private bool _ticking;

    public event Action<State> OnStateChanged;
    public event Action        OnStatsUpdated;

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!_ticking) return;
        Timer += Time.deltaTime;
        OnStatsUpdated?.Invoke();
    }

    public void SetDifficulty(Difficulty d) => CurrentDifficulty = d;

    public (int cols, int rows) GridSize() => CurrentDifficulty switch
    {
        Difficulty.Easy   => (4, 3),
        Difficulty.Medium => (4, 4),
        Difficulty.Hard   => (6, 5),
        _                 => (4, 4),
    };

    public void StartGame()
    {
        var (c, r) = GridSize();
        Lives        = 3;
        Score        = 0;
        Combo        = 0;
        Timer        = 0f;
        MatchesFound = 0;
        TotalPairs   = c * r / 2;
        _ticking     = true;
        SetState(State.Playing);
    }

    public void RecordMatch()
    {
        Combo++;
        Score += Mathf.RoundToInt(100 * (1f + (Combo - 1) * 0.5f));
        MatchesFound++;
        OnStatsUpdated?.Invoke();
        if (MatchesFound >= TotalPairs) StartCoroutine(CoVictory());
    }

    public void RecordMiss()
    {
        Combo = 0;
        Lives--;
        OnStatsUpdated?.Invoke();
        if (Lives <= 0) StartCoroutine(CoGameOver());
    }

    public void ReturnToMenu()
    {
        _ticking = false;
        SetState(State.Menu);
    }

    private IEnumerator CoVictory()
    {
        yield return new WaitForSeconds(0.8f);
        _ticking = false;
        // Time bonus: up to 1500 pts for completing under 300 s
        Score += Mathf.Max(0, Mathf.RoundToInt((300f - Timer) * 5f));
        if (Score > BestScore) BestScore = Score;
        OnStatsUpdated?.Invoke();
        SetState(State.Victory);
    }

    private IEnumerator CoGameOver()
    {
        yield return new WaitForSeconds(1.3f);
        _ticking = false;
        SetState(State.GameOver);
    }

    private void SetState(State s)
    {
        CurrentState = s;
        OnStateChanged?.Invoke(s);
    }
}
