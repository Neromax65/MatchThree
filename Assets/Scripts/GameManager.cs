using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // [SerializeField] private Element elementPrefab;
    // [SerializeField] private Transform grid;

    public static bool GravityInverted = false;

    // private Tile[] _tiles;
    
    
    public enum GameState
    {
        Initializing, PlayingAnimation, WaitingForInput
    }

    public static GameState GameStatus = GameState.Initializing;
    
    private void Awake()
    {
        Instance = this;
    }

    public Tile SelectedTile { get; private set; }

    // public void SelectTile(Tile tile)
    // {
    //     if (SelectedTile != null)
    //     {
    //         if (SelectedTile.IsValidToSwapWith(tile))
    //         {
    //             Grid.Instance.SwapElements(SelectedTile, tile);
    //             SelectNone();
    //         }
    //         else if (SelectedTile == tile)
    //         {
    //             SelectNone();
    //         }
    //         else
    //         {
    //             SelectNone();
    //             SelectedTile = tile;
    //             SelectedTile.HighlightBorder(1f);
    //         }
    //     }
    //     else
    //     {
    //         SelectedTile = tile;
    //         SelectedTile.HighlightBorder(1f);
    //     }
    //     
    //     
    //     // if (SelectedTile != null)
    //     //     SelectedTile.Deselect();
    //
    //     // foreach (var validSwapTile in Grid.ValidSwapTiles)
    //     // {
    //     //     validSwapTile.HighlightBorder(0f);
    //     // }
    //     // Grid.ValidSwapTiles.Clear();
    //     // if (tile.Column < Grid.Tiles.GetLength(0) - 1)
    //     //     Grid.ValidSwapTiles.Add(Grid.Tiles[tile.Column + 1, tile.Row]);
    //     // if (tile.Column > 0)
    //     //     Grid.ValidSwapTiles.Add(Grid.Tiles[tile.Column - 1, tile.Row]);
    //     // if (tile.Row < Grid.Tiles.GetLength(1) - 1)
    //     //     Grid.ValidSwapTiles.Add(Grid.Tiles[tile.Column, tile.Row + 1]);
    //     // if (tile.Row > 0)
    //     //     Grid.ValidSwapTiles.Add(Grid.Tiles[tile.Column, tile.Row - 1]);
    //     // foreach (var validSwapTile in Grid.ValidSwapTiles)
    //     // {
    //     //     validSwapTile.HighlightBorder(0.5f);
    //     // }
    //     // SelectedTile = tile;
    //     // Debug.Log($"Selected Tile [X:{tile.Column}|Y:{tile.Row}]");
    //     // List<Tile> tempMatchedTiles = new List<Tile>();
    //     // var matches = Grid.Instance.GetMatchesForTileRecursive(tile, ref tempMatchedTiles).Count;
    //     // Debug.Log($"Current matches: {matches}");
    // }

    // public void SelectNone()
    // {
    //     SelectedTile.Deselect();
    //     SelectedTile = null;
    // }
    //
    // private void FillField()
    // {
    //     var tiles = FindObjectsOfType<Tile>();
    //     foreach (var tile in tiles)
    //     {
    //         var element = Instantiate(elementPrefab);
    //         tile.AddElement(element);
    //     }
    // }
    //
    // // Start is called before the first frame update
    // void Start()
    // {
    //     // FillField();
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
