using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    // fields set in the Unity Inspector game 
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    // fields set dynamically
    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;
    public LineRenderer rubber;
    public Transform anchorPoint;

    [Header("Audio")]
    public AudioSource audioSource;   // The AudioSource component on the Slingshot
    public AudioClip snapSound;       // The rubber snap sound effect

    
    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        // Rubber band setup
        if(rubber != null)
        {
            rubber.positionCount = 2;
            rubber.startWidth = 0.04f;
            rubber.endWidth = 0.02f;
            rubber.enabled = false;
        }
    }
    void OnMouseEnter()
    {
        // print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        // print("Slingshot: OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        // The player has pressed the mouse button while over Slingshot
        aimingMode = true;
        // Instantiate a Projectile
        projectile = Instantiate(projectilePrefab) as GameObject;
        // Start it at the launchPoint
        projectile.transform.position = launchPos;
        // Set it to isKinematic for now 
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update()
    {
        // If Slingshot is not in aimingMode, don't run this code 
        if (!aimingMode) return;

        // Get the current mouse position in 2D screen coordinates 
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;
        // Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;

        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // Move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // Update the rubber band to follow the projectile 
        if(rubber != null)
        {
            rubber.enabled = true;

            //Start point = slingshot ancher, endpoint = projectile
            Vector3 anchorPos = (anchorPoint != null) ? anchorPoint.position : launchPos;
            rubber.SetPosition(0, anchorPos);
            rubber.SetPosition(1, projectile.transform.position);

            // Optional: dynamic thickness based on stretch distance
            float dist = Vector3.Distance(anchorPos, projectile.transform.position);
            rubber.startWidth = Mathf.Lerp(0.03f, 0.06f, dist / 3f);
            rubber.endWidth = Mathf.Lerp(0.02f, 0.04f, dist / 3f);
        }

        if(Input.GetMouseButtonUp(0)) // This will only return true on the Update that the player released the mouse button
        {
            // The mouse has been released 
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false; // allows the Projectile to now fly through the air based on physics simulation 
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous; // page 698

            // 🔊 Play rubber band snap sound here
            if (audioSource != null && snapSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // Optional: adds realism
                audioSource.PlayOneShot(snapSound);
            }
            projRB.velocity = -mouseDelta * velocityMult;

            // Switch to slingshot view immediately before setting POI 
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);                       // a

            FollowCam.POI = projectile; // Set the _MainCamera POI
            // Add a ProjectileLine to the Projectile
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;

            if(rubber != null)
            {
                rubber.enabled = false;
            }
            MissionDemolition.SHOT_FIRED();
        }
    }

}
