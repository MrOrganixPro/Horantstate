using Unity.VisualScripting;
using UnityEngine;

public class BoltScript : MonoBehaviour
{
    SpriteRenderer sr;
    [SerializeField] float speed;
    // Update is called once per frame
    void FixedUpdate()
    {
       transform.position += transform.forward*speed* Time.fixedDeltaTime; 
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Troop"))
        {
            sr.enabled = false;
            gameObject.SetActive(false);
        }
    }
}
