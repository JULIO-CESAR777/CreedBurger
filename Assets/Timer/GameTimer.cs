using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Duración de la partida (segundos)")]
    public float duration = 120f; // 2 minutos por defecto

    public float Remaining { get; private set; }
    public bool IsOver { get; private set; }

    public event Action<float> OnTick;     // notifica tiempo restante cada frame
    public event Action OnTimesUp;         // notifica cuando llega a 0

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        ResetTimer();
    }

    public void ResetTimer()
    {
        Remaining = Mathf.Max(0f, duration);
        IsOver = false;
        OnTick?.Invoke(Remaining);
       
    }

    void Update()
    {
        if (IsOver) return;

        Remaining -= Time.deltaTime;
        if (Remaining <= 0f)
        {
            Remaining = 0f;
            IsOver = true;
            OnTick?.Invoke(Remaining);
            OnTimesUp?.Invoke();
            return;
        }
        OnTick?.Invoke(Remaining);
    }

    public void AddTime(float extraSeconds)
    {
        if (IsOver) return;
        Remaining = Mathf.Max(0f, Remaining + extraSeconds);
        OnTick?.Invoke(Remaining);
    }
}
