using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OwnerBehaviour : MonoBehaviour
{
    void Update()
    {
        // Debug drawline destination
        Debug.DrawLine(transform.position, nav.destination, Color.yellow);


        // Check to see if player is within reach, and if so swing, and then after
        // he finished swinging, check again if in reach to see if we should
        // damage the player
        CheckToSwingAtPlayer();


        // Input for debugging different destinations for owner to navgivate to
        if (Input.GetKeyDown(KeyCode.M))
            SetRandomDestination();

        // Check if reached destination
        CheckReachedDesination();

        // Checks if player is in seight, including a 180 degree of eyesight
        // range, then changes appropriate booleans
        CheckCanSeePlayer();


        // Checks if currently in sight or currently in agro, and then determines
        // the animation and run speed
        CheckIfChasePlayer();


    }
    void CheckToSwingAtPlayer()
    {
        // Check if player is really close
        // and the swing will reach farther
        if (PlayerIsWithinReach(0.5f) == true)
            // Swing
            if (swinging == false)
                StartCoroutine(Swing());
    }
    bool swinging = false;
    IEnumerator Swing()
    {
        swinging = true;

        // Freeze owner
        Freeze(0.5f);
        
        // Animate owner
        animator.Play("attack");

        // Swing time
        yield return new WaitForSeconds(0.5f);
        playerHealth.ChangeHealth(-20f);

        // Recover time
        yield return new WaitForSeconds(0.5f);

        swinging = false;
    }
    bool PlayerIsWithinReach(float reachMultiplier)
    {
        if (Physics.SphereCast(transform.position, 1f, transform.forward * reachMultiplier, out RaycastHit hit, 1f) == true)
            if (hit.collider.gameObject.tag == "Player")
                return true;
        return false;   
    }

    void CheckCanSeePlayer()
    {
        if (CanSeePlayer() == true)
        {
            chasePlayer = true;

            // Erase agro coroutine
            agroLossCoroutine = null;
        }
        else
            // Start timer for agro loss
            if (agroLossCoroutine == null)
            {
                agroLossCoroutine = AgroLossCoroutine();
                StartCoroutine(agroLossCoroutine);
            }
    }
    void CheckIfChasePlayer()
    {
        if (chasePlayer == true)
        {
            // Owner chase speed
            nav.speed = chaseSpeed;

            // Owner animation
            animator.SetBool("walk faster", true);
            animator.SetBool("walk", false);

            // Owner destination
            nav.SetDestination(player.position);
        }
        else
        {
            // Owned default speed
            nav.speed = defaultSpeed;

            // Owner animation
            animator.SetBool("walk faster", false);
            animator.SetBool("walk", true);
        }
    }
    [HideInInspector] public bool canSeePlayer = false;
    bool chasePlayer = false;
    bool CanSeePlayer()
    {
        Vector3 rayDirection = player.position - transform.position;

        if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit))
        {
            // Check to see if player is in owners field of view
            float fieldOfViewValue = GetFieldOfViewValue();

            if (hit.transform.tag == "Player" && fieldOfViewValue > 0)
                return true;
        }
        return false;
    }
    IEnumerator AgroLossCoroutine()
    {
        yield return new WaitForSeconds(5f);
        chasePlayer = false;
    }
    float GetFieldOfViewValue()
    {
        // check if behind, ignore y
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toOther = player.position - transform.position;
        return Vector3.Dot(forward.normalized, toOther.normalized);
    }
    void CheckReachedDesination()
    {
        // Ignore the y value
        Vector2 start = new Vector2(transform.position.x, transform.position.z);
        Vector2 end = new Vector2(nav.destination.x, nav.destination.z);
        
        if (Vector2.Distance(start, end) < 1)
        {
            SetRandomDestination();
            // Invoke("SetRandomDestination", Random.Range(3f, 7.5f));
        }
    }
    public void Freeze(float time)
    {
        StartCoroutine(FreezeQuarantine(time));
    }
    IEnumerator FreezeQuarantine(float time)
    {
        // Set this bool is stopped. I think this just stops adding
        // velocity to navigation unity so i set velocity to zero
        // in next step
        nav.isStopped = true;
     
        // Set owner velocity to ZEROOO
        nav.velocity = Vector3.zero;

        // Wait time
        yield return new WaitForSeconds(time);

        nav.isStopped = false;
    }
    public Transform destinationsParent;
    Transform[] destinations;
    void SetRandomDestination()
    {
        // If currently chasing player, don't set a new destination
        // else he will jitter to destination and then back to player
        // and owner shakes
        if (chasePlayer == true)
            return;

        int index = Random.Range(0, destinations.Length);
        nav.SetDestination(destinations[index].position );

        // If idleing, then start walk animation. If we are NOT idleing, 
        // that means we must be chasing the player or something else so
        // don't crossfade cause will mess up animation SLIGHTLY but still ...
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle") == true)
            animator.CrossFade("walk", 0.2f);
    }
    Vector3 previousPositionToCheckWhenStuck;
    void CheckIfStuck()
    {
        // Debug
        // print(Vector3.Distance(previousPosition, transform.position));
        
        // The value can be reduced to be more strict or vice versa
        if (Vector3.Distance(previousPositionToCheckWhenStuck, transform.position) < 0.75f)
            SetRandomDestination();
        
        // Store the current position as previous position
        previousPositionToCheckWhenStuck = transform.position;
    }
    float defaultSpeed;
    [SerializeField] float chaseSpeed = 4f;
    void Start()
    {
        // Get player transform so we can follow player
        player = GameObject.FindWithTag("Player").transform;

        // Get player health behaviour
        playerHealth = player.GetComponent<PlayerHealth>();


        // Repeat the check if stuck function incase he gets stuck
        InvokeRepeating("CheckIfStuck", 3f, 3f);

        // Store default speed
        defaultSpeed = nav.speed;

        // Start him with a random destination path
        SetRandomDestination();
    }
    IEnumerator agroLossCoroutine;
    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();

            // Store destinations
        destinations = new Transform[destinationsParent.childCount];
        for (int i = 0; i < destinationsParent.childCount; i++)
            destinations[i] = destinationsParent.GetChild(i);

        animator = GetComponent<Animator>();
    }    
    PlayerHealth playerHealth;
    Transform player;
    NavMeshAgent nav;
    Animator animator;
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1, 1f);    
    }
}
