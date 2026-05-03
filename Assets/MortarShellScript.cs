using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MortarShellScript : MonoBehaviour
{
    public GameObject muzzleFlash;
    public GameObject mortar;
    public GameObject targetSign;
    [HideInInspector]public GameObject target;
    [SerializeField] float speed;
    [SerializeField] int shellDamage;
    public StateScript.Team team;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    WaitForSeconds s = new WaitForSeconds(0.01f);
    public IEnumerator ShellShot()
    {
        yield return s;
        GameObject m = Instantiate(muzzleFlash,new Vector2(transform.position.x,transform.position.y+0.8f),Quaternion.Euler(0,0,90));
        yield return s;
        yield return s;
        Destroy(m);
        Vector2 targetPos = target.transform.GetChild(0).transform.position;//0 = center
        GameObject target_sign =Instantiate(targetSign,targetPos,Quaternion.Euler(0,0,0));
        target_sign.GetComponent<SpriteRenderer>().color = GameManager.Instance.GetTeamColorSharp(team);
        StartCoroutine(MortarShock());
        for(int i=0; i<20; i++)
        {
            //speed 0.6 
            transform.position += transform.right*speed;
            yield return s;
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MortarBackUp());
        yield return new WaitForSeconds(2f);
        transform.position = new Vector2(targetPos.x,targetPos.y+27);

        for(int i=0; i<45; i++)
        {
            if(i==40)
                Destroy(target_sign);
            transform.position += transform.right*-speed;
            yield return s;
        }
        StateScript targetScript = target.GetComponent<StateScript>();
        if(targetScript.team!=team)
        {
            if(targetScript.troopCount<=shellDamage)
                targetScript.DealDamageToState(targetScript.troopCount+1,team);
            else
                targetScript.DealDamageToState(shellDamage,team);
        }
        
        
        Destroy(gameObject);
    }
    WaitForSeconds hundredth = new WaitForSeconds(0.01f);
    IEnumerator MortarShock()
    {
        for (int i=0; i<5;i++)
        {
            mortar.transform.position += -transform.right*(1f/25f);
            yield return hundredth;
        }
    }
    IEnumerator MortarBackUp()
    {
        for(int i=0; i<60;i++)
        {
            mortar.transform.position += transform.right*(1f/300f);
            yield return hundredth;
        }
    }
}
