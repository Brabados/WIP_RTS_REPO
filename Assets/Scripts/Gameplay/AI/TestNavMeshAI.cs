using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAI : BaseAI
{

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        AIManager.Instance.Register(this);
    }

    private void Update()
    {

        if (Task == null && TimeUntillnewTask <= 0)
        {
            AssignAction(SmartObjectManager.Instance.RegisteredObjects);
        }
        else if (Task != null && Destination != null)
        {
            float distance;
            distance = Vector3.Distance(this.gameObject.transform.position, Destination.position);
            if (distance < 2 && !isDoingTask)
            {
                isDoingTask = true;
                Task.Perform(this, OnTaskFinish);
            }
                   
        }
        else 
        {
            if(Task != null)
            {
                foreach(SmartObject x in SmartObjectManager.Instance.RegisteredObjects)
                {
                    foreach(BaseInteraction z in x.Interactions)
                    {
                        if(Task == z)
                        {
                            Destination = x.Entrance.transform;
                            navMeshAgent.SetDestination(Destination.position);
                        }
                    }
                }
            }
            TimeUntillnewTask -= Time.deltaTime;
        
        }


    }

    public override void OnTaskFinish(BaseInteraction Interation)
    {
        Task.Unlock();
        Task = null;
        TimeUntillnewTask = TimeBetweenTasks;
    }


    public override void AssignAction(List<SmartObject> toAssign)
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
                    isDoingTask = false;
                    Destination = RanSO.Entrance.transform;
                    navMeshAgent.SetDestination(Destination.position);
                }
            }
        }
    }
}
