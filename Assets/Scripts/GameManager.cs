using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Element elementPrefab;
    [SerializeField] private Transform grid;
    private Tile[] _tiles;
    
    private void Awake()
    {
        Instance = this;
    }

    public Tile SelectedTile { get; private set; }

    public void SelectTile(Tile tile)
    {
        if (SelectedTile != null)
            SelectedTile.Deselect();
        SelectedTile = tile;
    }

    private void FillField()
    {
        var tiles = FindObjectsOfType<Tile>();
        foreach (var tile in tiles)
        {
            var element = Instantiate(elementPrefab);
            tile.AddElement(element);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        FillField();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
