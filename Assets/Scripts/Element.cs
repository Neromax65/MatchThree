using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Enums;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
///     Цветной элемент, располагаемый на игровом поле
/// </summary>
public class Element : MonoBehaviour
{
    /// <summary>
    ///     Перечисление возможных цветов/типов элементов
    /// </summary>
    public enum ElementType
    {
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Magenta
    }

    /// <summary>
    ///     Кеширование индекса анимации
    /// </summary>
    private static readonly int MatchIndex = Animator.StringToHash("Match");

    /// <summary>
    ///     Индекс колонки элемента, его координата X в рамках игрового поля
    /// </summary>
    [HideInInspector] public int column;

    /// <summary>
    ///     Индекс ряда элемента, его координата Y в рамках игрового поля
    /// </summary>
    [HideInInspector] public int row;

    /// <summary>
    ///     Ссылка на основной спрайт элемента
    /// </summary>
    [SerializeField] private Image mainImage;

    /// <summary>
    ///     Ссылка на спрайт иконки смены гравитации
    /// </summary>
    [SerializeField] private Image invertGravityImage;

    /// <summary>
    ///     Ссылка на спрайт иконки смены гравитации
    /// </summary>
    [SerializeField] private Image selectionBorderImage;

    /// <summary>
    ///     Ссылка на компонент аниматор
    /// </summary>
    [SerializeField] private Animator animator;

    /// <summary>
    ///     Тип/цвет элемента
    /// </summary>
    public ElementType Type { get; private set; }

    /// <summary>
    ///     Установить конкретный тип элемента
    /// </summary>
    /// <param name="type">Тип элемента</param>
    public void SetType(ElementType type)
    {
        Type = type;
        UpdateColor();
    }

    /// <summary>
    ///     Получить случайный тип элемента из всех возможных
    /// </summary>
    /// <returns>Случайный тип элемента</returns>
    public static ElementType GetRandomType()
    {
        return (ElementType) Random.Range(0, Enum.GetNames(typeof(ElementType)).Length);
    }

    /// <summary>
    ///     Создание цветного элемента
    /// </summary>
    /// <param name="column">Колонка</param>
    /// <param name="row">Ряд</param>
    /// <param name="type">Тип элемента</param>
    /// <param name="inverseGravity">Меняет ли гравитацию</param>
    /// <returns>Созданный элемент</returns>
    public static Element Create(int column, int row, ElementType type, bool inverseGravity)
    {
        var spawnPosition = GetWorldCoords(column, row);
        var element = Instantiate(GameManager.Instance.elementPrefab, spawnPosition, Quaternion.identity,
            Grid.Instance.transform);
        element.column = column;
        element.row = row;
        element.gameObject.name = element.ToString();
        element.SetType(type);
        element.invertGravityImage.enabled = inverseGravity;

        return element;
    }

    /// <summary>
    ///     Засчитывание элемента как совпавшего и удаление его из игры
    /// </summary>
    public void Match()
    {
        mainImage.raycastTarget = false;
        invertGravityImage.raycastTarget = false;
        column = -1;
        row = -1;

        if (invertGravityImage.enabled)
            GameManager.GravityInverted = !GameManager.GravityInverted;

        animator.SetTrigger(MatchIndex);

        Destroy(gameObject, 0.11f);
    }

    /// <summary>
    ///     Поменяться местами с другим элементом. Вызывается самим игроком. Если после смены мест совпадений не нашлось,
    ///     то элементы перемещаются обратно.
    /// </summary>
    /// <param name="otherElement"></param>
    /// <returns></returns>
    public IEnumerator Swap(Element otherElement)
    {
        GameManager.GameStatus = GameStatus.PlayingAnimation;
        using (var transaction = new SwapTransaction(this, otherElement))
        {
            var swap1 = StartCoroutine(UpdateWorldPosition());
            var swap2 = StartCoroutine(otherElement.UpdateWorldPosition());

            yield return swap1;
            yield return swap2;

            // int matches = Grid.Instance.CalculateMatches(false);
            var matches = Grid.Instance.CalculateMatches(false);
            if (matches > 0)
            {
                transaction.Commit();
                Grid.Instance.CalculateMatches(true);
                yield break;
            }
        }

        var returnSwap1 = StartCoroutine(UpdateWorldPosition());
        var returnSwap2 = StartCoroutine(otherElement.UpdateWorldPosition());

        yield return returnSwap1;
        yield return returnSwap2;

        GameManager.GameStatus = GameStatus.WaitingForInput;
    }

    /// <summary>
    ///     Обновление положения элемента в мировых координатах в соответствии с координатами на сетке
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateWorldPosition(float time = 0.35f)
    {
        Vector2 startPosition = transform.position;
        var targetPosition = GetWorldCoords(column, row);

        if (time > 0)
            for (float t = 0; t < 1; t += Time.deltaTime * 1 / time)
            {
                transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

        gameObject.name = $"Element [{column},{row}][{Type}]";
        transform.position = targetPosition;
    }

    /// <summary>
    ///     Является ли элемент смежным (не считая диагонали) по отношению к другому элементу
    /// </summary>
    /// <param name="otherElement">Другой элемент</param>
    /// <returns>True - элемент смежный, False - элемент не смежный</returns>
    public bool IsAdjacentTo(Element otherElement)
    {
        return Mathf.Abs(otherElement.column - column) + Mathf.Abs(otherElement.row - row) == 1;
    }

    /// <summary>
    ///     Переключить отлов лучей
    /// </summary>
    /// <param name="flag">True - вкл, False - выкл</param>
    public void ToggleRaycasts(bool flag)
    {
        mainImage.raycastTarget = flag;
        invertGravityImage.raycastTarget = flag;
    }

    /// <summary>
    ///     Переключить видимость рамки выбора
    /// </summary>
    /// <param name="flag">True - вкл, False - выкл</param>
    public void ToggleSelectionBorder(bool flag)
    {
        selectionBorderImage.enabled = flag;
    }

    /// <summary>
    ///     Обновление цвета элемента в соответствии с типом
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Выкинет при несуществующем типе элемента</exception>
    private void UpdateColor()
    {
        switch (Type)
        {
            case ElementType.Red:
                mainImage.color = Color.red;
                break;
            case ElementType.Green:
                mainImage.color = Color.green;
                break;
            case ElementType.Blue:
                mainImage.color = Color.blue;
                break;
            case ElementType.Yellow:
                mainImage.color = Color.yellow;
                break;
            case ElementType.Cyan:
                mainImage.color = Color.cyan;
                break;
            case ElementType.Magenta:
                mainImage.color = Color.magenta;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Преобразование координат сетки в мировые координаты с учетом смещения
    /// </summary>
    /// <param name="column">Колонка</param>
    /// <param name="row">Ряд</param>
    /// <returns>Вектор в координатах мира</returns>
    private static Vector2 GetWorldCoords(int column, int row)
    {
        return new Vector2(column * GameManager.CellSize.x + GameManager.Offset.x,
            row * GameManager.CellSize.x + GameManager.Offset.y);
    }

    public override string ToString()
    {
        return $"Element [{column},{row}][{Type}]";
    }
}