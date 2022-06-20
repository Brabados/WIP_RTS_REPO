using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAI : MonoBehaviour
{

    [SerializeField] Transform Destination;
    [SerializeField] SmartObject Assigned;
    [SerializeField] GameObjectEvent NeedNewTask;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Assigned == null)
        {
            PickAction();
        }

        if(navMeshAgent.destination.x == this.transform.position.x && navMeshAgent.destination.z == this.transform.position.z)
        {
           
        }
    }

    public void PickAction()
    {
        NeedNewTask.Raise(this.gameObject);
    }

    public void AssignAction(SmartObject toAssign)
    {
        Assigned = toAssign;
        navMeshAgent.destination = Assigned.gameObject.transform.position;
    }
}
