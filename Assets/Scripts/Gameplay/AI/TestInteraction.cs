using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestInteraction : BaseInteraction
{

    protected class PerformerInfo
    {
        public int ID;
        public float ElapsedTime;
        public UnityAction<BaseInteraction> onCompletion;
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

    public override void Perform(MonoBehaviour Performer, UnityAction<BaseInteraction> onCompleted)
    {
        //Error Catching for debug
        if (CurretUsers <= 0)
        {
            Debug.LogError("Trying to perform with no users " + Name);
        }


        if(InteractionType == EInteractionType.Instantanious)
        {
            onCompleted.Invoke(this);
        }
        else if(InteractionType == EInteractionType.OverTime)
        {
            bool check = false;
            foreach(PerformerInfo x in Performers)
            {
                if(Performer.gameObject.GetComponent<TestNavMeshAI>().ID == x.ID)
                {
                    check = true;
                }
            }
            if (check == false)
            {
                PerformerInfo toAdd = new PerformerInfo();
                toAdd.ElapsedTime = 0;
                toAdd.ID = Performer.gameObject.GetComponent<TestNavMeshAI>().ID;
                toAdd.onCompletion = onCompleted;
                Performers.Add(toAdd);
            }
        }

    }

    public override void Unlock()
    {
        //Error Catching for debug
        if (CurretUsers < 0)
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
                Performers[index].onCompletion.Invoke(this);
                Performers.RemoveAt(index);
            }
        }
    }
}
