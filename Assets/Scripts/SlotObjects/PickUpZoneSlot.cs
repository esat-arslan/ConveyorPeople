using UnityEngine;

public class PickUpZoneSlot : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public Car Occupant {get; private set;}
    public bool IsOccupied => Occupant != null;

    public void AssignToCar(Car car)
    {
        Occupant = car;
    }

    public void Clear()
    {
        Occupant = null;
    }
    
}
