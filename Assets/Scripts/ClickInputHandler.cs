using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInputHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Debug.Log("Click Detected");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (!hit)
        {
            Debug.Log("Hit nOOO");
            return;
        }

        Debug.Log($"Raychast hit {hit.collider.name}");
        Car car = hit.collider.GetComponent<Car>();
        if (car == null)
        {
            Debug.Log("hit object is not a car");
            return;
        }
        Debug.Log("Car clicked!");
        car.SetActive(true);
        Debug.Log($"Clicked on the car {car.gameObject.GetInstanceID()}");
    }
}
