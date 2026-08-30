using System.Collections;
using UnityEngine;
using DG.Tweening; // YENİ: DOTween'i buraya da aldık!

public class InputManager : MonoBehaviour
{
    private CarPiece selectedCar;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    
    [Header("--- AYARLAR ---")]
    public float swipeThreshold = 50f; 
    
    // swapSpeed yerine saniye cinsinden süre kullanacağız (Örn: 0.3 saniye)
    public float swapDuration = 0.3f; 

    void Update()
    {
        if (GridManager.Instance.currentState != GameState.WaitingForInput) return;

        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedCar = hit.collider.GetComponent<CarPiece>();
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedCar != null)
        {
            endTouchPosition = Input.mousePosition;
            CalculateSwipeDirectionAndSwap();
            selectedCar = null; 
        }
    }

    void CalculateSwipeDirectionAndSwap()
    {
        Vector2 swipeVector = endTouchPosition - startTouchPosition;
        if (swipeVector.magnitude < swipeThreshold) return;

        swipeVector.Normalize();

        int targetX = selectedCar.xIndex;
        int targetY = selectedCar.yIndex;

        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            if (swipeVector.x > 0) targetX += 1; 
            else targetX -= 1; 
        }
        else
        {
            if (swipeVector.y > 0) targetY += 1; 
            else targetY -= 1; 
        }

        if (targetX < 0 || targetX >= GridManager.Instance.lanes.Length) return;
        if (targetY < 0 || targetY >= GridManager.Instance.lanes[targetX].slots.Length) return;
        if (GridManager.Instance.lanes[targetX].slots[targetY].isBlocked) return;

        GameObject targetCarObj = GridManager.Instance.lanes[targetX].slots[targetY].parkedCar;
        if (targetCarObj == null) return;

        CarPiece targetCar = targetCarObj.GetComponent<CarPiece>();
        GameManager.Instance.UseMove();
        
        StartCoroutine(SwapCarsRoutine(selectedCar, targetCar));
    }

    IEnumerator SwapCarsRoutine(CarPiece car1, CarPiece car2)
    {
        GridManager.Instance.currentState = GameState.Processing;

        Vector3 pos1 = car1.transform.position;
        Vector3 pos2 = car2.transform.position;

        // YENİ SİHİR: Eski Lerp döngüsü gitti! Ease.OutSine ile tüy gibi hafif kayacaklar.
        car1.transform.DOMove(pos2, swapDuration).SetEase(Ease.OutSine);
        car2.transform.DOMove(pos1, swapDuration).SetEase(Ease.OutSine);

        // Animasyon bitene kadar bekle
        yield return new WaitForSeconds(swapDuration);

        // Görsel iş bitince arkadaki beyne (GridManager) veriyi güncelle diyoruz
        GridManager.Instance.SwapCarsInMemory(car1, car2);
    }
}