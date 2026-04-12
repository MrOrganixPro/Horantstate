using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LineTip : MonoBehaviour
{
    public LineRenderer line;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(line.GetPosition(1), line.GetPosition(0));
        if (distance < 0.6f)
        {
            GetComponent<Renderer>().enabled = false;
            transform.position = new Vector2(99, 0);
            return;
        }
        Vector2 pos = (Vector2)line.GetPosition(1);
        transform.position = pos + (Vector2)transform.up * -0.2f;
        Vector2 direction = line.GetPosition(0) - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle+92);
        GetComponent<Renderer>().enabled = true;    
    }
}
