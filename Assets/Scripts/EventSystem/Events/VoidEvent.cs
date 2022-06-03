using UnityEngine;

[CreateAssetMenu(fileName = "New Void Event", menuName = "Game Event/Void Event")]

public class VoidEvent : BaseGameEvent<Void>
{
    public void Raise() => Raise(new Void());
}
