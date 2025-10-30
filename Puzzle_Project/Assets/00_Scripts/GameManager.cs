using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public int totalPieceCount;
    public bool isGameCompleted = false;

    [SerializeField] private GameObject GameOne, GameTwo;
    
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

    public void SetGameStart()
    {
        if (totalPieceCount == 35) GameOne.SetActive(true);
        else GameTwo.SetActive(true);
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
