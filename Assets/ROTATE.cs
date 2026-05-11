using UnityEngine;

public class ROTATE : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    public StateScript.Team team;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void FixedUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed);
    }
}
