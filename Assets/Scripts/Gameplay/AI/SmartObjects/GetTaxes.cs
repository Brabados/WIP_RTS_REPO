using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SmartObjectHouse))]
public class GetTaxes : SimpleInteraction
{

    public SmartObjectHouse LinkedHouse;

    public override void Perform(MonoBehaviour Performer, UnityAction<BaseInteraction> onCompleted)
    {
        base.Perform(Performer, onCompleted);
        Performer.gameObject.GetComponent<TaxCollectorAI>().CarriedMoney += LinkedHouse.HomesMoney.currentMoney;
        LinkedHouse.HomesMoney.currentMoney = 0;
    }

    // Start is called before the first frame update

    public override bool isAvalible()
    {
        if(base.isAvalible() && LinkedHouse.HomesMoney.currentMoney >= 10)
        {
            return true;
        }
        return false;
    }
    void Start()
    {
        
    }
}
