using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    [SerializeField] private Text gameStatusText;
    
    void Update()
    {
        gameStatusText.text = GameManager.GameStatus.ToString();
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Force match");
            Grid.Instance.CalculateMatches(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Force fall");
            Grid.Instance.DropElementsAll();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log($"Check for possible moves: {Grid.Instance.IsMoveAvailable()}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Force spawn new tiles");
            Grid.Instance.SpawnNewElements();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log($"Grid is full: {Grid.Instance.IsGridFull()}");
        }
    }
}
