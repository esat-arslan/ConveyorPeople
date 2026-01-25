using UnityEngine;

public class CarPersonSlot : MonoBehaviour
{
    public Person Occupant {get;private set;}
    public int SlotIndex {get;private set;}

    public Vector3 Position => transform.position;
    public bool IsOccupied => Occupant != null;

    public void AssignToSlot(Person person)
    {
        Occupant = person;
    }

    public void Clear()
    {
        Occupant = null;
    }
}
