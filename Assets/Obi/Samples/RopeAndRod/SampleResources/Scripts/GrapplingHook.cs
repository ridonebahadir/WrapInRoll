using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using Cinemachine;

/**
 * Sample component that shows how to use Obi Rope to create a grappling hook for a 2.5D game.
 * 95% of the code is the grappling hook logic (user input, scene raycasting, launching, attaching the hook, etc) and parameter setup,
 * to show how to use Obi completely at runtime. This might not be practical for real-world scenarios,
 * but illustrates how to do it.
 *
 * Note that the choice of using actual rope simulation for grapple dynamics is debatable. Usually
 * a simple spring works better both in terms of performance and controllability. 
 *
 * If complex interaction is required with the scene, a purely geometry-based approach (ala Worms ninja rope) can
 * be the right choice under certain circumstances.
 */
public class GrapplingHook : MonoBehaviour
{
    public LayerMask layer;
    public CinemachineTargetGroup cinemachineTargetGroup;
    public CinemachineBrain cinemachineBrain;
    public ObiSolver solver;
    public GameObject Player;
    public ObiCollider character;
    public float hookExtendRetractSpeed = 2;
    public Material material;
    public ObiRopeSection section;
    public GameObject tap;

    private ObiRope rope;
    private ObiRopeBlueprint blueprint;
    private ObiRopeExtrudedRenderer ropeRenderer;

    private ObiRopeCursor cursor;

    private RaycastHit hookAttachment;
    //public  Transform targetParent;
    public  Transform target;
    public  Transform dolanma;
    public int level;
    public int count;
    public float time;
    public bool otomatikHook;



    public PlayerRoll playerRoll;
    public Animator HumanAnim;
    public TrailRenderer trail;
    public Rigidbody humanRb;

    public GrapplingHook grapplingHook;
    
    //public GameObject player;
    void Awake()
    {
        level = PlayerPrefs.GetInt("Level",1);
        target = dolanma.GetChild(level - 1).GetChild(1).gameObject.transform;
        // Create both the rope and the solver:	
        rope = gameObject.AddComponent<ObiRope>();
        ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = section;
        ropeRenderer.uvScale = new Vector2(1, 5);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        rope.GetComponent<MeshRenderer>().material = material;

        // Setup a blueprint for the rope:
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;

        // Tweak rope parameters:
        rope.maxBending = 0f;
        rope.maxCompression = 1f;
        rope.bendCompliance = 1;

        // Add a cursor to be able to change rope length:
        cursor = rope.gameObject.AddComponent<ObiRopeCursor>();
        cursor.cursorMu = 0;
        cursor.direction = true;
        if (otomatikHook)
        {
            Invoke("Halat", time);
        }
       

    }
    void Halat()
    {

        
        LaunchHook(target.transform);
        //touch = false;
        //HumanAnim.enabled = false;
        //HumanAnim.SetBool("Finish", true);
        HumanAnim.enabled=false;
        playerRoll.enabled = false;
        trail.enabled = true;
        humanRb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        

    }
    private void OnDestroy()
    {
        DestroyImmediate(blueprint);
    }

    /**
	 * Raycast against the scene to see if we can attach the hook to something.
	 */
    Vector3 mouseInScene;
    public void LaunchHook(Transform targetObj)
    {

        // Get the mouse position in the scene, in the same XY plane as this object:
        Vector3 mouse = Input.mousePosition;
        mouse.z = targetObj.transform.position.z - Camera.main.transform.position.z;

        mouseInScene = Camera.main.ScreenToWorldPoint(mouse);
      
         //Get a ray from the character to the mouse:
        Ray ray = new Ray(transform.position, targetObj.transform.position - transform.position);

        // Raycast to see what we hit:
        if (Physics.Raycast(ray, out hookAttachment,1000,layer, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("HOOK = "+hookAttachment.collider.gameObject.name);
            // We actually hit something, so attach the hook!
            StartCoroutine(AttachHook());
        }

    }

    private IEnumerator AttachHook()
    {
        yield return 0;
        Vector3 localHit = rope.transform.InverseTransformPoint(hookAttachment.point);
        
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything,0);
        
        // Procedurally generate the rope path (a simple straight line):
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        // Generate the particle representation of the rope (wait until it has finished):
        yield return blueprint.Generate();
        
        // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
        rope.ropeBlueprint = blueprint;
        rope.GetComponent<MeshRenderer>().enabled = true;

        // Pin both ends of the rope (this enables two-way interaction between character and rope):
        var pinConstraints = rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(rope.solverIndices[0], character, transform.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.AddConstraint(rope.solverIndices[blueprint.activeParticleCount - 1], hookAttachment.collider.GetComponent<ObiColliderBase>(),
                                                          hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);

        rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }

    public void DetachHook()
    {
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        rope.ropeBlueprint = null;
        rope.GetComponent<MeshRenderer>().enabled = false;
    }

    //public Animator anim;
    
   
    void Update()
    {
        //if (!touch)
        //{


        //    //anim.enabled = true;
        //    //player.SetActive(false);
           
            
        //    touch = true;
            
        //    return;
        //    //    DetachHook();

        //}

        if (Input.GetMouseButtonUp(0))
        {
            //target = targetParent.GetChild(count);
            //DetachHook();
            Invoke("Late", 1f);
            tap.SetActive(false);
            Halat();
            //count++;
            //if (count==1)
            //{
                
            //}
            //else
            //{
            //    Invoke("Halat", 0.5f);
            //    cinemachineTargetGroup.m_Targets[0].weight = 1;
            //    cinemachineTargetGroup.m_Targets[count].weight = 1;
            //    cinemachineTargetGroup.m_Targets[count-1].weight = 0;
            //}
            grapplingHook.enabled = false;


        }
        //if (rope.isLoaded)
        //{

        //    //cursor.ChangeLength(rope.restLength - hookExtendRetractSpeed * Time.deltaTime);
        //    if (rope.restLength > 10)
        //    {
        //        Debug.Log("Rope =" + rope.restLength);
        //        //Player.transform.position = Vector3.Lerp(Player.transform.position, target, 0.1f);
        //    }



        //    //Debug.Log("Rope = " + rope.restLength);
        //    if (Input.GetKey(KeyCode.W))
        //    {

        //    }
        //    //if (Input.GetKey(KeyCode.S))
        //    //{
        //    //    cursor.ChangeLength(rope.restLength + hookExtendRetractSpeed * Time.deltaTime);
        //    //}
        //}
    }
    void Late()
    {
        cinemachineBrain.enabled = true;
        target.transform.GetChild(1).gameObject.SetActive(false);
    }
}
