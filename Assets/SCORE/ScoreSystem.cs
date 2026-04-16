using System;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    public int Coins;
    public int Score { get; private set; }

    public int MaxScore { get; private set; }
    public const string PREF_MAX_SCORE = "MaxScore";

    public event Action OnScoreChanged; // opcional para refrescar UI

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MaxScore = PlayerPrefs.GetInt(PREF_MAX_SCORE, 0);
    }

    /// <summary>Reinicia monedas y score de la corrida actual (si lo necesitas al iniciar partida).</summary>
    public void ResetRun()
    {
        Coins = 0;
        Score = 0;
        OnScoreChanged?.Invoke();
    }

    public void AwardKillCustomer()        // +2 monedas y +2 al score
    {
        AddCoins(2, alsoAddToScore: true);
    }

    public void AwardDeliverySuccess()     // +5 monedas y +5 al score
    {
        AddCoins(5, alsoAddToScore: true);
    }

    public void AddCoins(int amount, bool alsoAddToScore)
    {
        if (amount <= 0) return;

        Coins += amount;
        if (alsoAddToScore)
        {
            Score += amount;
            UpdateMaxScoreIfNeeded();
        }

        OnScoreChanged?.Invoke();
    }
    
    public void AddCScore(int amount)
    {
        if (amount <= 0) return;

        Score += amount;
       

        OnScoreChanged?.Invoke();
    }
    public bool HasEnoughCoins(int amount)
    {
        return Coins >= amount;
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (Coins < amount) return false;

        Coins -= amount;
        OnScoreChanged?.Invoke();
        return true;
    }

    private void UpdateMaxScoreIfNeeded()
    {
        if (Score > MaxScore)
        {
            MaxScore = Score;
            PlayerPrefs.SetInt(PREF_MAX_SCORE, MaxScore);
            PlayerPrefs.Save();
        }
    }
}