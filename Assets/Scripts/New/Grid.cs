using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace New
{
    public class Grid : MonoBehaviour
    {
        public static Grid Instance;

        public static Vector2 Offset;
        public static int CellSize = 80;

        public int Columns;
        public int Rows;

        [SerializeField] private Element elementPrefab;

        public Element[,] Elements { get; private set; }

        // public event Action<Element, Element> SwapStarted;
        // public event Action<Element, Element> SwapEnded;

        private void Awake()
        {
            Instance = this;
            Offset = Camera.main.WorldToScreenPoint(-new Vector2(Columns/2, Rows/2));
        }

        private void Start()
        {
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            Elements = new Element[Columns,Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    Vector2 spawnPosition = new Vector2(x * CellSize + Offset.x, y * CellSize + Offset.y);
                    Elements[x, y] = Instantiate(elementPrefab, spawnPosition, Quaternion.identity, transform);
                    Elements[x, y].Column = x;
                    Elements[x, y].Row = y;
                    // Elements[x, y].Type = (Element.ElementType) Random.Range(0, Enum.GetNames(typeof(Element.ElementType)).Length);
                    Elements[x,y].SetRandomType();
                    Elements[x, y].gameObject.name = $"Element [{x},{y}][{Elements[x, y].Type}]";
                }
            }
        }


        public void CheckMatchesAll(Element[,] elements)
        {
            bool matched = false;
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    if (elements[x,y] == null) continue;

                    List<Element> matches = CheckMatches(elements, elements[x, y]);
                    if (matches.Count >= 3)
                    {
                        matched = true;
                        foreach (var element in matches)
                        {
                            elements[element.Column, element.Row].Match();
                            elements[element.Column, element.Row] = null;
                        }
                    }
                }
            }
            if (matched) DropElementsAll(elements, false);
        }

        public void DropElementsAll(Element[,] elements, bool reversedGravity)
        {
            bool atLestOneElementDropped = false;
            for (int x = 0; x < Columns; x++)
            {
                int? firstEmptyY = null;
                if (!reversedGravity) 
                    for (int y = 0; y < Rows; y++)
                    {
                        if (DropElement(elements, x, y, ref firstEmptyY))
                            atLestOneElementDropped = true;
                    }
                else
                    for (int y = Rows - 1; y >= 0; y--)
                    {
                        if (DropElement(elements, x, y, ref firstEmptyY))
                            atLestOneElementDropped = true;                    }
            }

            if (atLestOneElementDropped) FallAnimation(elements);
        }

        private bool DropElement(Element[,] elements, int x, int y, ref int? firstEmptyY)
        {
            if (!firstEmptyY.HasValue && elements[x, y] == null)
            {
                firstEmptyY = y;
            }
            else if (firstEmptyY.HasValue && elements[x, y] != null)
            {
                elements[x, firstEmptyY.Value] = elements[x,y];
                elements[x, y] = null;
                elements[x, firstEmptyY.Value].Row = firstEmptyY.Value;
                elements[x, firstEmptyY.Value].IsFalling = true;
                // elements[x, firstEmptyY.Value].UpdatePosition(true);
                
                firstEmptyY++;

                return true;
            }

            return false;
        }
        
        public List<Element> CheckMatches(Element[,] elements, Element element)
        {
            List<Element> matchedElements = new List<Element>();
            CheckMatchesRecursive(elements, element, ref matchedElements);
            return matchedElements;
        }

        // public static void OnSwapEnded(Element element1, Element element2)
        // {
        //     
        // }
        
        void CheckMatchesRecursive(Element[,] elements, Element element, ref List<Element> matchedElements)
        {
            if (element == null)
                return;
            
            matchedElements.Add(element);

            // elements[element.Row, element.Column] = null;

            if (element.Column < Columns - 1)
            {
                var checkedElement = elements[element.Column + 1, element.Row];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(elements, checkedElement, ref matchedElements);
            }            
            if (element.Column > 0)
            {
                var checkedElement = elements[element.Column - 1, element.Row];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(elements, checkedElement, ref matchedElements);
            }            
            if (element.Row < Rows - 1)
            {
                var checkedElement = elements[element.Column, element.Row + 1];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(elements, checkedElement, ref matchedElements);
            }   
            if (element.Row > 0)
            {
                var checkedElement = elements[element.Column, element.Row - 1];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(elements, checkedElement, ref matchedElements);
            }

            // if (element.Column > 0 && Elements[element.Column - 1, element.Row] != null && Elements[element.Column - 1, element.Row].Type == element.Type && !matchedElements.Contains(Elements[element.Column - 1, element.Row]))
            //     CheckMatchesRecursive(Elements[element.Column - 1, element.Row], ref matchedElements);
            //
            // if (element.Row < Rows - 1  && Elements[element.Column, element.Row + 1] != null && Elements[element.Column, element.Row + 1].Type == element.Type && !matchedElements.Contains(Elements[element.Column, element.Row + 1]))
            //     CheckMatchesRecursive(Elements[element.Column, element.Row + 1], ref matchedElements);
            //
            // if (element.Row > 0 && Elements[element.Column, element.Row - 1] != null && Elements[element.Column, element.Row - 1].Type == element.Type && !matchedElements.Contains(Elements[element.Column, element.Row - 1]))
            //     CheckMatchesRecursive(Elements[element.Column, element.Row - 1], ref matchedElements);
            
            // for (int y = -1; y <= 1; y += 2)
            // {
            //     for (int x = -1; x <= 1; x += 2)
            //     {
            //         if (x == 0 || y == 0 || element.Column + x <= 0 || element.Column + x >= Columns 
            //         || element.Row + y <= 0 || element.Row + y >= Rows)
            //             continue;
            //         
            //         var checkedElement = Elements[element.Column + x, element.Row + y];
            //         if (checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
            //         {
            //             CheckMatchesRecursive(checkedElement, ref matchedElements);
            //         }
            //     }
            // }
        }
        
        public IEnumerator SwapAnimation(Element element1, Element element2, bool revert = false)
        {
            // TODO: 64?
            Vector2 targetPosition1 = new Vector2(element1.Column * CellSize + Offset.x, element1.Row * CellSize + Offset.y);
            Vector2 targetPosition2 = new Vector2(element2.Column * CellSize + Offset.x, element2.Row * CellSize + Offset.y);
            
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                element1.transform.position = Vector2.Lerp(element1.transform.position, targetPosition1, t);
                element2.transform.position = Vector2.Lerp(element2.transform.position, targetPosition2, t);
                yield return null;
            }
            
            element1.transform.position = targetPosition1;
            element2.transform.position = targetPosition2;
        
            if (revert) yield break;
            
            // TODO: В отдельный метод + DRY
            // Element[,] tempArray = new Element[Elements.GetLength(0), Elements.GetLength(1)];
            // Array.Copy(Elements, tempArray, Elements.Length);
            var element1Matches = CheckMatches(Elements, element1);
            var element2Matches = CheckMatches(Elements, element2);
        
            // TODO: 3?
        
            bool matched = false;
            
            if (element1Matches.Count >= 3)
            {
                foreach (var element in element1Matches)
                {
                    Elements[element.Column, element.Row].Match();
                    Elements[element.Column, element.Row] = null;
                }
        
                matched = true;
            }
            
            if (element2Matches.Count >= 3)
            {
                foreach (var element in element2Matches)
                {
                    // if (element == null) continue;
                    if (Elements[element.Column, element.Row] == null) continue;
                    Elements[element.Column, element.Row].Match();
                    Elements[element.Column, element.Row] = null;
                }
        
                matched = true;
            }
        
            if (matched)
            {
                DropElementsAll(Elements, false);
            } else
            {
                element1.Swap(element2, true);
            }
            
        }


        public void FallAnimation(Element[,] elements)
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {

                    // bool last = (x == Columns - 1 && y == Rows - 1);
                    // var element = elements[x, y];
                    // if (element != null)
                    StartCoroutine(FallAnimationOne(elements, x, y));
                    // var element = elements[x, y];
                    //
                    // if (element != null && element.IsFalling)
                    // {
                    //     Vector2 targetPosition = new Vector2(element.transform.position.x, element.Row * CellSize + Offset.y);
                    //     for (float t = 0; t < 1; t += Time.deltaTime)
                    //     {
                    //         element.transform.position = Vector2.Lerp(element.transform.position, targetPosition, t);
                    //     }
                    //
                    //     element.transform.position = targetPosition;
                    //     element.IsFalling = false;
                    //     yield return null;
                    // }

                    // yield return null;
                }
            }
            
            // CheckMatchesAll(elements);
        }

        IEnumerator FallAnimationOne(Element[,] elements, int x, int y)
        {
                
            var element = elements[x, y];
            // if (element == null || elements[x,y] == null || !element.IsFalling) yield break;

            if (element != null && elements[x,y] != null && element.IsFalling)
            {

                Vector2 startPosition = element.transform.position;
                Vector2 targetPosition = new Vector2(startPosition.x, element.Row * CellSize + Offset.y);
                for (float t = 0; t < 1; t += Time.deltaTime * 1.75f)
                {
                    if (element == null) break;
                    element.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                    yield return null;
                }

                if (element != null)
                {
                    element.transform.position = targetPosition;
                    element.IsFalling = false;
                }
            }
            if (x == Columns - 1 && y == Rows - 1) CheckMatchesAll(elements);
            // yield return null;
        }
    }
}