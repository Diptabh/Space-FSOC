using UnityEngine;
using UnityEngine.InputSystem;

public class ch : MonoBehaviour
{
    public Transform objectA;
    public Transform objectB;

    void Update() {
        float distance = Vector3.Distance(objectA.position, objectB.position);
        Debug.Log("Distance: " + distance);
    }
}