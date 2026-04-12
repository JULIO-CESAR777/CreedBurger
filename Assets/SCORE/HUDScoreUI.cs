using TMPro;
using UnityEngine;

public class HUDScoreUI : MonoBehaviour
{
    
    ScoreSystem scoreSystem;
    
    public TMP_Text coinsText;
    public TMP_Text scoreText;
    public TMP_Text maxScoreText;

    [SerializeField] private GameObject noMoneyPanel;
    [SerializeField] private GameObject SubstractMoneyPanel;
    public float noMoneyCd;
    private float counter;
    private float counterSubstract;
    GameManager gm;
    private int _price;
    
    void Start()
    {
        scoreSystem = ScoreSystem.Instance;
        if (ScoreSystem.Instance == null) return;
        ScoreSystem.Instance.OnScoreChanged += Refresh;
        Refresh();
        gm = GameManager.GetInstance();
        counter = 0;
        noMoneyPanel.SetActive(false);
        SubstractMoneyPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (scoreSystem != null)
            scoreSystem.OnScoreChanged -= Refresh;
    }

    void Refresh()
    {
        coinsText.text = $"Coins: {scoreSystem.Coins}";
        scoreText.text = $"Score: {scoreSystem.Score}";
        maxScoreText.text = $"Max: {scoreSystem.MaxScore}";
    }

    private void Update()
    {
        if (gm.gameState == GameState.Pause) return;
        if (noMoneyPanel.activeInHierarchy)
        {
            counter += Time.deltaTime;
            if (counter >= noMoneyCd)
            {
                noMoneyPanel.SetActive(false);
                counter = 0;
            }
        }
        if (SubstractMoneyPanel.activeInHierarchy)
        {
            counterSubstract += Time.deltaTime;
            if (counterSubstract >= noMoneyCd)
            {
                scoreSystem.TrySpendCoins(_price);
                SubstractMoneyPanel.SetActive(false);
                counterSubstract = 0;
            }
        }
        
    }

    public void ShowNoMoneyPanel()
    {
        noMoneyPanel.SetActive(true);
    }

    public void ShowSubstractMoneyPanel(int price)
    {
        if (price <= 0) return;
        _price = price;
        SubstractMoneyPanel.GetComponent<TextMeshProUGUI>().text = "-"+ price.ToString();
        SubstractMoneyPanel.SetActive(true);
    }
}