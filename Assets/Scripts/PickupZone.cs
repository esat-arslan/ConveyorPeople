using System.Collections.Generic;
using UnityEngine;

public class PickupZone : MonoBehaviour
{
    [SerializeField] private PickUpZoneSlot slotPrefab;
    [SerializeField] private List<Transform> slotPositions;

    private readonly List<PickUpZoneSlot> slots = new();
    public IReadOnlyList<PickUpZoneSlot> Slots => slots;

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        foreach (Transform pos in slotPositions)
        {
            PickUpZoneSlot slot = Instantiate(slotPrefab, pos.position, Quaternion.identity, transform);
            slots.Add(slot);
        }
    }

    public PickUpZoneSlot GetFreeSlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied) return slot;
        }
        return null;
    }
}
