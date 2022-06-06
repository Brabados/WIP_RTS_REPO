using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteraction : BaseInteraction
{

    [SerializeField]
    protected int MaxUsers = 1;

    protected int CurretUsers = 0;

    public override bool isAvalible()
    {
        return CurretUsers < MaxUsers;
    }

    public override void Lock()
    {
        CurretUsers++;
        if(CurretUsers > MaxUsers)
        {
            Debug.LogError("Too many users have locked " + Name);
        }
    }

    public override void Perform(MonoBehaviour Performer)
    {
        if (CurretUsers <= 0)
        {
            Debug.LogError("Trying to perform with no users " + Name);
        }

    }

    public override void Unlock()
    {
       if (CurretUsers <= 0)
        {
            Debug.LogError("Attempted to unlock already unlocked Interaction " + Name);
        }
        CurretUsers--;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
