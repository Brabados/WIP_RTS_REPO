using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInteractionType
{
    Instantanious = 0,
    OverTime = 1
}

public abstract class BaseInteraction : MonoBehaviour
{
    [SerializeField] 
    protected string _Name;
    [SerializeField]
    protected EInteractionType _InteractionType = EInteractionType.Instantanious;
    [SerializeField]
    protected float _Duration = 0f;
    [SerializeField]
    public IntEvent Completetion;

    public string Name => _Name;
    public EInteractionType InteractionType => _InteractionType;
    public float Duration => _Duration;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract bool isAvalible();

    public abstract void Lock();

    public abstract void Unlock();

    public abstract void Perform(MonoBehaviour Performer);
}
