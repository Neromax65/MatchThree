using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Класс, отвечающий за начальные параметры и состояние игры
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Реализация синглтона
    /// </summary>
    public static GameManager Instance;
    
    /// <summary>
    /// Текущее состояние игры
    /// </summary>
    public static GameStatus GameStatus = GameStatus.Initializing;
    
    /// <summary>
    /// Инверсия гравитации
    /// </summary>
    public static bool GravityInverted = false;
    
    /// <summary>
    /// Ссылка на префаб элемента
    /// </summary>
    public Element elementPrefab;

    /// <summary>
    /// Размер одной клетки. На таком расстоянии друг от друга будут находиться элементы.
    /// </summary>
    public static Vector2 CellSize;
    
    /// <summary>
    /// Смещение центра размещения элементов в зависимости от разрешения
    /// </summary>
    public static Vector2 Offset;

    /// <summary>
    /// Минимальное количество подряд идущих элементов, необходимых для засчитывания совпадения.
    /// </summary>
    [FormerlySerializedAs("MinElementsToMatch")] public int minElementsToMatch = 3;
    
    /// <summary>
    /// Минимальное количество совпадений для создания элемента, меняющего гравитацию.
    /// </summary>
    [FormerlySerializedAs("MatchesToSpawnInverseElement")] public int matchesToSpawnInverseElement = 4;
    
    /// <summary>
    /// Количество колонок в сетке (X)
    /// </summary>
    public int columns;
    
    /// <summary>
    /// Количество рядов в сетке (Y)
    /// </summary>
    public int rows;
    
    /// <summary>
    /// Ссылка на экран окончания игры
    /// </summary>
    [SerializeField] private GameObject endMenu;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AdjustToCurrentResolution();
    }

    private void AdjustToCurrentResolution()
    {
        float cellSizeMultiplier = FindObjectOfType<Canvas>().scaleFactor;
        CellSize = elementPrefab.GetComponent<RectTransform>().sizeDelta * cellSizeMultiplier;
        Offset = new Vector2(Screen.width/2 - columns/2 * CellSize.x, Screen.height/2 - rows/2 * CellSize.y);
    }

    /// <summary>
    /// Показ экрана окончания игры
    /// </summary>
    public void EndGame()
    {
        endMenu.SetActive(true);
    }

    /// <summary>
    /// Перезапуск текущей сцены
    /// </summary>
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Выход из приложения
    /// </summary>
    public void Exit()
    {
        Application.Quit(0);
    }
}
