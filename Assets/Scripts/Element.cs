using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Element : MonoBehaviour
{
    [SerializeField] private Image hexagon;
    [SerializeField] private Image topDown;
    public bool IsMoving = false;
    
    
    private void OnValidate()
    {
        hexagon = GetComponentInChildren<Image>();
    }

    public enum ElementType
    {
        Red, Green, Blue, Yellow, Cyan, Magenta
    }

    public void MarkReverseGravity()
    {
        topDown.enabled = true;
    }

    public void Match()
    {
        if (topDown.enabled)
            GameManager.GravityReversed = !GameManager.GravityReversed;
        Destroy(gameObject);
    }

    private void Start()
    {
        // SetRandomType();
    }

    public ElementType Type { get; private set; }

    public void SetType(ElementType type)
    {
        Type = type;
        AlignColor(Type);
    }

    public void SetRandomType()
    {
        Type = (ElementType)Random.Range(0, Enum.GetNames(typeof(ElementType)).Length);
        AlignColor(Type);
    }

    void AlignColor(ElementType type)
    {
        switch (type)
        {
            case ElementType.Red:
                hexagon.color = Color.red;
                break;
            case ElementType.Green:
                hexagon.color = Color.green;
                break;
            case ElementType.Blue:
                hexagon.color = Color.blue;
                break;
            case ElementType.Yellow:
                hexagon.color = Color.yellow;
                break;
            case ElementType.Cyan:
                hexagon.color = Color.cyan;
                break;            
            case ElementType.Magenta:
                hexagon.color = Color.magenta;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
