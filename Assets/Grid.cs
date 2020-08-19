using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private int rows;
    [SerializeField] private int columns;

    [SerializeField] private Tile tilePrefab;
    private Tile[,] _tiles;

    void InitializeGrid()
    {
        _tiles = new Tile[columns,rows];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                _tiles[column,row] = Instantiate(tilePrefab);
                _tiles[column,row].transform.SetParent(transform);
                _tiles[column,row].UpdatePosition(column, row);
            }
        }
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
        // CheckLines();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckLines()
    {
        Tile firstTile = _tiles[0, 0];
        Tile lastTile = firstTile;
        List<Tile> matchedTiles = new List<Tile>();
        List<Tile> tempMatchedTiles = new List<Tile>();
        
        for (int row = 0; row < rows; row++)
        {
            // int matchedElements = 1;
            
            for (int column = 0; column < columns; column++)
            {
                var curTile = _tiles[column, row];
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
            GameObject.Destroy(tile.Element.gameObject);
        }
        if (matchedTiles.Count > 0)
            Debug.Log($"Destroyed {matchedTiles.Count} tiles!");
        matchedTiles.Clear();

    }
}
