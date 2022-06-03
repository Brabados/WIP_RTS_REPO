using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseButtonSript : MonoBehaviour
{
    public void Toggle()
    {
        if (gameObject.activeInHierarchy == false)
        {         
            gameObject.SetActive(true);
        }
        else
        {        
            gameObject.SetActive(false);
        }

    }
}
