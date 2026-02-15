using UnityEngine;

public class CarPersonSlot : MonoBehaviour
{
    public Person Occupant {get;private set;}
    public int SlotIndex {get;private set;}

    public Vector3 Position => transform.position;
    private bool isReserved;
    public bool IsReserved => isReserved;
    public bool IsOccupied => Occupant != null;

    public bool IsAvailable => !IsOccupied && !IsReserved;

    public void AssignToSlot(Person person)
    {
        Occupant = person;
        isReserved = false;

        person.transform.SetParent(transform);
        person.transform.localPosition = Vector3.zero;
    }

    public void Reserve()
    {
        isReserved = true;
    }

    public void Clear()
    {
        Occupant = null;
    }
}
