using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    [SerializeField] private Text gameStatusText;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameStatusText.text = GameManager.GameStatus.ToString();
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Force match");
            New.Grid.Instance.CheckMatchesAll();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Force fall");
            New.Grid.Instance.DropElementsAll();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log($"Check for possible moves: {New.Grid.Instance.CheckForPossibleMoves()}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Force spawn new tiles");
            New.Grid.Instance.SpawnNewElements();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log($"Grid is full: {New.Grid.Instance.IsGridFull()}");
        }
    }
}
