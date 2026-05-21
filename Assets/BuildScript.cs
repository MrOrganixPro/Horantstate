using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using static StateScript;

public class BuildScript : MonoBehaviour
{
    [SerializeField] GameObject rangeOutline;
    public GameObject muzzleFlash;
    Coroutine CrossbowCoroutine;
    Coroutine CannonCoroutine;
    [SerializeField] GameObject CannonBall;
    [SerializeField] LineRenderer lineRenderer;
    private StateScript.Team _team;

    public StateScript.Team team
    {
        get => _team;
        set
        {
            _team = value;
        }
    }
    [SerializeField] float cannonCooldown = 1.5f;
    [SerializeField] float crossbowCooldown = 0.2f;
    [Header("Radiuses")]
    [SerializeField] float crossbowRadius = 3f;
    [SerializeField] float cannonRadius = 2f;
    [SerializeField] float boostRadius = 1.5f;
    [SerializeField] float bigFortRadius;
    public void Start()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.enabled = false;
    }
    [SerializeField] GameObject RotatorPrefab;
    GameObject rotator;
    ROTATE rotatorScript;
    public void BigFortControl()
    {
        StartCoroutine(BigFortControlCR());
    }
    WaitForSeconds two_tenth = new WaitForSeconds(0.2f);
    List<StateScript> previouslyControlledStates;
    IEnumerator BigFortControlCR()
    {
        previouslyControlledStates = new List<StateScript>();
        StateScript thisState = this.GetComponentInParent<StateScript>();
        FirstInstantiateShieldSprites(thisState);
        while(true)
        {
            yield return two_tenth;
            if(previouslyControlledStates.Count>0)
            {
                foreach(StateScript pcs in previouslyControlledStates)
                {
                    if(pcs.team != this.team)
                    {
                        pcs.shield.SetActive(false);
                        pcs.UnconnectToBigFort();
                    }
                }
                previouslyControlledStates.Clear();
            }
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, bigFortRadius, 1<<12);
            foreach(Collider2D hitCollider in hitColliders)
            {
                StateScript hitState=hitCollider.transform.parent.GetComponent<StateScript>();
                if(hitState == thisState)
                    continue;
                if (hitState.team==this.team)
                {
                    hitState.shield.SetActive(true);
                    previouslyControlledStates.Add(hitState);
                    hitState.ConnectToBigFort(thisState);            
                }           
            }
        }
    }
    void FirstInstantiateShieldSprites(StateScript thisState)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, bigFortRadius, 1<<12);
        foreach(Collider2D hitCollider in hitColliders)
        {
            StateScript hitState=hitCollider.transform.parent.GetComponent<StateScript>();
            if(hitState == thisState)
                continue;
            GameObject shield =Instantiate(GameManager.Instance.Shield,hitCollider.transform.position,Quaternion.Euler(0,0,0));         
            shield.transform.localScale = new Vector3(1.25f,1.25f,1);
            shield.transform.parent = rangeParentTransform;
            hitState.shield = shield;
            shield.SetActive(false);
        }
    }
    public void Booster()
    {
        MakeRangeOutline(boostRadius);
        rotator = Instantiate(RotatorPrefab,transform.position,Quaternion.identity);
        
        StateScript st = transform.parent.GetComponent<StateScript>();
        st.Visuals.renderers.Add(rotator.transform.GetChild(0).GetComponent<SpriteRenderer>());//first child is the rotating light
        st.Visuals.renderers.Add(rotator.transform.GetChild(1).GetComponent<SpriteRenderer>());//first child is the background light

        rotator.transform.SetParent(gameObject.transform);
        
        rotatorScript = rotator.GetComponent<ROTATE>(); 
        rotatorScript.team = this.team;
        StartCoroutine(BoosterCR());
    }
    WaitForSeconds tenth  = new WaitForSeconds(0.1f); 
    IEnumerator BoosterCR()
    {
        Team previousTeam = team;
        while (true)
        {
            if(team != previousTeam)
            {
                rotatorScript.team = this.team;
                previousTeam = team;
            }
            yield return tenth;
        }
    }
    public void Crossbow()
    {
        if (CrossbowCoroutine == null)
        {
            CrossbowCoroutine = StartCoroutine(CrossbowCR());
        }
        MakeRangeOutline(crossbowRadius);
    }
    
    void LayerUpdate()
    {
        detectionLayer = AllLayers;
        int myLayerBit = 1 << GameManager.Instance.LayerAssignment(team);
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
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, crossbowRadius, detectionLayer);
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
            this.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
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
        MakeRangeOutline(cannonRadius);
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
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, cannonRadius, detectionLayer);
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
            GameObject mFlash = Instantiate(muzzleFlash, muzzlePosition, Quaternion.Euler(0, 0, angle));
            yield return hundredth;
            yield return hundredth;
            Destroy(mFlash);
            GameObject top = Instantiate(CannonBall, transform.position, Quaternion.Euler(0, 0, angle));
            top.GetComponent<CannonBallScript>().detectionLayers = this.detectionLayer;
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(CannonBackUp());
            yield return new WaitForSeconds(cannonCooldown);
        }
    }
    WaitForSeconds hundredth = new WaitForSeconds(0.01f);
    IEnumerator CannonShock()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.position += -transform.right * (1f / 10f);
            yield return hundredth;
        }
    }
    IEnumerator CannonBackUp()
    {
        for (int i = 0; i < 50; i++)
        {
            transform.position += transform.right * (1f / 100f);
            yield return hundredth;
        }
    }
    public LayerMask AllLayers;
    public LayerMask detectionLayer; 
    void MakeRangeOutline(float givenRadius)
    {
        GameObject range = Instantiate(rangeOutline, transform.position, Quaternion.identity);
        range.transform.SetParent(rangeParentTransform);
        range.transform.localScale = new Vector3(givenRadius/3.14f, givenRadius/3.14f, 1);
    }
    [SerializeField] Transform rangeParentTransform;
}
