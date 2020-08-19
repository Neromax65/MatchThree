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

    private void OnValidate()
    {
        image = GetComponentInChildren<Image>();
    }

    public enum ElementType
    {
        Red, Green, Blue
    }

    private void Start()
    {
        SetRandomType();
    }

    public ElementType Type { get; private set; } 
    

    private void SetRandomType()
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
