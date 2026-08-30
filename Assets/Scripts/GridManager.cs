using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween kütüphanesi aktif

public enum GameState { Spawning, WaitingForInput, Processing }

[System.Serializable]
public class SlotData { public Transform slotTransform; public bool isOccupied; public bool isBlocked = false; public GameObject parkedCar; }

[System.Serializable]
public class Lane { public string laneName; public Transform exitPoint; public SlotData[] slots; }

public class GridManager : MonoBehaviour
{
    public static GridManager Instance; 
    
    [Header("--- EFEKTLER VE UI ---")]
    public GameObject floatingTextPrefab;

    [Header("--- TÜM BÖLÜMLER (VERİTABANI) ---")]
    public LevelData[] allLevels; 
    
    [HideInInspector]
    public LevelData currentLevel; 

    [Header("--- DURUM MAKİNESİ (STATE MACHINE) ---")]
    public GameState currentState = GameState.Spawning;

    [Header("--- ARAÇ AYARLARI ---")]
    public GameObject[] carPrefabs; 

    [Header("--- ŞERİTLER VE OTOPARK ---")]
    public Lane[] lanes; 

    [Header("--- AKIŞ AYARLARI ---")]
    public float spawnInterval = 0.2f; 
    public float driveDuration = 0.4f;     
    public float carYOffset = 0.0f; 

    void Awake()
    {
        Instance = this;
        int levelIndex = MenuManager.selectedLevelNumber - 1;

        if (allLevels != null && allLevels.Length > 0 && levelIndex >= 0 && levelIndex < allLevels.Length)
        {
            currentLevel = allLevels[levelIndex];
            Debug.Log("AWAKE KONTROLÜ: Yüklenen Bölüm -> " + currentLevel.name);
        }
        else if (allLevels != null && allLevels.Length > 0)
        {
            Debug.LogWarning("AWAKE KONTROLÜ: Bölüm bulunamadı! Varsayılan olarak 1. bölüm açılıyor.");
            currentLevel = allLevels[0];
        }
    }

    [ContextMenu("Matrisi Otomatik Doldur (Dinamik!)")]
    public void AutoFillMatrix()
    {
        int dynamicLaneCount = 0;
        while (GameObject.Find("Exit_" + dynamicLaneCount) != null)
        {
            dynamicLaneCount++;
        }

        if (dynamicLaneCount == 0) return;

        lanes = new Lane[dynamicLaneCount];

        for (int col = 0; col < dynamicLaneCount; col++)
        {
            lanes[col] = new Lane();
            lanes[col].laneName = "Serit_" + col;
            
            GameObject exitObj = GameObject.Find("Exit_" + col);
            lanes[col].exitPoint = exitObj.transform;

            int dynamicRowCount = 0;
            while (GameObject.Find("Slot_" + dynamicRowCount + "_" + col) != null)
            {
                dynamicRowCount++;
            }

            lanes[col].slots = new SlotData[dynamicRowCount];

            for (int i = 0; i < dynamicRowCount; i++)
            {
                int rowNum = (dynamicRowCount - 1) - i; 
                string targetName = "Slot_" + rowNum + "_" + col;
                GameObject slotObj = GameObject.Find(targetName);
                
                lanes[col].slots[i] = new SlotData();
                lanes[col].slots[i].slotTransform = slotObj.transform;
            }
        }
    }

    void Start()
    {
        currentState = GameState.Spawning;
        ApplyLevelData(); 
        StartCoroutine(TrafficRoutine());
    }

    void ApplyLevelData()
    {
        if (currentLevel == null) return;
        
        if (currentLevel.closedLanes != null)
        {
            foreach (int closedLaneIndex in currentLevel.closedLanes)
            {
                if (closedLaneIndex >= 0 && closedLaneIndex < lanes.Length)
                    lanes[closedLaneIndex].exitPoint = null; 
            }
        }
        
        if (currentLevel.blockedSlots != null)
        {
            foreach (BlockedSlot blocked in currentLevel.blockedSlots)
            {
                if (blocked.laneIndex < lanes.Length)
                {
                    int maxRowIndex = lanes[blocked.laneIndex].slots.Length - 1;
                    int realIndex = maxRowIndex - blocked.slotIndex; 
                    if (realIndex >= 0 && realIndex <= maxRowIndex)
                        lanes[blocked.laneIndex].slots[realIndex].isBlocked = true;
                }
            }
        }
    }

