using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAI : MonoBehaviour
{
    public int ID;
    [SerializeField] Transform Destination;
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

        if (Task == null)
        {
            PickAction();
        }
        else
        {
            if (!navMeshAgent.pathPending)
            {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        Task.Perform(this);
                    }
                }
            }
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

            BaseInteraction RanBI = RanSO.Interactions[Random.Range(0, RanSO.Interactions.Count - 1)];
            Task = RanBI;

            if (RanBI.CanPerform())
            {
                Task = RanBI;
                RanBI.Lock();
                Destination = RanSO.Entrance.transform;
                navMeshAgent.SetDestination(Destination.position);
            }
        }
    }

    public void TaskComplete()
    {
        Task.Unlock();
        Task = null;
    }
}
