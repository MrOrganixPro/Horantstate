using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Scripting.APIUpdating;

public class TroopScript : MonoBehaviour
{
    Vector2 targetPos;
    bool canMove;
    public int troopDamage =1;
    [SerializeField]float speed;
    public GameObject sender;
    public GameObject target;
    [SerializeField]SpriteRenderer spriteRenderer;
    public StateScript.Team troopTeam;
     
    [SerializeField] private float colorDarkeningMultiplier;
    void Start()
    {
        
    }
    public void ArmyOrder(int index, int totalSoldiers, float spacing,bool stayForward)
    {
        targetPos = target.transform.GetChild(0).position;
        targetPos = new Vector3(targetPos.x, targetPos.y);
        Vector2 directionToTarget = (targetPos - (Vector2)transform.position).normalized;

        // 2. Rotate to face target
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 3. Calculate the "Side" vector (Perpendicular to the direction)
        // In 2D, flipping (x, y) to (-y, x) gives a 90-degree rotation
        Vector2 sideVector = new Vector2(-directionToTarget.y, directionToTarget.x);

        // 4. Calculate centered offset
        // This ensures Soldier 0 is on the left, and the middle soldier is at the center
        float offsetAmount = (index - (totalSoldiers - 1) / 2f) * spacing;

        targetPos = targetPos + (sideVector * offsetAmount);

        // 5. Apply the position
        // Move them toward the target AND shift them along the sideVector
        Vector3 finalMovement =(sideVector * offsetAmount);

        // We use a simple assignment or addition depending on if this is a "Teleport" or "Step"
        transform.position += finalMovement;
        if (stayForward)
        {
            transform.position += transform.right * 0.07f;
        }
    }
    
    public void StartCollisionCheck()
    {
        StartCoroutine(CollisionCheck());
    }
    IEnumerator CollisionCheck()
    {
        //6blue 7red 8green 9yellow
        int allMasks = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9);
        int myLayerBit = 1 << gameObject.layer;
        allMasks &= ~myLayerBit;
        WaitForSeconds s = new WaitForSeconds(0.03f);
        while(gameObject)
        {
            Collider2D hitCollider = Physics2D.OverlapCircle(transform.position, 0.2f, allMasks);
            if(hitCollider != null)
            {
                Destroy(hitCollider.gameObject);
                Destroy(gameObject);
            }
            yield return s;
        }
    }
    public void March()
    {
        if(targetPos == default)
            targetPos = target.transform.GetChild(0).position;
        spriteRenderer.color = ColorUtils.Darken(GameManager.Instance.GetTeamColor(troopTeam),colorDarkeningMultiplier);
        canMove = true;
        StartCoroutine(MovingCR());
    }
    IEnumerator MovingCR()
    {
        while ((Vector2)transform.position != targetPos)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        StateScript targetScript = target.GetComponent<StateScript>();
        if (targetScript.team == troopTeam)
        {
            targetScript.troopCount++;
        }
        else
        {
            targetScript.DealDamageToState(troopDamage,troopTeam);
        }
        Destroy(gameObject);
    }
}
