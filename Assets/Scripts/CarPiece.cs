using UnityEngine;

// Arabaların türlerini/renklerini tanımladığımız liste
public enum CarType
{
    Taxi,       // Sarı
    Police,     // Siyah/Mavi
    SedanSports,// Turuncu
    F1,         // Yeni eklendi
    Truck       // Yeni eklendi
}

public class CarPiece : MonoBehaviour
{
    [Header("KİMLİK KARTI")]
    public CarType myCarType;

    [Header("HARİTA KOORDİNATLARI (OTOMATİK)")]
    // Arabanın matristeki yerini tutan hafıza (BUNLARIN SİLİNMEMESİ GEREKİYOR!)
    public int xIndex; 
    public int yIndex; 
}