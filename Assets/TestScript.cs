using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Force match");
            New.Grid.Instance.CheckMatchesAll(New.Grid.Instance.Elements);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Force fall");
            New.Grid.Instance.DropElementsAll(New.Grid.Instance.Elements, false);
        }
    }
}
