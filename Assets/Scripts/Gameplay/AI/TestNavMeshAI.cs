using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAI : MonoBehaviour
{
    public int ID;
    [SerializeField] Transform Destination;
    [SerializeField] SmartObject Assigned;
    [SerializeField] BaseInteraction Task;
    [SerializeField] IntEvent NeedNewTask;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        AIManager.Instance.Register(this);
    }

    private void Update()
    {
        if (Assigned == null)
        {
            PickAction();
        }

    }

    public void PickAction()
    {
        NeedNewTask.Raise(ID);
    }

    public void AssignAction(List<SmartObject> toAssign)
    {
        if (toAssign.Count > 0)
        {
            SmartObject RanSO = toAssign[Random.Range(0, toAssign.Count - 1)];
            Assigned = RanSO;

            BaseInteraction RanBI = RanSO.Interactions[Random.Range(0, RanSO.Interactions.Count - 1)];
            Task = RanBI;

            Destination = RanSO.gameObject.transform;
            navMeshAgent.SetDestination(Destination.position);
        }
    }
}
