using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Игровая сетка. Класс отвечает за положение элементов на поле, просчитывание совпадений, проверки на возможные ходы.
/// </summary>
public class Grid : MonoBehaviour
{
    /// <summary>
    /// Максимальное количество итераций для цикла, подбирающего играбельное поле.
    /// </summary>
    private const int OverflowLimit = 4000;

    /// <summary>
    /// Реализация синглтона
    /// </summary>
    public static Grid Instance;

    /// <summary>
    /// Двумерный массив, хранящий в себе все элементы, ключами являются X (Column) и Y (Row) координаты соответственно
    /// </summary>
    private Element[,] _elements;

    /// <summary>
    /// Кеширование колонок
    /// </summary>
    private int _columns;
    
    /// <summary>
    /// Кеширование рядов
    /// </summary>
    private int _rows;

    private void Awake()
    {
        Instance = this;
        _rows = GameManager.Instance.rows;
        _columns = GameManager.Instance.columns;
    }

    private void Start()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Инициализация игровой сетки. Отвечает за генерацию играбельного поля.
    /// </summary>
    /// <exception cref="StackOverflowException">Может выбросить, если было указано слишком большая ширина/высота
    /// сетки при малом количестве возможных элементов </exception>
    private void InitializeGrid()
    {
        _elements = new Element[_columns, _rows];
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                _elements[x, y] = Element.Create(x, y, Element.GetRandomType(), false);
            }
        }

        int counter = 0;
        while (CalculateMatches(false) > 0 || !IsMoveAvailable())
        {
            if (counter >= OverflowLimit)
                throw new StackOverflowException();

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    _elements[x, y].SetType(Element.GetRandomType());
                }
            }

            counter++;
        }

        GameManager.GameStatus = GameStatus.WaitingForInput;
    }


    /// <summary>
    /// Проверка возможности хода
    /// </summary>
    /// <returns>True - есть возможные ходы, False - нет возможных ходов</returns>
    public bool IsMoveAvailable()
    {
        Debug.Log("IsMoveAvailable");
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                var element = _elements[x, y];
                if (element == null) continue;

                var adjacentElements = GetAdjacentElements(element);

                foreach (var adjacentElement in adjacentElements)
                {
                    using (new SwapTransaction(element, adjacentElement))
                    {
                        var matchesCount = GetMatchesForElement(element).Count;
                        if (matchesCount >= GameManager.Instance.minElementsToMatch) return true;
                    }
                }
            }
        }

        return false;
    }


    /// <summary>
    /// Просчитыване совпадений по всем элементам
    /// </summary>
    /// <param name="clearMatched">Нужно ли очищать поле от совпавших элементов</param>
    /// <returns>Количество совпавших элементов</returns>
    public int CalculateMatches(bool clearMatched)
    {
        int totalMatches = 0;
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                if (_elements[x, y] == null) continue;

                List<Element> currentMatchedElements = GetMatchesForElement(_elements[x, y]);
                if (currentMatchedElements.Count >= GameManager.Instance.minElementsToMatch)
                {
                    totalMatches += currentMatchedElements.Count;

                    if (clearMatched)
                    {
                        Element.ElementType matchedType = currentMatchedElements[0].Type;
                        int randomMatchedIndex = Random.Range(0, currentMatchedElements.Count);
                        int randomMatchedCol = currentMatchedElements[randomMatchedIndex].column;
                        int randomMatchedRow = currentMatchedElements[randomMatchedIndex].row;

                        foreach (var element in currentMatchedElements)
                        {
                            _elements[element.column, element.row] = null;
                            element.Match();
                        }

                        if (currentMatchedElements.Count >= GameManager.Instance.matchesToSpawnInverseElement)
                        {
                            _elements[randomMatchedCol, randomMatchedRow] =
                                Element.Create(randomMatchedCol, randomMatchedRow, matchedType, true);
                        }
                    }
                }
            }
        }

        if (clearMatched)
        {
            if (totalMatches > 0)
                DropElementsAll();
            else if (IsMoveAvailable())
                GameManager.GameStatus = GameStatus.WaitingForInput;
            else
                GameManager.Instance.EndGame();
        }

        return totalMatches;
    }

    /// <summary>
    /// Запуск падения всех элементов
    /// </summary>
    public void DropElementsAll()
    {
        bool dropped = false;

        for (int x = 0; x < _columns; x++)
        {
            int? firstEmptyY = null;
            if (!GameManager.GravityInverted)
                for (int y = 0; y < _rows; y++)
                {
                    if (DropElement(x, y, ref firstEmptyY))
                        dropped = true;
                }
            else
                for (int y = _rows - 1; y >= 0; y--)
                {
                    if (DropElement(x, y, ref firstEmptyY))
                        dropped = true;
                }
        }

        if (dropped)
        {
            GameManager.GameStatus = GameStatus.PlayingAnimation;
            StartCoroutine(FallAnimation());
        }
        else
        {
            if (!IsGridFull())
                CreateNewElements();
            else
                CalculateMatches(true);
        }
    }

    /// <summary>
    /// Обработка падения конкретного элемента
    /// </summary>
    /// <param name="x">Колонка элемента</param>
    /// <param name="y">Ряд элемента</param>
    /// <param name="firstEmptyY">Первый пустой ряд</param>
    /// <returns>Был ли сброшен элемент</returns>
    private bool DropElement(int x, int y, ref int? firstEmptyY)
    {
        if (!firstEmptyY.HasValue && _elements[x, y] == null)
        {
            firstEmptyY = y;
        }
        else if (firstEmptyY.HasValue && _elements[x, y] != null)
        {
            _elements[x, firstEmptyY.Value] = _elements[x, y];
            _elements[x, y] = null;
            _elements[x, firstEmptyY.Value].row = firstEmptyY.Value;

            firstEmptyY = GameManager.GravityInverted ? --firstEmptyY : ++firstEmptyY;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Создание новых элементов в верхнем/нижнем ряду в зависимости от гравитации
    /// </summary>
    public void CreateNewElements()
    {
        int y = GameManager.GravityInverted ? 0 : _rows - 1;
        for (int x = 0; x < _columns; x++)
        {
            if (_elements[x, y] == null)
                _elements[x, y] = Element.Create(x, y, Element.GetRandomType(), false);
        }

        DropElementsAll();
    }

    /// <summary>
    /// Получение списка совпадений для конкретного элемента
    /// </summary>
    /// <param name="element">Конкретный элемент</param>
    /// <returns>Список совпавших элементов</returns>
    private List<Element> GetMatchesForElement(Element element)
    {
        List<Element> matchedElements = new List<Element>();
        CheckMatchesRecursive(element, ref matchedElements);
        return matchedElements;
    }

    /// <summary>
    /// Рекурсивная проверка на совпадения
    /// </summary>
    /// <param name="element">Текущий проверяемый элемент</param>
    /// <param name="matchedElements">Список совпавших элементов в данном комплекте</param>
    void CheckMatchesRecursive(Element element, ref List<Element> matchedElements)
    {
        matchedElements.Add(element);

        var adjacentElements = GetAdjacentElements(element);
        foreach (var adjacentElement in adjacentElements)
        {
            if (adjacentElement.Type == element.Type && !matchedElements.Contains(adjacentElement))
                CheckMatchesRecursive(adjacentElement, ref matchedElements);
        }
    }

    /// <summary>
    /// Проверка на заполненность поля
    /// </summary>
    /// <returns>True - поле заполнено, False - есть пустые клетки</returns>
    public bool IsGridFull()
    {
        for (int x = 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                if (_elements[x, y] == null)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Запуск анимации падения
    /// </summary>
    /// <returns></returns>
    private IEnumerator FallAnimation()
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        for (int x = 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                if (_elements[x, y] == null) continue;

                coroutines.Add(_elements[x, y].StartCoroutine(_elements[x, y].UpdateWorldPosition()));
            }
        }

        foreach (var coroutine in coroutines)
            yield return coroutine;

        CreateNewElements();
    }

    /// <summary>
    /// Получение соседних элементов, не учитывая диагонали
    /// </summary>
    /// <param name="middleElement">Центральный элемент</param>
    /// <returns></returns>
    private List<Element> GetAdjacentElements(Element middleElement)
    {
        List<Element> adjacentElements = new List<Element>(4);
        if (middleElement.column < _columns - 1 && _elements[middleElement.column + 1, middleElement.row] != null)
            adjacentElements.Add(_elements[middleElement.column + 1, middleElement.row]);

        if (middleElement.column > 0 && _elements[middleElement.column - 1, middleElement.row] != null)
            adjacentElements.Add(_elements[middleElement.column - 1, middleElement.row]);

        if (middleElement.row < _rows - 1 && _elements[middleElement.column, middleElement.row + 1] != null)
            adjacentElements.Add(_elements[middleElement.column, middleElement.row + 1]);

        if (middleElement.row > 0 && _elements[middleElement.column, middleElement.row - 1] != null)
            adjacentElements.Add(_elements[middleElement.column, middleElement.row - 1]);

        return adjacentElements;
    }

    /// <summary>
    /// Обновление индексов элемента в общем массиве в соответствии с его колонкой и рядом 
    /// </summary>
    /// <param name="element">Элемент</param>
    public void UpdateElementIndices(Element element)
    {
        if (element == null) return;
        _elements[element.column, element.row] = element;
    }
}