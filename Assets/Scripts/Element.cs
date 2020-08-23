using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Element : MonoBehaviour
{
    [SerializeField] private Image image;
    public bool IsMoving = false;
    
    
    private void OnValidate()
    {
        image = GetComponentInChildren<Image>();
    }

    public enum ElementType
    {
        Red, Green, Blue, Yellow, Cyan, Magenta
    }

    private void Start()
    {
        // SetRandomType();
    }

    public ElementType Type { get; private set; } 
    

    public void SetRandomType()
    {
        Type = (ElementType)Random.Range(0, Enum.GetNames(typeof(ElementType)).Length);
        switch (Type)
        {
            case ElementType.Red:
                image.color = Color.red;
                break;
            case ElementType.Green:
                image.color = Color.green;
                break;
            case ElementType.Blue:
                image.color = Color.blue;
                break;
            case ElementType.Yellow:
                image.color = Color.yellow;
                break;
            case ElementType.Cyan:
                image.color = Color.cyan;
                break;            
            case ElementType.Magenta:
                image.color = Color.magenta;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
