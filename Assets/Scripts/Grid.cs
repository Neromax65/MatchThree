using System;
using System.Collections;
using System.Collections.Generic;
using Helpers;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    private const int CellSize = 80;
    private const int MinElementsToMatch = 3;
    private const int MatchesToSpawnInverseElement = 4;
    private const int OverflowLimit = 4000;
    
    public static Grid Instance;
    public Element elementPrefab;
    public int columns;
    public int rows;
    
    private Element[,] _elements;
    
    private static Vector2 _offset;

    private void Awake()
    {
        Instance = this;
        _offset = Camera.main.WorldToScreenPoint(-new Vector2(columns/2, rows/2));
    }

    private void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        _elements = new Element[columns,rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                _elements[x, y] = Element.SpawnRandom(x, y);
            }
        }

        int counter = 0;
        while (CalculateMatches(false) > 0 || !IsMoveAvailable())
        {
            if (counter >= OverflowLimit)
                throw new StackOverflowException();
            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    _elements[x,y].SetType(Element.GetRandomType());
                }
            }

            counter++;
        }

        GameManager.GameStatus = GameStatus.WaitingForInput;
    }
    
    public bool IsMoveAvailable()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var element = _elements[x, y];
                if (element == null) continue;

                var adjacentElements = GetAdjacentElements(element);

                foreach (var adjacentElement in adjacentElements)
                {
                    using (new SwapTransaction(element, adjacentElement))
                    {
                        var matchesCount = GetMatchesForElement(element).Count;
                        if (matchesCount >= MinElementsToMatch) return true;
                    } 
                }
            }
        }
        return false;
    }


    public int CalculateMatches(bool clearMatched)
    {
        int totalMatches = 0;
        // List<Coroutine> matchRoutines = new List<Coroutine>();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (_elements[x,y] == null) continue;

                List<Element> matches = GetMatchesForElement(_elements[x, y]);
                if (matches.Count >= MinElementsToMatch)
                {
                    totalMatches += matches.Count;
                        
                    if (clearMatched)
                    {
                        Element.ElementType matchedType = matches[0].Type;
                        int randomMatchedIndex = Random.Range(0, matches.Count);
                        int randomMatchedCol = matches[randomMatchedIndex].Column;
                        int randomMatchedRow = matches[randomMatchedIndex].Row;
                            
                        foreach (var element in matches)
                        {
                            _elements[element.Column, element.Row].Match();
                            // matchRoutines.Add(StartCoroutine(_elements[element.Column, element.Row].Match()));
                            _elements[element.Column, element.Row] = null;
                        }
                        if (matches.Count >= MatchesToSpawnInverseElement)
                        {
                            _elements[randomMatchedCol, randomMatchedRow] = Element.SpawnInverseGravity( randomMatchedCol, randomMatchedRow, matchedType);
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

    public void DropElementsAll()
    {
        bool dropped = false;

        for (int x = 0; x < columns; x++)
        {
            int? firstEmptyY = null;
            if (!GameManager.GravityInverted) 
                for (int y = 0; y < rows; y++)
                {
                    if (DropElement(x, y, ref firstEmptyY))
                        dropped = true;
                }
            else
                for (int y = rows - 1; y >= 0; y--)
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
                SpawnNewElements();
            else
                CalculateMatches(true);
        }
    }

    private bool DropElement(int x, int y, ref int? firstEmptyY)
    {
        if (!firstEmptyY.HasValue && _elements[x, y] == null)
        {
            firstEmptyY = y;
        }
        else if (firstEmptyY.HasValue && _elements[x, y] != null)
        {
            _elements[x, firstEmptyY.Value] = _elements[x,y];
            _elements[x, y] = null;
            _elements[x, firstEmptyY.Value].Row = firstEmptyY.Value;

            firstEmptyY = GameManager.GravityInverted ? --firstEmptyY : ++firstEmptyY;
                
            return true;
        }

        return false;
    }

    public void SpawnNewElements()
    {
        int y = GameManager.GravityInverted ? 0 : rows - 1;
        for (int x = 0; x < columns; x++)
        {
            if (_elements[x, y] == null)
                _elements[x, y] = Element.SpawnRandom(x, y);
        }

        DropElementsAll();
    }

    private List<Element> GetMatchesForElement(Element element)
    {
        List<Element> matchedElements = new List<Element>();
        CheckMatchesRecursive(element, ref matchedElements);
        return matchedElements;
    }

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

    public bool IsGridFull()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (_elements[x, y] == null)
                    return false;
            }
        }

        return true;
    }

    private IEnumerator FallAnimation()
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (_elements[x, y] == null) continue;
                
                coroutines.Add(_elements[x,y].StartCoroutine(_elements[x,y].SettleWorldPosition()));
            }
        }

        foreach (var coroutine in coroutines)
            yield return coroutine;
        
        SpawnNewElements();
    }

    private List<Element> GetAdjacentElements(Element middleElement)
    {
        List<Element> adjacentElements = new List<Element>(4);
        if (middleElement.Column < columns - 1 && _elements[middleElement.Column + 1, middleElement.Row] != null)
            adjacentElements.Add(_elements[middleElement.Column + 1, middleElement.Row]);
            
        if (middleElement.Column > 0 && _elements[middleElement.Column - 1, middleElement.Row] != null)
            adjacentElements.Add(_elements[middleElement.Column - 1, middleElement.Row]);
            
        if (middleElement.Row < rows - 1 && _elements[middleElement.Column, middleElement.Row + 1] != null)
            adjacentElements.Add(_elements[middleElement.Column, middleElement.Row + 1]);
            
        if (middleElement.Row > 0 && _elements[middleElement.Column, middleElement.Row - 1] != null)
            adjacentElements.Add(_elements[middleElement.Column, middleElement.Row - 1]);

        return adjacentElements;
    }

    public void UpdateElementIndices(Element element)
    {
        if (element == null) return;
        _elements[element.Column, element.Row] = element;
    }

    public static Vector2 GetWorldCoords(int column, int row)
    {
        return new Vector2(column * CellSize + _offset.x, row * CellSize + _offset.y);
    }
}