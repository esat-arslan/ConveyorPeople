using UnityEngine;

[CreateAssetMenu(fileName = "Car_SO", menuName = "Scriptable Objects/Car_SO")]
public class Car_SO : ScriptableObject
{
    public CarType type;
    public Color color;
    public int size;
}
public enum CarType { Red, Blue, Green }
