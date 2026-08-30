using UnityEngine;
using System.Collections.Generic;

// ARTIK BUNA "LEVEL TYPE" DEĞİL "KAZANMA ŞARTI" DİYORUZ (TimeAttack silindi)
public enum WinCondition
{
    ScoreTarget,
    CollectCars,
    DropSpecialItem
}

[System.Serializable]
public class CarTarget
{
    public CarType carType; // Senin tanımladığın araç tipi
    public int targetCount;
}

[System.Serializable]
public class BlockedSlot
{
    public int laneIndex; // Şerit (X)
    public int slotIndex; // Satır (Y)
}

[CreateAssetMenu(fileName = "Level_X", menuName = "Otopark Oyunu/Yeni Bolum Olustur")]
public class LevelData : ScriptableObject
{
    [Header("--- GENEL AYARLAR ---")]
    public int levelNumber;
    
    [Header("--- HEDEF (KAZANMA ŞARTI) ---")]
    [Tooltip("Bölümü geçmek için ne yapmak gerekiyor?")]
    public WinCondition winCondition;
    public int targetScore = 0; 
    public List<CarTarget> carsToCollect;

    [Header("--- SINIRLAR (KAYBETME ŞARTLARI) ---")]
    [Tooltip("Bölümde hamle sınırı olsun mu?")]
    public bool useMoveLimit;
    public int moveLimit = 20; 
    
    [Tooltip("Bölümde süre sınırı olsun mu? İkisi aynı anda da seçilebilir!")]
    public bool useTimeLimit;
    public float timeLimit = 60f; 

    [Header("--- HARİTA DİZAYNI (YOLLAR VE ENGELLER) ---")]
    public List<int> closedLanes; 
    public List<BlockedSlot> blockedSlots; 
}