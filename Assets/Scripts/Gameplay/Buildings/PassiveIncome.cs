using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveIncome : MonoBehaviour
{

    public int currentMoney;
    public int passiveIncome = 5;
    public float TimeTillIncome = 2f;
    public float TimeNextIncome = -1;

    private void Update()
    {
        if(TimeNextIncome <= 0)
        {
            currentMoney += passiveIncome;
            TimeNextIncome = TimeTillIncome;
        }
        else
        {
            TimeNextIncome -= Time.deltaTime;
        }
    }

}
