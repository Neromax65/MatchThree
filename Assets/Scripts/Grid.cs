using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

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
        tempTiles = new Tile[columns, rows];
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

        const int maxIterations = 4000;
        int iteration = 0;
        while (CountMatchedTiles(Tiles) > 0 || !CheckForPossibleMoves(Tiles))
        {
            if (iteration > maxIterations)
            {
                // Debug.LogError("Exceed maximum numbers of iterations.");
                throw new OverflowException("Exceed maximum numbers of iterations.");
                break;
            }
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Tiles[x,y].Element.SetRandomType();
                }
            }

            iteration++;
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
        List<Tile> tempMatchedTiles = new List<Tile>();
        
        using (new SwapTransaction(tile1, tile2))
        {
            int tile1Matches = GetMatchesForTileRecursive(tile1, ref tempMatchedTiles).Count;
            tempMatchedTiles.Clear();
            int tile2Matches = GetMatchesForTileRecursive(tile2, ref tempMatchedTiles).Count;
            
            if (tile1Matches < 3 && tile2Matches < 3)
                return;
        }
        
        
        var tempElement = tile1.Element;
        
        tile1.RemoveElement(false);
        tile1.AddElement(tile2.Element, true);
        
        tile2.RemoveElement(false);
        tile2.AddElement(tempElement, true);
        
        // tile1.MovingAnimationEnded += () => MatchAndClear(Tiles);
        tile2.MovingAnimationEnded += () => MatchAndClear(Tiles);
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


    private Tile[,] tempTiles;
    Tile currentTile = null;
    List<Tile> matchedTiles = new List<Tile>();
    
    void MatchAndClear(Tile[,] board)
    {
        Debug.Log("MatchAndClear");
        
        clearedTiles = false;
        CopyGrid(board, tempTiles);
        
        currentTile = null;
        matchedTiles.Clear();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                CheckTileRecursive(x, y);
                if (matchedTiles.Count >= 3)
                {
                    var matchedType = matchedTiles.First().Element.Type;
                    
                    foreach (var tile in matchedTiles)
                    {
                        ClearTile(tile.Column, tile.Row);
                        clearedTiles = true;
                    }

                    if (matchedTiles.Count >= 4)
                    {
                        var reverseGravityElement = Instantiate(elementPrefab);
                        reverseGravityElement.SetType(matchedType);
                        reverseGravityElement.MarkReverseGravity();
                        matchedTiles[Random.Range(0, matchedTiles.Count)].AddElement(reverseGravityElement);
                    }
                }
                currentTile = null;
                matchedTiles.Clear();
            }
        }

        if (clearedTiles)
        {
            DropTiles(board);
        }
        //     StartCoroutine(DropTiles(board));
        // }

    }
    
    int CountMatchedTiles(Tile[,] board)
    {
        CopyGrid(board, tempTiles);
        
        currentTile = null;
        matchedTiles.Clear();

        int matches = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                CheckTileRecursive(x, y);
                if (matchedTiles.Count >= 3)
                {
                    matches += matchedTiles.Count;
                }
                currentTile = null;
                matchedTiles.Clear();
            }
        }

        return matches;
    }


    void CheckTileRecursive(int x, int y)
    {
        if (tempTiles[x,y] == null || tempTiles[x,y].Element == null) return;

        if (currentTile == null)
        {
            currentTile = tempTiles[x, y];
            tempTiles[x, y] = null;
            matchedTiles.Add(currentTile);
        } else if (currentTile.Element.Type != tempTiles[x, y].Element.Type)
        {
            return;
        }
        else
        {
            matchedTiles.Add(tempTiles[x,y]);
            tempTiles[x, y] = null;
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
    void DropTiles(Tile[,] board)
    {
        coroutineEnded = false;
        Debug.Log("DropTiles");
        Coroutine coroutine = null;
        for (int x = 0; x < columns; x++)
        {
            coroutine = StartCoroutine(DropTilesY(board, x, GameManager.GravityReversed));

            // yield return null;
        }

        // yield return new WaitUntil(() => coroutineEnded);
        
        // if (CountEmptyTiles(board) > 0)
        //     SpawnNewTiles(board);
        // else
        //     MatchAndClear(board);

        // yield return null;
        // UpdateIndexes(false);
        // yield break;
    }

    private bool coroutineEnded;

    private IEnumerator DropTilesY(Tile[,] board, int x, bool inverted)
    {
        Debug.Log("DropTilesY");
        firstEmpty = null;
        if (inverted)
        {
            for (int y = rows - 1; y >= 0; y--)
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
                    for (float t = 0; t < 1; t += Time.deltaTime * 3)
                    {
                        element.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                        yield return null;
                    }

                    board[x, firstEmpty.Value].AddElement(board[x, y].Element, false);
                    board[x, y].RemoveElement(false);

                    firstEmpty--;
                }

                yield return null;
            }
        }
        else
        {
            for (int y = 0; y < rows; y++)
            {
                var element = board[x, y].Element;
                if (element == null && !firstEmpty.HasValue)
                {
                    firstEmpty = y;
                }
                else if (element != null && firstEmpty.HasValue)
                {
                    element.transform.SetParent(FindObjectOfType<Canvas>().transform);
                    Vector2 startPosition = element.transform.position;
                    Vector2 targetPosition = board[x, firstEmpty.Value].transform.position;
                    for (float t = 0; t < 1; t += Time.deltaTime * 3)
                    {
                        element.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                        yield return null;
                    }

                    board[x, firstEmpty.Value].AddElement(board[x, y].Element, false);
                    board[x, y].RemoveElement(false);

                    firstEmpty++;
                } 
                yield return null;
            }
        }

        if (x == columns - 1)
        {
            if (CountEmptyTiles(board) > 0)
                SpawnNewTiles(board);
            else
                MatchAndClear(board);
        }
    }

    // List<Tile> tempMatchedTiles = new List<Tile>();
    public List<Tile> GetMatchesForTileRecursive(Tile tile, ref List<Tile> tempMatchedTiles)
    {
        Tile tileToCheck = null;
        tempMatchedTiles.Add(tile);
        if (tile.Column > 0)
        {
            tileToCheck = Tiles[tile.Column - 1, tile.Row];
            if (tileToCheck.Element != null && tile.Element != null && tileToCheck.Element.Type == tile.Element.Type && !tempMatchedTiles.Contains(tileToCheck))
            {
                GetMatchesForTileRecursive(tileToCheck, ref tempMatchedTiles);
            }
        }

        if (tile.Column < columns - 1)
        {
            tileToCheck = Tiles[tile.Column + 1, tile.Row];
            if (tileToCheck.Element != null && tile.Element != null && tileToCheck.Element.Type == tile.Element.Type && !tempMatchedTiles.Contains(tileToCheck))
            {
                GetMatchesForTileRecursive(tileToCheck, ref tempMatchedTiles);
            }
        }
        
        if (tile.Row > 0)
        {
            tileToCheck = Tiles[tile.Column, tile.Row - 1];
            if (tileToCheck.Element != null && tile.Element != null && tileToCheck.Element.Type == tile.Element.Type && !tempMatchedTiles.Contains(tileToCheck))
            {
                GetMatchesForTileRecursive(tileToCheck, ref tempMatchedTiles);
            }
        }
        
        if (tile.Row < rows - 1)
        {
            tileToCheck = Tiles[tile.Column, tile.Row + 1];
            if (tileToCheck.Element != null && tile.Element != null && tileToCheck.Element.Type == tile.Element.Type && !tempMatchedTiles.Contains(tileToCheck))
            {
                GetMatchesForTileRecursive(tileToCheck, ref tempMatchedTiles);
            }
        }

        return tempMatchedTiles;
    }
    
    void SpawnNewTiles(Tile[,] board)
    {
        Debug.Log("SpawnNewTiles");
        for (int x = 0; x < columns; x++)
        {
            if (GameManager.GravityReversed)
            {
                if (board[x, 0].Element == null)
                {
                    var element = Instantiate(elementPrefab);
                    element.SetRandomType();
                    board[x, 0].AddElement(element);
                }
            } else {
                if (board[x, rows - 1].Element == null)
                {
                    var element = Instantiate(elementPrefab);
                    element.SetRandomType();
                    board[x, rows - 1].AddElement(element);
                }
            }
        }

        DropTiles(board);
        // StartCoroutine(DropTiles(board));
    }


    int CountEmptyTiles(Tile[,] board)
    {
        List<Tile> emptyTiles = new List<Tile>();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (board[x,y].Element == null)
                    emptyTiles.Add(board[x,y]);
            }
        }

        Debug.Log($"CountEmptyTiles: {emptyTiles.Count}");
        return emptyTiles.Count;
    }

    bool CheckForPossibleMoves(Tile[,] board)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var tile = board[x, y];
                if (tile.Column > 0)
                {
                    using (new SwapTransaction(tile, board[tile.Column - 1, tile.Row]))
                    {
                        var tmpMatchedTiles = new List<Tile>();
                        var matchesCount = GetMatchesForTileRecursive(tile, ref tmpMatchedTiles).Count;
                        if (matchesCount >= 3)
                            return true;
                    }
                }
                if (tile.Column < columns - 1)
                {
                    using (new SwapTransaction(tile, board[tile.Column + 1, tile.Row]))
                    {
                        var tmpMatchedTiles = new List<Tile>();
                        var matchesCount = GetMatchesForTileRecursive(tile, ref tmpMatchedTiles).Count;
                        if (matchesCount >= 3)
                            return true;
                    }
                }
                if (tile.Row > 0)
                {
                    using (new SwapTransaction(tile, board[tile.Column, tile.Row - 1]))
                    {
                        var tmpMatchedTiles = new List<Tile>();
                        var matchesCount = GetMatchesForTileRecursive(tile, ref tmpMatchedTiles).Count;
                        if (matchesCount >= 3)
                            return true;
                    }
                }
                if (tile.Row < rows - 1)
                {
                    using (new SwapTransaction(tile, board[tile.Column, tile.Row + 1]))
                    {
                        var tmpMatchedTiles = new List<Tile>();
                        var matchesCount = GetMatchesForTileRecursive(tile, ref tmpMatchedTiles).Count;
                        if (matchesCount >= 3)
                            return true;
                    }
                }
            }
        }

        return false;
    }
}
