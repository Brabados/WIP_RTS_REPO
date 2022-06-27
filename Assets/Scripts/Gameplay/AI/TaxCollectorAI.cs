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
        List<SmartObject> RanSO = new List<SmartObject>(); ;
        List<BaseInteraction> RanBI = new List<BaseInteraction>();

        if (Task == null)
        {
            if (CarriedMoney < 250)
            {
                foreach (SmartObject x in toAssign)
                {
                    foreach (BaseInteraction y in x.Interactions)
                    {
                        if (y is GetTaxes)
                        {
                            RanSO.Add(x);
                            RanBI.Add(y);
                        }
                    }
                }
            }
            if (RanBI.Count >= 1)
            {
                int HighestIndex = 0;
                for (int x = RanBI.Count -1; x >= 0; x--)
                {
                    if (RanBI[x].isAvalible())
                    {
                        if ((RanBI[HighestIndex] as GetTaxes).LinkedHouse.HomesMoney.currentMoney < (RanBI[x] as GetTaxes).LinkedHouse.HomesMoney.currentMoney)
                        {
                            HighestIndex = x;
                        }
                    }
                }

                Task = RanBI[HighestIndex];
                Task = RanBI[HighestIndex];
                RanBI[HighestIndex].Lock();
                isDoingTask = false;
                Destination = RanSO[HighestIndex].Entrance.transform;
                navMeshAgent.SetDestination(Destination.position);
            }
        }
    }
}