    IEnumerator TrafficRoutine()
    {
        if (lanes.Length == 0) yield break;
        int maxSlots = 0;
        foreach (var lane in lanes)
        {
            if (lane.slots.Length > maxSlots) maxSlots = lane.slots.Length;
        }

        for (int slotIndex = 0; slotIndex < maxSlots; slotIndex++)
        {
            for (int laneIndex = 0; laneIndex < lanes.Length; laneIndex++)
            {
                if (slotIndex >= lanes[laneIndex].slots.Length) continue;
                Transform currentExit = lanes[laneIndex].exitPoint;
                SlotData targetSlotData = lanes[laneIndex].slots[slotIndex];

                if (currentExit == null || targetSlotData.isBlocked || targetSlotData.slotTransform == null) continue;
                if (carPrefabs == null || carPrefabs.Length == 0) continue;

                int randomCarIndex = Random.Range(0, carPrefabs.Length);
                GameObject selectedPrefab = carPrefabs[randomCarIndex];
                GameObject spawnedCar = Instantiate(selectedPrefab, currentExit.position, currentExit.rotation);
                spawnedCar.name = "Araba_Serit_" + laneIndex + "_Slot_" + slotIndex;

                targetSlotData.isOccupied = true;
                targetSlotData.parkedCar = spawnedCar;

                CarPiece carPiece = spawnedCar.GetComponent<CarPiece>();
                if (carPiece != null)
                {
                    carPiece.xIndex = laneIndex;
                    carPiece.yIndex = slotIndex;
                }
                StartCoroutine(DriveToSlot(spawnedCar, targetSlotData.slotTransform));
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        yield return new WaitForSeconds(1f); 
        StartCoroutine(CheckBoardMatchesRoutine());
    }

    IEnumerator DriveToSlot(GameObject car, Transform target)
    {
        Vector3 finalPos = target.position + new Vector3(0, carYOffset, 0);
        
        car.transform.DOMove(finalPos, driveDuration).SetEase(Ease.OutSine);
        
        yield return new WaitForSeconds(driveDuration);
    }

    public void SwapCarsInMemory(CarPiece car1, CarPiece car2)
    {
        int x1 = car1.xIndex; int y1 = car1.yIndex;
        int x2 = car2.xIndex; int y2 = car2.yIndex;

        lanes[x1].slots[y1].parkedCar = car2.gameObject;
        lanes[x2].slots[y2].parkedCar = car1.gameObject;

        car1.xIndex = x2; car1.yIndex = y2;
        car2.xIndex = x1; car2.yIndex = y1;
        
        StartCoroutine(CheckBoardMatchesRoutine());
    }

    IEnumerator CheckBoardMatchesRoutine()
    {
        yield return null;
        HashSet<CarPiece> carsToDestroy = new HashSet<CarPiece>();

        foreach (Lane lane in lanes)
        {
            foreach (SlotData slot in lane.slots)
            {
                if (slot.parkedCar != null && !slot.isBlocked)
                {
                    CarPiece piece = slot.parkedCar.GetComponent<CarPiece>();
                    if (piece != null && !carsToDestroy.Contains(piece))
                    {
                        List<CarPiece> cluster = GetCluster(piece);
                        if (cluster.Count >= 5) 
                        {
                            foreach (CarPiece c in cluster) carsToDestroy.Add(c);
                        }
                    }
                }
            }
        }

        if (carsToDestroy.Count > 0)
        {
            // İŞTE SİHİRLİ EKRAN SARSINTISI KODU BURADA!
            Camera.main.transform.DOShakePosition(0.10f, 0.05f);

            int popScore = carsToDestroy.Count * 10;
            if (GameManager.Instance != null) GameManager.Instance.AddScore(popScore);
                
            // --- YAZIYI FIRLATTIĞIMIZ YENİ SİHİRLİ KISIM ---
            // Patlayan arabalardan ilkini seçip yazıyı onun tepesinde çıkartıyoruz
            CarPiece firstCar = null;
            foreach (CarPiece c in carsToDestroy) { firstCar = c; break; }

            if (floatingTextPrefab != null && firstCar != null)
            {
                // Yazıyı arabanın biraz yukarısında doğur
                Vector3 spawnPos = firstCar.transform.position + new Vector3(0, 1.5f, 0);
                
                // Quaternion.Euler(50, 0, 0) -> Yazıyı kameraya tam dik baksın diye 50 derece eğik doğuruyoruz!
                GameObject textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.Euler(50, 0, 0));
                
                FloatingText floatScript = textObj.GetComponent<FloatingText>();
                if (floatScript != null)
                {
                    floatScript.Setup("+" + popScore, Color.yellow); 
                }
            }
            // ----------------------------------------------------
                
            foreach (CarPiece carToPop in carsToDestroy)
            {
                if (GameManager.Instance != null) GameManager.Instance.CollectCar(carToPop.myCarType);
                
                lanes[carToPop.xIndex].slots[carToPop.yIndex].isOccupied = false;
                lanes[carToPop.xIndex].slots[carToPop.yIndex].parkedCar = null;
                
                // YENİ VE EFSANE KISIM: Zart diye silinme bitti.
                // Araba 0.2 saniyede içine çökerek küçülüyor, işlem tamamlanınca (OnComplete) Destroy ediliyor!
                carToPop.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => 
                {
                    Destroy(carToPop.gameObject);
                });
            }
            
            // Küçülme animasyonunun süresi kadar (0.2s) bekleyip yukarıdan yeni arabaları öyle düşürüyoruz.
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(ApplyGravityRoutine());
        }
        else
        {
            currentState = GameState.WaitingForInput;
        }
    }

