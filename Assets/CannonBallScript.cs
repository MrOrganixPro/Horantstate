using System.Collections;
using UnityEngine;

public class CannonBallScript : MonoBehaviour
{
    public LayerMask detectionLayers;
    public float speed;
    void Start()
    {
        StartCoroutine(kys());
    }
    private void Update()
    {
    }
    void FixedUpdate()
    {
        transform.position += transform.right * speed;
            Collider2D[] hitCollider = Physics2D.OverlapCircleAll(transform.position, 0.5f, detectionLayers);
            if (hitCollider != null)
            {
                foreach(Collider2D c in hitCollider)
                {
                    Destroy(c.gameObject);
                }
            }
    }
    IEnumerator kys()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
/*
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
*/
}
