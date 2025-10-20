using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))] // Compiler attributes ensures that any GameObject this script is attached to has a Rigidbody component
public class Projectile : MonoBehaviour
{
    const int LOOKBACK_COUNT = 10; // max length
    static List<Projectile> PROJECTILES = new List<Projectile>();
    [SerializeField]
    private bool _awake = true;
    public bool awake
    {
        get
        {
            return _awake;
        }
        private set
        {
            _awake = value;
        }
    }

    private Vector3 prevPos;
    //This private list stores the history of Projectile's move distance 
    private List<float> deltas = new List<float>();
    private Rigidbody rigid;

    // 🎧 Impact audio
    public AudioClip hitSound;          // assign in Inspector
    private AudioSource audioSource;    // set in Start()

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();

        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        awake = true;
        prevPos = new Vector3(1000, 1000, 0);
        deltas.Add(1000);

        PROJECTILES.Add(this);
    }

    void FixedUpdate()
    {
        if (rigid.isKinematic || !awake)
        {
            return;
        }

        Vector3 deltaV3 = transform.position - prevPos;
        deltas.Add(deltaV3.magnitude);
        prevPos = transform.position;

        //Limit lookback; one of very few times that I'll use while!
        while (deltas.Count > LOOKBACK_COUNT)
        {
            deltas.RemoveAt(0);
        }
        //Iterate over deltas and find the greatest one 
        float maxDelta = 0;

        foreach (float f in deltas)
        {
            if (f > maxDelta)
            {
                maxDelta = f;
            }
        }

        // If the Projectile hasn't moved more than the sleepThreshold
        if (maxDelta <= Physics.sleepThreshold)
        {
            //Set awake to false and put the Rigidbody to sleep 
            awake = false;
            rigid.Sleep();
        }
    }
    
    // 🎯 Play impact sound when colliding
    // 🎯 Play impact sound every time the projectile hits anything
    void OnCollisionEnter(Collision collision)
    {
        if (hitSound == null || audioSource == null) return;

        // Louder for harder impacts
        float impactVolume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 10f);
        audioSource.PlayOneShot(hitSound, impactVolume);
    }


    private void OnDestroy()
    {
        PROJECTILES.Remove(this);
    }
    
    static public void DESTROY_PROJECTILES()
    {
        foreach(Projectile p in PROJECTILES)
        {
            Destroy(p.gameObject);
        }
    }
}
