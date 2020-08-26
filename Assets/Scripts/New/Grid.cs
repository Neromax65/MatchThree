using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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

            const int exceedLimit = 4000;
            int counter = 0;
            while (CheckMatchesAll(false) > 0 || !CheckForPossibleMoves())
            {
                if (counter >= exceedLimit)
                {
                    throw new StackOverflowException();
                }
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        Elements[x,y].SetRandomType();
                    }
                }

                counter++;
            }

            GameManager.GameStatus = GameManager.GameState.WaitingForInput;
        }
        
        public bool CheckForPossibleMoves()
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    var element = Elements[x, y];
                    if (element == null) continue;
                    
                    if (element.Column > 0 && Elements[element.Column - 1, element.Row] != null)
                    {
                        using (new SwapTransaction(element, Elements[element.Column - 1, element.Row]))
                        {
                            var matchesCount = CheckMatches(element).Count;
                            if (matchesCount >= 3) return true;
                        }
                    }
                    if (element.Column < Columns - 1 && Elements[element.Column + 1, element.Row] != null)
                    {
                        using (new SwapTransaction(element, Elements[element.Column + 1, element.Row]))
                        {
                            var matchesCount = CheckMatches(element).Count;
                            if (matchesCount >= 3) return true;
                        }
                    }
                    if (element.Row > 0 && Elements[element.Column, element.Row - 1] != null)
                    {
                        using (new SwapTransaction(element, Elements[element.Column, element.Row - 1]))
                        {
                            var matchesCount = CheckMatches(element).Count;
                            if (matchesCount >= 3) return true;
                        }
                    }
                    if (element.Row < Rows - 1 && Elements[element.Column, element.Row + 1] != null)
                    {
                        using (new SwapTransaction(element, Elements[element.Column, element.Row + 1]))
                        {
                            var matchesCount = CheckMatches(element).Count;
                            if (matchesCount >= 3) return true;
                        }
                    }
                }
            }
            return false;
        }


        public int CheckMatchesAll(bool destroyAndFall = true)
        {
            int totalMatchCount = 0;
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    if (Elements[x,y] == null) continue;

                    List<Element> matches = CheckMatches(Elements[x, y]);
                    if (matches.Count >= 3)
                    {
                        totalMatchCount += matches.Count;

                        
                        if (destroyAndFall)
                        {
                            Element.ElementType matchedType = matches[0].Type;
                            int randomMatchedIndex = Random.Range(0, matches.Count);
                            int randomMatchedCol = matches[randomMatchedIndex].Column;
                            int randomMatchedRow = matches[randomMatchedIndex].Row;
                            
                            foreach (var element in matches)
                            {
                                Elements[element.Column, element.Row].Match();
                                Elements[element.Column, element.Row] = null;
                            }
                            if (matches.Count >= 4)
                            {
                                Vector2 spawnPosition = new Vector2(randomMatchedCol * CellSize + Offset.x, randomMatchedRow * CellSize + Offset.y);
                                Elements[randomMatchedCol, randomMatchedRow] = Instantiate(elementPrefab, spawnPosition, quaternion.identity, transform);
                                Elements[randomMatchedCol, randomMatchedRow].Column = randomMatchedCol; 
                                Elements[randomMatchedCol, randomMatchedRow].Row = randomMatchedRow; 
                                Elements[randomMatchedCol, randomMatchedRow].SetType(matchedType); 
                                Elements[randomMatchedCol, randomMatchedRow].EnableGravityInverting(); 
                            }
                        }

                    }
                }
            }

            if (destroyAndFall && totalMatchCount > 0)
                DropElementsAll();
            else if (totalMatchCount == 0)
                GameManager.GameStatus = GameManager.GameState.WaitingForInput;

            return totalMatchCount;
        }

        public void DropElementsAll()
        {
            bool atLeastOneElementDropped = false;
            for (int x = 0; x < Columns; x++)
            {
                int? firstEmptyY = null;
                if (!GameManager.GravityInverted) 
                    for (int y = 0; y < Rows; y++)
                    {
                        if (DropElement(Elements, x, y, ref firstEmptyY))
                            atLeastOneElementDropped = true;
                    }
                else
                    for (int y = Rows - 1; y >= 0; y--)
                    {
                        if (DropElement(Elements, x, y, ref firstEmptyY))
                            atLeastOneElementDropped = true;                    }
            }

            if (atLeastOneElementDropped)
            {
                GameManager.GameStatus = GameManager.GameState.PlayingAnimation;
                StartCoroutine(FallAnimation());
            }
            else
            {
                if (!IsGridFull())
                    SpawnNewElements();
                else
                    CheckMatchesAll();
            }
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

                firstEmptyY = GameManager.GravityInverted ? --firstEmptyY : ++firstEmptyY;
                
                return true;
            }

            return false;
        }

        public void SpawnNewElements()
        {
            int y = GameManager.GravityInverted ? 0 : Rows - 1;
            bool spawned = false;
            for (int x = 0; x < Columns; x++)
            {
                if (Elements[x, y] == null)
                {
                    Vector2 spawnPos = new Vector2(x * CellSize + Offset.x, y * CellSize + Offset.y);
                    Elements[x, y] = Instantiate(elementPrefab, spawnPos, Quaternion.identity, transform);
                    Elements[x, y].Column = x;
                    Elements[x, y].Row = y;
                    Elements[x, y].SetRandomType();
                    spawned = true;
                }
            }

            if (spawned)
            {
                DropElementsAll();
            }
            else
            {
                Debug.Log("Didn`t spawn any element. Force check matches.");
                CheckMatchesAll();
            }
        }
        
        public List<Element> CheckMatches(Element element)
        {
            List<Element> matchedElements = new List<Element>();
            CheckMatchesRecursive(element, ref matchedElements);
            return matchedElements;
        }

        // public static void OnSwapEnded(Element element1, Element element2)
        // {
        //     
        // }
        
        void CheckMatchesRecursive(Element element, ref List<Element> matchedElements)
        {
            if (element == null)
                return;
            
            matchedElements.Add(element);

            if (element.Column < Columns - 1)
            {
                var checkedElement = Elements[element.Column + 1, element.Row];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(checkedElement, ref matchedElements);
            }            
            if (element.Column > 0)
            {
                var checkedElement = Elements[element.Column - 1, element.Row];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(checkedElement, ref matchedElements);
            }            
            if (element.Row < Rows - 1)
            {
                var checkedElement = Elements[element.Column, element.Row + 1];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(checkedElement, ref matchedElements);
            }   
            if (element.Row > 0)
            {
                var checkedElement = Elements[element.Column, element.Row - 1];
                if (checkedElement != null && checkedElement.Type == element.Type && !matchedElements.Contains(checkedElement))
                    CheckMatchesRecursive(checkedElement, ref matchedElements);
            }
        }
        
        public IEnumerator SwapAnimation(Element element1, Element element2, bool revert = false)
        {
            GameManager.GameStatus = GameManager.GameState.PlayingAnimation;
            
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

            if (revert)
            {
                GameManager.GameStatus = GameManager.GameState.WaitingForInput;
                yield break;
            }
            
            // TODO: В отдельный метод + DRY
            // Element[,] tempArray = new Element[Elements.GetLength(0), Elements.GetLength(1)];
            // Array.Copy(Elements, tempArray, Elements.Length);
            var element1Matches = CheckMatches(element1);
            var element2Matches = CheckMatches(element2);
        
            // TODO: 3?
        
            // bool matched = false;
            //
            // if (element1Matches.Count >= 3)
            // {
            //     foreach (var element in element1Matches)
            //     {
            //         Elements[element.Column, element.Row].Match();
            //         Elements[element.Column, element.Row] = null;
            //     }
            //
            //     matched = true;
            // }
            //
            // if (element2Matches.Count >= 3)
            // {
            //     foreach (var element in element2Matches)
            //     {
            //         // if (element == null) continue;
            //         if (Elements[element.Column, element.Row] == null) continue;
            //         Elements[element.Column, element.Row].Match();
            //         Elements[element.Column, element.Row] = null;
            //     }
            //
            //     matched = true;
            // }
        
            if (element1Matches.Count >= 3 || element2Matches.Count >= 3)
            {
                // DropElementsAll(Elements, false);
                CheckMatchesAll();
            } else
            {
                element1.Swap(element2, true);
            }
            
        }

        public bool IsGridFull()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Elements[x, y] == null)
                        return false;
                }
            }

            return true;
        }
        
        public int CountEmptyElements()
        {
            int counter = 0;
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Elements[x, y] == null)
                        counter++;
                }
            }

            return counter;
        }


        public IEnumerator FallAnimation()
        {
            // Coroutine[,] coroutines = new Coroutine[Columns, Rows];
            List<Coroutine> coroutines = new List<Coroutine>();
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    var element = Elements[x, y];
                    if (element == null)
                    {
                        // coroutines[x, y] = null;
                        continue;
                    }
                    
                    // bool last = (x == Columns - 1 && y == Rows - 1);
                    // var element = elements[x, y];
                    // if (element != null)
                    coroutines.Add(StartCoroutine(FallAnimationOne(element)));
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

            // yield return new WaitUntil(() => coroutines.All(c => c == null));

            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }

            Debug.Log("EndMove");
            
            // foreach (var coroutine in coroutines)
            // {
            //     if (coroutine != null) 
            //         Debug.Log("Coroutine is not null");
            // }

            // if (CountEmptyElements() <= 1)
            if (IsGridFull())
            {
                Debug.Log("Grid is full. Checking for matches.");
                CheckMatchesAll();
            }
            else
            {
                Debug.Log($"Grid is not full. There are {CountEmptyElements()} more empty elements. Spawning new elements.");
                SpawnNewElements();
            }
        }

        IEnumerator FallAnimationOne(Element element)
        {
                
            // var element = Elements[x, y];
            // if (element == null || elements[x,y] == null || !element.IsFalling) yield break;

            // if (element != null && Elements[x,y] != null)
            // {

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
            }
            
            // }

            // if (x >= Columns - 1 && y >= Rows - 1)
            // {
            //     if (IsGridFull())
            //         CheckMatchesAll();
            //     else
            //        SpawnNewElements(false);
            // }
            // yield return null;
        }
    }
}