    IEnumerator ApplyGravityRoutine()
    {
        bool gravityApplied = false;
        for (int x = 0; x < lanes.Length; x++)
        {
            for (int y = 0; y < lanes[x].slots.Length; y++)
            {
                if (!lanes[x].slots[y].isOccupied && !lanes[x].slots[y].isBlocked)
                {
                    for (int k = y + 1; k < lanes[x].slots.Length; k++)
                    {
                        if (lanes[x].slots[k].isOccupied && !lanes[x].slots[k].isBlocked)
                        {
                            GameObject fallingCar = lanes[x].slots[k].parkedCar;
                            CarPiece piece = fallingCar.GetComponent<CarPiece>();

                            lanes[x].slots[y].isOccupied = true;
                            lanes[x].slots[y].parkedCar = fallingCar;
                            lanes[x].slots[k].isOccupied = false;
                            lanes[x].slots[k].parkedCar = null;
                            piece.yIndex = y;

                            StartCoroutine(DriveToSlot(fallingCar, lanes[x].slots[y].slotTransform));
                            gravityApplied = true;
                            break; 
                        }
                    }
                }
            }
        }
        if (gravityApplied) yield return new WaitForSeconds(driveDuration);
        StartCoroutine(RefillBoardRoutine());
    }

    IEnumerator RefillBoardRoutine()
    {
        for (int x = 0; x < lanes.Length; x++)
        {
            for (int y = 0; y < lanes[x].slots.Length; y++)
            {
                if (!lanes[x].slots[y].isOccupied && !lanes[x].slots[y].isBlocked && lanes[x].exitPoint != null)
                {
                    Transform exitPoint = lanes[x].exitPoint;
                    GameObject newCar = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], exitPoint.position, exitPoint.rotation);
                    newCar.name = "Araba_Serit_" + x + "_Slot_" + y;

                    CarPiece piece = newCar.GetComponent<CarPiece>();
                    piece.xIndex = x; piece.yIndex = y;

                    lanes[x].slots[y].isOccupied = true;
                    lanes[x].slots[y].parkedCar = newCar;
                    StartCoroutine(DriveToSlot(newCar, lanes[x].slots[y].slotTransform));
                }
            }
        }
        yield return new WaitForSeconds(driveDuration);
        StartCoroutine(CheckBoardMatchesRoutine());
    }

    List<CarPiece> GetCluster(CarPiece startPiece)
    {
        List<CarPiece> cluster = new List<CarPiece>();
        Queue<CarPiece> queue = new Queue<CarPiece>();
        HashSet<CarPiece> visited = new HashSet<CarPiece>();

        queue.Enqueue(startPiece);
        visited.Add(startPiece);
        CarType targetType = startPiece.myCarType;

        while (queue.Count > 0)
        {
            CarPiece current = queue.Dequeue();
            cluster.Add(current);
            int x = current.xIndex; int y = current.yIndex;

            if (x + 1 < lanes.Length && y < lanes[x + 1].slots.Length) CheckNeighbor(x + 1, y, targetType, visited, queue);
            if (x - 1 >= 0 && y < lanes[x - 1].slots.Length) CheckNeighbor(x - 1, y, targetType, visited, queue);
            if (y + 1 < lanes[x].slots.Length) CheckNeighbor(x, y + 1, targetType, visited, queue);
            if (y - 1 >= 0) CheckNeighbor(x, y - 1, targetType, visited, queue);
        }
        return cluster;
    }

    void CheckNeighbor(int x, int y, CarType targetType, HashSet<CarPiece> visited, Queue<CarPiece> queue)
    {
        if (lanes[x].slots[y].isBlocked) return;
        GameObject neighborObj = lanes[x].slots[y].parkedCar;
        if (neighborObj != null)
        {
            CarPiece neighbor = neighborObj.GetComponent<CarPiece>();
            if (neighbor != null && !visited.Contains(neighbor) && neighbor.myCarType == targetType)
            {
                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
    }
}