using UnityEngine;
using TMPro; 
using DG.Tweening; 

public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(string textToDisplay, Color textColor)
    {
        if (textMesh == null) return;

        // 1. SİHİR: Yazı her zaman kameraya %100 düz baksın! İncecik kağıt gibi kalmaz.
        transform.rotation = Camera.main.transform.rotation;

        textMesh.text = textToDisplay;
        textMesh.color = textColor;
        textMesh.alpha = 1f; // Başlangıçta tam görünür olsun

        // 2. SİHİR: Yazı sıfırdan "POP" diye büyüyerek ekrana gelsin (Çok daha tatmin edici!)
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // 3. Yukarı doğru süzülme (1 saniye boyunca)
        transform.DOMoveY(transform.position.y + 1.5f, 1f).SetEase(Ease.OutCirc);

        // 4. Şeffaflaşma (Sadece son 0.5 saniyede yavaşça solsun ve yok olsun)
        textMesh.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}