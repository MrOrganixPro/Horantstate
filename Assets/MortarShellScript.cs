using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MortarShellScript : MonoBehaviour
{
    public GameObject targetSign;
    [HideInInspector]public GameObject target;
    [SerializeField] float speed;
    [SerializeField] int shellDamage;
    public StateScript.Team team;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ShellShot());
    }
    WaitForSeconds s = new WaitForSeconds(0.01f);
    IEnumerator ShellShot()
    {
        yield return s;
        Vector2 targetPos = target.transform.GetChild(0).transform.position;//0 = center
        GameObject target_sign =Instantiate(targetSign,targetPos,Quaternion.Euler(0,0,0));
        target_sign.GetComponent<SpriteRenderer>().color = GameManager.Instance.GetTeamColorSharp(team);
        for(int i=0; i<30; i++)
        {
            //speed 0.4 
            transform.position += transform.right*speed;
            yield return s;
        }
        yield return new WaitForSeconds(2.5f);
        transform.position = new Vector2(targetPos.x,targetPos.y+18);

        for(int i=0; i<45; i++)
        {
            if(i==40)
                Destroy(target_sign);
            transform.position += transform.right*-speed;
            yield return s;
        }
        StateScript targetScript = target.GetComponent<StateScript>();
        if(targetScript.team!=team)
            targetScript.DealDamageToState(shellDamage,team);
        Destroy(gameObject);
    }
}
