using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("--- UI (ARAYÜZ) REFERANSLARI ---")]
    public TextMeshProUGUI leftText;   // SOL PANEL (Sadece Hamle)
    public TextMeshProUGUI centerText; // ORTA PANEL (Ana Görev / Hedef Skor)
    public TextMeshProUGUI rightText;  // SAĞ PANEL (Sadece Süre)

    [Header("--- ARKA PLAN VERİLERİ ---")]
    private LevelData currentLevel;
    private int currentMoves;
    private float currentTime;
    private int currentScore;
    
    private Dictionary<CarType, int> runtimeTargets = new Dictionary<CarType, int>();
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentLevel = GridManager.Instance.currentLevel;
        if (currentLevel == null) return;

        currentMoves = currentLevel.moveLimit;
        currentTime = currentLevel.timeLimit;
        currentScore = 0;

        // Görev araba toplamaksa listeye al
        if (currentLevel.winCondition == WinCondition.CollectCars && currentLevel.carsToCollect != null)
        {
            foreach (var target in currentLevel.carsToCollect)
            {
                if (!runtimeTargets.ContainsKey(target.carType))
                    runtimeTargets.Add(target.carType, target.targetCount);
            }
        }

        UpdateUI();
    }

    void Update()
    {
        if (isGameOver || currentLevel == null) return;

        if (currentLevel.useTimeLimit)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateUI(); 
            }
            else
            {
                GameOver("SÜRE\nBİTTİ!");
            }
        }
    }

    public void UseMove()
    {
        if (isGameOver || currentLevel == null) return;

        if (currentLevel.useMoveLimit && currentMoves > 0)
        {
            currentMoves--;
            UpdateUI();

            if (currentMoves <= 0)
            {
                GameOver("HAMLE\nBİTTİ!");
            }
        }
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;
        
        currentScore += points;
        UpdateUI();
        
        if (currentLevel.winCondition == WinCondition.ScoreTarget && currentScore >= currentLevel.targetScore)
        {
            WinGame();
        }
    }

    public void CollectCar(CarType poppedCarType)
    {
        if (isGameOver || currentLevel.winCondition != WinCondition.CollectCars) return;

        if (runtimeTargets.ContainsKey(poppedCarType) && runtimeTargets[poppedCarType] > 0)
        {
            runtimeTargets[poppedCarType]--;
            UpdateUI();
            CheckWinCondition();
        }
    }

    void CheckWinCondition()
    {
        bool isWon = true;
        foreach (var count in runtimeTargets.Values)
        {
            if (count > 0) isWon = false; 
        }

        if (isWon) WinGame();
    }
    
    void WinGame()
    {
        isGameOver = true;
        Debug.Log("TEBRİKLER! BÖLÜM GEÇİLDİ!");
        centerText.text = "BÖLÜM\nGEÇİLDİ!";
    }

    void GameOver(string reason)
    {
        isGameOver = true;
        Debug.Log("GAME OVER: " + reason);
        centerText.text = reason; // Kaybedince sebebi ortada yazsın!
    }

    // İŞTE SENİN İSTEDİĞİN O YENİ EFSANE ARAYÜZ MANTIĞI
    void UpdateUI()
    {
        if (currentLevel == null || isGameOver) return;

        // 1. SOL TABELA (HAMLE)
        if (currentLevel.useMoveLimit) 
            leftText.text = "HAMLE\n" + currentMoves;
        else 
            leftText.text = "HAMLE\n\u221E"; // Sınır yoksa "Sonsuz" işareti koyar (İstersen "SERBEST" de yazdırabilirsin)

        // 2. SAĞ TABELA (SÜRE)
        if (currentLevel.useTimeLimit) 
            rightText.text = "SÜRE\n" + Mathf.CeilToInt(currentTime).ToString() + "s";
        else 
            rightText.text = "SÜRE\n\u221E";

        // 3. ORTA TABELA (ANA GÖREV)
        if (currentLevel.winCondition == WinCondition.CollectCars && runtimeTargets.Count > 0)
        {
            string targetString = "GÖREV\n";
            foreach (var kvp in runtimeTargets)
            {
                targetString += kvp.Key.ToString() + ": " + kvp.Value + "\n";
            }
            centerText.text = targetString.TrimEnd();
        }
        else if (currentLevel.winCondition == WinCondition.ScoreTarget)
        {
            centerText.text = "HEDEF\n" + currentScore + " / " + currentLevel.targetScore;
        }
        else
        {
            centerText.text = "GÖREV\n-";
        }
    }
}