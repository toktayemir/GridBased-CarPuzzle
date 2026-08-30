using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static int selectedLevelNumber = 1; 

    public void PlayLevel(int levelId)
    {
        // 1. GÜVENLİK KAMERASI: Buton gerçekten doğru sayıyı gönderiyor mu?
        Debug.Log("KAMERA 1 - BUTONA BASILDI! Gelen Numara: " + levelId);
        
        selectedLevelNumber = levelId; 
        SceneManager.LoadScene("SampleScene"); 
    }
}