using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaxCollectorAI : TestNavMeshAI
{

    public int CarriedMoney;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void AssignAction(List<SmartObject> toAssign)
    {
        SmartObject RanSO;
        BaseInteraction RanBI;
        if (CarriedMoney < 250)
        {
            foreach (SmartObject x in toAssign)
            {
                foreach (BaseInteraction y in x.Interactions)
                {
                    if (y is GetTaxes)
                    {
                        if (Task == null)
                        {
                            if (y.isAvalible())
                            {
                                RanSO = x;
                                RanBI = y;
                                Task = RanBI;
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
        }
    }
}
