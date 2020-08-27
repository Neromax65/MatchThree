using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Цветной элемент, располагаемый на игровом поле
/// </summary>
public class Element : MonoBehaviour
{
    [SerializeField] private Image mainIcon;
    [SerializeField] private Image invertGravityIcon;
    // [SerializeField] private Animator animator;
        
    /// <summary>
    /// Индекс колонки элемента, его координата X в рамках игрового поля
    /// </summary>
    public int Column;
        
    /// <summary>
    /// Индекс ряда элемента, его координата Y в рамках игрового поля
    /// </summary>
    public int Row;

    private static readonly int Match1 = Animator.StringToHash("Match");

    // Перечисление возможных цветов/типов элементов
    public enum ElementType
    {
        Red, Green, Blue, Yellow, Cyan, Magenta
    }

    /// <summary>
    /// Тип/цвет элемента
    /// </summary>
    public ElementType Type { get; private set; }

    public void EnableGravityInverting()
    {
        invertGravityIcon.enabled = true;
    }

    public void SetType(ElementType type)
    {
        Type = type;
        UpdateColor();
    }
        
    public static ElementType GetRandomType()
    {
        return (ElementType)Random.Range(0, Enum.GetNames(typeof(ElementType)).Length);
    }


    public static Element SpawnRandom(int column, int row)
    {
        Vector2 spawnPosition = Grid.GetWorldCoords(column, row);
        var element = Instantiate(Grid.Instance.elementPrefab, spawnPosition, Quaternion.identity, Grid.Instance.transform);
        element.Column = column;
        element.Row = row;
        element.gameObject.name = $"Element [{column},{row}][{element.Type}]";
        
        element.SetType((ElementType)Random.Range(0, Enum.GetNames(typeof(ElementType)).Length));
            
        return element;
    }
        
    public static Element SpawnInverseGravity(int column, int row, ElementType type)
    {
        Vector2 spawnPosition = Grid.GetWorldCoords(column, row);
        var element = Instantiate(Grid.Instance.elementPrefab, spawnPosition, Quaternion.identity, Grid.Instance.transform);
        element.Column = column;
        element.Row = row;
        element.gameObject.name = $"Element [{column},{row}][{element.Type}]";
        
        element.SetType(type);
        element.EnableGravityInverting();
            
        return element;
    }

    void UpdateColor()
    {
        switch (Type)
        {
            case ElementType.Red:
                mainIcon.color = Color.red;
                break;
            case ElementType.Green:
                mainIcon.color = Color.green;
                break;
            case ElementType.Blue:
                mainIcon.color = Color.blue;
                break;
            case ElementType.Yellow:
                mainIcon.color = Color.yellow;
                break;
            case ElementType.Cyan:
                mainIcon.color = Color.cyan;
                break;            
            case ElementType.Magenta:
                mainIcon.color = Color.magenta;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
        
        
    /// <summary>
    /// Засчитывание элемента как совпавшего и удаление его из игры
    /// </summary>
    public void Match()
    {
        mainIcon.raycastTarget = false;
        invertGravityIcon.raycastTarget = false;
        
        // var animator = GetComponent<Animator>();
        // animator.SetTrigger(Match1);
        //
        // // yield return new WaitWhile(animator.GetCurrentAnimatorStateInfo(0).);
        //
        //
        // yield return new WaitForSeconds(0.1f);
        //
        // // while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        // // {
        // //     yield return null;
        // // }
        // //
        
        if (invertGravityIcon.enabled)
            GameManager.GravityInverted = !GameManager.GravityInverted;
        Destroy(gameObject);
        // yield break;
    }

    private void Update()
    {
        // TODO: Test
        // GetComponentInChildren<Text>().text = $"[{Column},{Row}]";
        gameObject.name = $"Element [{Column},{Row}][{Type}]";
    }

    public IEnumerator SettleWorldPosition()
    {
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = Grid.GetWorldCoords(Column, Row);
        for (float t = 0; t < 1; t += Time.deltaTime * 2)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }

    public IEnumerator Swap(Element otherElement)
    {
        GameManager.GameStatus = GameStatus.PlayingAnimation;

        using (var transaction = new SwapTransaction(this, otherElement))
        {
            Coroutine swap1 = StartCoroutine(SettleWorldPosition());
            Coroutine swap2 = StartCoroutine(otherElement.SettleWorldPosition());

            yield return swap1;
            yield return swap2;

            int matches = Grid.Instance.CalculateMatches(false);
            if (matches > 0)
            {
                // Grid.Instance.UpdateElementIndices(this);
                // Grid.Instance.UpdateElementIndices(otherElement);
                Grid.Instance.CalculateMatches(true);
                transaction.Commit();
                yield break;
            }
        }
        
        Coroutine returnSwap1 = StartCoroutine(SettleWorldPosition());
        Coroutine returnSwap2 = StartCoroutine(otherElement.SettleWorldPosition());

        yield return returnSwap1;
        yield return returnSwap2;
        
        GameManager.GameStatus = GameStatus.WaitingForInput;
    }

    public bool IsAdjacentTo(Element otherElement)
    {
        return Mathf.Abs(otherElement.Column - Column) + Mathf.Abs(otherElement.Row - Row) == 1;
    }
}