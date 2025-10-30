using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public int totalPieceCount;
    public bool isGameCompleted = false;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad((this.gameObject));
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public bool CheckGameCompleted(int count)
    {
        isGameCompleted = count == totalPieceCount;
        Debug.Log(count);
        if (isGameCompleted)
        {
            AudioManager.instance.PlaySound("Completed");
        }
        return isGameCompleted;
    }
}
