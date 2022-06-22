using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteraction : BaseInteraction
{

    protected class PerformerInfo
    {
        public int ID;
        public float ElapsedTime;
    }

    [SerializeField]
    protected int MaxUsers = 1;



    protected List<PerformerInfo> Performers = new List<PerformerInfo>();

    protected int CurretUsers = 0;

    public override bool isAvalible()
    {
        return CurretUsers < MaxUsers;
    }

    public override void Lock()
    {
        CurretUsers++;
        //Error Catching for debug
        if (CurretUsers > MaxUsers)
        {
            Debug.LogError("Too many users have locked " + Name);
        }
    }

    public override void Perform(MonoBehaviour Performer)
    {
        //Error Catching for debug
        if (CurretUsers <= 0)
        {
            Debug.LogError("Trying to perform with no users " + Name);
        }


        if(InteractionType == EInteractionType.Instantanious)
        {
            Completetion.Raise(Performer.gameObject.GetComponent<TestNavMeshAI>().ID);
        }
        else if(InteractionType == EInteractionType.OverTime)
        {
            PerformerInfo toAdd = new PerformerInfo();
            toAdd.ElapsedTime = 0;
            toAdd.ID = Performer.gameObject.GetComponent<TestNavMeshAI>().ID;
            Performers.Add(toAdd);
        }

    }

    public override void Unlock()
    {
        //Error Catching for debug
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
    protected virtual void Update()
    {
        for(int index = Performers.Count - 1; index >= 0; index--)
        {
            Performers[index].ElapsedTime += Time.deltaTime;
            if(Performers[index].ElapsedTime >= Duration)
            {
                Completetion.Raise(Performers[index].ID);
                Performers.RemoveAt(index);
            }
        }
    }

    public override bool CanPerform()
    {
        if(CurretUsers < MaxUsers)
        {
            return true;
        }
        return false;
    }
}
