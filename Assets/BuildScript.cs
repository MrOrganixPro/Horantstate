using System.Collections;
using UnityEngine;
using static StateScript;

public class BuildScript : MonoBehaviour
{
    Coroutine CrossbowCoroutine;
    Coroutine CannonCoroutine;
    GameObject Bolt;
    public StateScript.Team team;
    public void Crossbow()
    {
        if (CrossbowCoroutine == null)
        {
            CrossbowCoroutine = StartCoroutine(CrossbowCR());
        }
    }
    int LayerAssignment(Team t)
    {
        switch (t)
        {
            case StateScript.Team.Blue: return 6;
            case StateScript.Team.Red: return 7;
            case StateScript.Team.Green: return 8;
            case StateScript.Team.Yellow: return 9;
            default: return -1;
        }
    }
    IEnumerator CrossbowCR()
    {
        int myLayerBit = 1 << LayerAssignment(team);
        detectionLayer&= ~myLayerBit;
        while (true)
        {
            Debug.Log("CROSSBOWCRLOOP");
            while (team == StateScript.Team.None)
            {
                yield return new WaitForSeconds(0.5f);
            }
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
            if(hitColliders.Length == 0)
            {
                Debug.Log("NO ENEMIES IN RANGE");
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            float shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            // 2. Loop through the array
            foreach (Collider2D enemy in hitColliders)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

                // 3. Check if this enemy is closer than the last one we checked
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy.transform;
                }
            }
            Vector2 direction = nearestEnemy.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            this.transform.rotation = Quaternion.Euler(0, 0, angle-90);
            Debug.Log("BOLT");
            //Instantiate(Bolt, transform.position, Quaternion.Euler(0, 0, angle));
            yield return new WaitForSeconds(0.2f);
        }
    }
    public void Cannon()
    {
        if (CannonCoroutine == null)
        {
            CannonCoroutine = StartCoroutine(CannonCR());
        }
    }
    IEnumerator CannonCR()
    {
        yield return new WaitForSeconds(0.5f);
        Cannon();
    }
    float detectionRadius = 4f;
    public LayerMask detectionLayer; // Set this in the Inspector to avoid hitting everything

    public Collider2D[] GetObjectsInCircle(Vector2 centerPoint)
    {
        // This returns an array of all colliders within the radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(centerPoint, detectionRadius, detectionLayer);

        return hitColliders;
    }

    // Optional: Visualize the circle in the Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
