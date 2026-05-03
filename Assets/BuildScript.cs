using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StateScript;

public class BuildScript : MonoBehaviour
{
    public GameObject muzzleFlash;
    Coroutine CrossbowCoroutine;
    Coroutine CannonCoroutine;
    [SerializeField]GameObject CannonBall;
    [SerializeField]LineRenderer lineRenderer;
    public StateScript.Team team;
    [SerializeField] float cannonCooldown = 1.5f;
    [SerializeField] float crossbowCooldown = 0.2f;
    public void Start()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);    
        lineRenderer.enabled = false;
    }
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
    void LayerUpdate()
    {
        detectionLayer = AllLayers;
        int myLayerBit = 1 << LayerAssignment(team);
        detectionLayer &= ~myLayerBit;
    }
    IEnumerator CrossbowCR()
    {
        Team previousTeam = team;   
        LayerUpdate();
        while (true)
        {
            if (team != previousTeam)
            {
                previousTeam = team;
                LayerUpdate();
            }
            while (team == StateScript.Team.None)
            {
                yield return new WaitForSeconds(0.5f);
            }
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
            if(hitColliders.Length == 0)
            {
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
            //
            //
            lineRenderer.SetPosition(1, nearestEnemy.position);
            lineRenderer.enabled = true;
            Destroy(nearestEnemy.gameObject);
            yield return new WaitForSeconds(0.02f);
            lineRenderer.SetPosition(1, transform.position);
            lineRenderer.enabled = false;
            yield return new WaitForSeconds(crossbowCooldown);
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
        Team previousTeam = team;
        LayerUpdate();
        while (true)
        {
            if (team != previousTeam)
            {
                previousTeam = team;
                LayerUpdate();
            }
            while (team == StateScript.Team.None)
            {
                yield return new WaitForSeconds(0.5f);
            }
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
            if (hitColliders.Length == 0)
            {
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
            this.transform.rotation = Quaternion.Euler(0, 0, angle);
            //
            //
            StartCoroutine(CannonShock());
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 forwardDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector2 leftDir = new Vector2(-Mathf.Sin(angleRad), Mathf.Cos(angleRad)); // Rotated 90 degrees left
            Vector2 muzzlePosition = (Vector2)transform.position + (forwardDir * 1.2f) + (leftDir * 0.05f);
            GameObject mFlash = Instantiate(muzzleFlash,muzzlePosition,Quaternion.Euler(0,0,angle));
            yield return hundredth;
            yield return hundredth;
            Destroy(mFlash);
            GameObject top = Instantiate(CannonBall, transform.position, Quaternion.Euler(0,0,angle));
            top.GetComponent<CannonBallScript>().detectionLayers = this.detectionLayer;
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(CannonBackUp());
            yield return new WaitForSeconds(cannonCooldown);
        }
    }
    WaitForSeconds hundredth = new WaitForSeconds(0.01f);
    IEnumerator CannonShock()
    {
        for (int i=0; i<5;i++)
        {
            transform.position += -transform.right*(1f/10f);
            yield return hundredth;
        }
    }
    IEnumerator CannonBackUp()
    {
        for(int i=0; i<50;i++)
        {
            transform.position += transform.right*(1f/100f);
            yield return hundredth;
        }
    }
    float detectionRadius = 4f;
    public LayerMask AllLayers;
    public LayerMask detectionLayer; // Set this in the Inspector to avoid hitting everything

    public Collider2D[] GetObjectsInCircle(Vector2 centerPoint)
    {
        // This returns an array of all colliders within the radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(centerPoint, detectionRadius, detectionLayer);

        return hitColliders;
    }

    /*
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    */
}
