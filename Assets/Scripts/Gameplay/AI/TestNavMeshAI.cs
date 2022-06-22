using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAI : MonoBehaviour
{
    public int ID;
    [SerializeField] Transform Destination;
    [SerializeField] BaseInteraction Task;
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        AIManager.Instance.Register(this);
    }

    private void Update()
    {

        if (Task == null)
        {
            AssignAction(SmartObjectManager.Instance.RegisteredObjects);
        }
        else
        {
            float distance;
            distance = Vector3.Distance(this.gameObject.transform.position, Destination.position);
            if (distance < 2)
            {
                Task.Perform(this, OnTaskFinish);
            }
                   
        }


    }

    public void OnTaskFinish(BaseInteraction Interation)
    {
        Task.Unlock();
        Task = null;
    }


    public void AssignAction(List<SmartObject> toAssign)
    {
        if (Task == null)
        {
            if (toAssign.Count > 0)
            {
                SmartObject RanSO = toAssign[Random.Range(0, toAssign.Count)];

                BaseInteraction RanBI = RanSO.Interactions[Random.Range(0, RanSO.Interactions.Count)];
                Task = RanBI;

                if (RanBI.isAvalible())
                {
                    Task = RanBI;
                    RanBI.Lock();
                    Destination = RanSO.Entrance.transform;
                    navMeshAgent.SetDestination(Destination.position);
                }
            }
        }
    }
}
