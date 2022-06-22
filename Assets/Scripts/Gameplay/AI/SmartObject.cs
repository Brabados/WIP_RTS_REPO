using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject : MonoBehaviour
{

    [SerializeField] 
    protected string _Name;

    public string Name => _Name;

    public int ID;

    [SerializeField]
    public GameObject Entrance;

    protected List<BaseInteraction> CashedInterations = null;

    [SerializeField]
    protected IntEvent _Completetion;
    public List<BaseInteraction> Interactions
    {
        get
        {
            if(CashedInterations == null)
            {
                CashedInterations = new List<BaseInteraction>(GetComponents<BaseInteraction>());
            }

            return CashedInterations;
        }
    }

    public void Start()
    {

    }

    public void OnPlacement()
    {
        SmartObjectManager.Instance.Register(this);
    }

    public void OnDestroy()
    {
        SmartObjectManager.Instance.Deregister(this);
    }
}
