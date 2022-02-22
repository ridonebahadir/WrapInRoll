using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class BombArea : MonoBehaviour
{
    [Range(0, 50)]
    public int segments = 50;
    //[Range(0, 5)]
    //public float xradius = 5;
    //[Range(0, 5)]
    //public float yradius = 5;
    LineRenderer line;


    //EXPLOSION
    public static float radiusStatic;
    public float radius;
    public float power;
    public LayerMask bombLayer;
    public List<GameObject> bombItem;
    public GameObject BombParticle;
    public static int bombItemCount;
    public Renderer bombRenderer;

    [System.Obsolete]
    void Start()
    {
        bombRenderer = GetComponent<Renderer>();
        line = gameObject.GetComponent<LineRenderer>();

        line.SetVertexCount(segments + 1);
        line.useWorldSpace = false;
        //CreatePoints();
    }
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag=="Player")
    //    {
    //        Debug.Log("PATLADI");
    //        Detonate();
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            bombRenderer.enabled = false;
            Debug.Log("PATLADI");
            Detonate();
        }
    }
    //void CreatePoints()
    //{
    //    float x;
    //    float y;
    //    //float z;

    //    float angle = 20f;

    //    for (int i = 0; i < (segments + 1); i++)
    //    {
    //        x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius/2;
    //        y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius/2;

    //        line.SetPosition(i, new Vector3(0, y, x));

    //        angle += (360f / segments);
    //    }
    //}
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    // //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
    // Gizmos.DrawWireSphere(transform.position , HumanRoll.radiusStatic);
    //}
    void Detonate()
    {

        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, bombLayer);
        foreach (Collider hit in colliders)
        {
            //Rigidbody rb = hit.GetComponent<Rigidbody>();
            bombItem.Add(hit.gameObject);
            //if (rb != null)
            //rb.AddExplosionForce(power, explosionPos, radius,1.0F,ForceMode.Impulse);

        }
        StartCoroutine(BombItem());
        bombItemCount = bombItem.Count+1;
        Debug.Log("bombItemCount = " + bombItemCount);

    }

    IEnumerator BombItem()
    {
        for (int i = 0; i < bombItem.Count; i++)
        {
            Instantiate(BombParticle, bombItem[i].transform.position, Quaternion.identity);
            Destroy(bombItem[i].gameObject);
            yield return new WaitForSeconds(0.3f);
        }
       
        gameObject.SetActive(false);

    }
}