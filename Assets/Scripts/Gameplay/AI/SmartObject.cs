using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject : MonoBehaviour
{

    [SerializeField] protected string _Name;

    public string Name => _Name;

    public int ID;

    protected List<BaseInteraction> CashedInterations = null;

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
        SmartObjectManager.Instance.Register(this);
    }

    public void OnDestroy()
    {
        SmartObjectManager.Instance.Deregister(this);
    }
}