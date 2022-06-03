using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateMaterials : MonoBehaviour
{
    public Material isPlaceable;
    public Material isNotPlaceable;
    public bool placed = false;
    private void OnTriggerEnter(Collider other)
    {
        if (placed == false)
        {


            //if it's not the terrain and it's not itself
            if (other.gameObject.name != "Terrain" && other.gameObject != gameObject)
            {
                if (gameObject.GetComponent<Renderer>())
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial = isNotPlaceable;
                    print(other.gameObject.name);
                }
                else
                {
                    if (gameObject.GetComponentInChildren<Renderer>())
                    {
                        gameObject.GetComponentInChildren<Renderer>().sharedMaterial = isNotPlaceable;
                        print(other.gameObject.name);
                    }
                }
            }
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (placed == false)
        {


            //if it's not the terrain and it's not itself
            if (other.gameObject.name != "Terrain" && other.gameObject != gameObject)
            {
                //making sure we don't change the wrong building. Only the building currently being placed will have a trigger.
                if (gameObject.GetComponent<Collider>().isTrigger == true)
                {
                    if (gameObject.GetComponentInChildren<Renderer>().sharedMaterial != isNotPlaceable)
                    {
                        if (gameObject.GetComponent<Renderer>())
                        {
                            gameObject.GetComponent<Renderer>().sharedMaterial = isNotPlaceable;
                            print(other.gameObject.name);
                        }
                        else
                        {
                            if (gameObject.GetComponentInChildren<Renderer>())
                            {
                                gameObject.GetComponentInChildren<Renderer>().sharedMaterial = isNotPlaceable;
                                print(other.gameObject.name);
                            }
                        }
                    }
                }
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (placed == false)
        {
            if (gameObject.GetComponent<Renderer>())
            {
                gameObject.GetComponent<Renderer>().sharedMaterial = isPlaceable;

            }
            else
            {
                if (gameObject.GetComponentInChildren<Renderer>())
                {
                    gameObject.GetComponentInChildren<Renderer>().sharedMaterial = isPlaceable;

                }
            }
        }
    }
}
