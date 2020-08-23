using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Grid Instance { get; private set; }
    
    
    [SerializeField] private int rows;
    [SerializeField] private int columns;

    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Element elementPrefab;
    public static Tile[,] Tiles;
    public static readonly List<Tile> ValidSwapTiles = new List<Tile>(4);


    private void Awake()
    {
        Instance = this;
    }

    void InitializeGrid()
    {
        Tiles = new Tile[columns, rows];
        toTest = new Tile[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Tiles[x, y] = Instantiate(tilePrefab);
                Tiles[x, y].transform.SetParent(transform);
                Tiles[x, y].Column = x;
                Tiles[x, y].Row = y;
                Tiles[x, y].UpdatePosition();
                Tiles[x, y].gameObject.name = $"Tile [{x}:{y}]]";
                var element = Instantiate(elementPrefab);
                element.SetRandomType();
                Tiles[x, y].AddElement(element);
            }
        }
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
        MatchAndClear(Tiles);
        // CheckLines();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SwapElements(Tile tile1, Tile tile2)
    {
        var tempElement = tile1.Element;
        
        tile1.RemoveElement(false);
        tile1.AddElement(tile2.Element, true);
        
        tile2.RemoveElement(false);
        tile2.AddElement(tempElement, true);
        
        // tile1.MovingAnimationEnded += () => MatchAndClear(Tiles);
        tile2.MovingAnimationEnded += () => MatchAndClear(Tiles);
    }

    public void CheckHorizontalLines()
    {
        Tile firstTile = Tiles[0, 0];
        Tile lastTile = firstTile;
        List<Tile> matchedTiles = new List<Tile>();
        List<Tile> tempMatchedTiles = new List<Tile>();
        
        for (int row = 0; row < rows; row++)
        {
            // int matchedElements = 1;
            
            for (int column = 0; column < columns; column++)
            {
                var curTile = Tiles[column, row];
                if (column > 0)
                {
                    if (curTile == lastTile || curTile.Element == null) continue;
                    if (curTile.Element.Type == lastTile.Element.Type)
                    {
                        if (!tempMatchedTiles.Contains(lastTile))
                            tempMatchedTiles.Add(lastTile);
                        tempMatchedTiles.Add(curTile);
                        Debug.Log($"tempMatchCount: {tempMatchedTiles.Count}");
                        // matchedElements++;
                    }
                    else
                    {
                        if (tempMatchedTiles.Count >= 3)
                        {
                            matchedTiles.AddRange(tempMatchedTiles);
                            Debug.Log($"Match {tempMatchedTiles.Count} of {tempMatchedTiles[0].Element.Type}!");
                        }
                        tempMatchedTiles.Clear();

                    }
                }

                if (column == columns - 1)
                {
                    if (tempMatchedTiles.Count >= 3)
                    {
                        matchedTiles.AddRange(tempMatchedTiles);
                        Debug.Log($"Match {tempMatchedTiles.Count} of {tempMatchedTiles[0].Element.Type}!");
                    }
                    tempMatchedTiles.Clear();
                }

                lastTile = curTile;
            }
            tempMatchedTiles.Clear();
            lastTile = null;

        }

        foreach (var tile in matchedTiles)
        {
            if (tile.Element != null)
                Destroy(tile.Element.gameObject);
        }
        if (matchedTiles.Count > 0)
            Debug.Log($"Destroyed {matchedTiles.Count} tiles!");
        matchedTiles.Clear();

    }


    void CopyGrid(Tile[,] source, Tile[,] destination)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                destination[x, y] = source[x, y];
            }
        }
    }

    private bool clearedTiles = false;


    private Tile[,] toTest;
    Tile currentTile = null;
    List<Tile> matchedTiles = new List<Tile>();
    
    void MatchAndClear(Tile[,] board)
    {
        clearedTiles = false;
        CopyGrid(board, toTest);
        
        currentTile = null;
        matchedTiles.Clear();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                CheckTileRecursive(x, y);
                if (matchedTiles.Count >= 3)
                {
                    foreach (var tile in matchedTiles)
                    {
                        ClearTile(tile.Column, tile.Row);
                        clearedTiles = true;
                    }
                }
                currentTile = null;
                matchedTiles.Clear();
            }
        }
        if (clearedTiles)
        {
            StartCoroutine(DropTiles(board));
        }
    }


    void CheckTileRecursive(int x, int y)
    {
        if (toTest[x,y] == null || toTest[x,y].Element == null) return;

        if (currentTile == null)
        {
            currentTile = toTest[x, y];
            toTest[x, y] = null;
            matchedTiles.Add(currentTile);
        } else if (currentTile.Element.Type != toTest[x, y].Element.Type)
        {
            return;
        }
        else
        {
            matchedTiles.Add(toTest[x,y]);
            toTest[x, y] = null;
        }

        if (x > 0) CheckTileRecursive(x - 1, y);
        if (x < columns - 1) CheckTileRecursive(x + 1, y);
        if (y > 0) CheckTileRecursive(x, y - 1);
        if (y < rows - 1) CheckTileRecursive(x, y + 1);
    }

    void ClearTile(int x, int y)
    {
        // Destroy(Tiles[x, y].Element.gameObject);
        Tiles[x,y].RemoveElement(true);
    }

    private int? firstEmpty;
    IEnumerator DropTiles(Tile[,] board)
    {
        for (int x = 0; x < columns; x++)
        {
            firstEmpty = null;
            for (int y = 0; y < rows; y++)
            {
                if (board[x, y].Element == null && !firstEmpty.HasValue)
                {
                    firstEmpty = y;
                }
                else if (firstEmpty.HasValue && board[x, y].Element != null)
                {
                    var element = board[x, y].Element;
                    element.transform.SetParent(FindObjectOfType<Canvas>().transform);
                    Vector2 startPosition = element.transform.position;
                    Vector2 targetPosition = board[x, firstEmpty.Value].transform.position;
                    // for (float t = 0; t < 1; t+= Time.deltaTime)
                    // {
                    float t = 0;
                        while (Vector2.Distance(element.transform.position, targetPosition) > 0.1f)
                        {
                            element.transform.position = Vector2.Lerp(element.transform.position, targetPosition, t);
                            t += Time.deltaTime;
                            yield return null;
                        }
                    // }
                    board[x, firstEmpty.Value].AddElement(board[x,y].Element, false);
                    board[x,y].RemoveElement(false);
                    
                    // board[x, firstEmpty.Value] = board[x, y];
                    // board[x, y].Element = null;
                    
                    firstEmpty++;
                }
            }

            // yield return null;
        }
        MatchAndClear(board);
        // UpdateIndexes(false);
    }
    
    void UpdateIndexes(bool updatePositions)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (Tiles[x,y] != null)
                {
                    Tiles[x, y].Row = y;
                    Tiles[x, y].Column = x;
                    if (updatePositions)
                        Tiles[x, y].UpdatePosition();
                }
            }
        }
    }
}
