using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GrapplingGun : MonoBehaviour
{
    /* Watched a tutorial for this, again it has been customized a bit to fit what I want it to be, and it is a culmination of multiple tutorials rather than watching just one, this script handles the core functionality of swinging
      and grappling, I have a good understanding of what it does but definitely could not have came up with this by myself and for the most part got rid of all the public variables and whatnot plaguing this script. Both the 
      swinging and the grappling come from different tutorials, so I have combined them into this one singular script */

    private LineRenderer lr;
    private Vector3 grapplePoint;
    [SerializeField] LayerMask grappleObject;
    public Transform gunTip, camera, player; //Cannot figure out how to not make this one public
    float maxDistance = 100f;
    [SerializeField] float grappleDelayTime;
    SpringJoint joint;
    [SerializeField] Player _player;
    private Boolean isSwinging;
    private Boolean grappling;
    [SerializeField] float overshootYAxis;

    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    [SerializeField] KeyCode grappleKey = KeyCode.Mouse1;

    [SerializeField] Transform orientation;
    [SerializeField] Rigidbody rb;
    [SerializeField] float horinontalThrustForce;
    [SerializeField] float forwardThrustForce;
    [SerializeField] float extendCableSpeed;

    [SerializeField] RaycastHit predictionHit;
    [SerializeField] float predictionSphereCastRadius;
    [SerializeField] Transform predictionPoint;
    public static GrapplingGun instance;
    [SerializeField] AudioClip swing; 
    [SerializeField] AudioClip grap;



    private void Awake()
    {
        _player.GetComponent<Player>();
        instance = this;
    }




    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSwinging();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopSwinging();
        }

        if (Input.GetKeyDown(grappleKey))
        {
            
            StartGrappling();
        }

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        if (joint != null) odmMovement();

        checkForSwingPoint();
    }


    void StartSwinging()
    {
        if (predictionHit.point == Vector3.zero) return;
        if (grappling == true) StopGrappling();
        _player.ResetRestrictions();

       
            isSwinging = true;
            AudioSource.PlayClipAtPoint(swing, transform.position);
            grapplePoint = predictionHit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 15f;
            joint.damper = 20f;
            joint.massScale = 20f;
            


        
    }

    void StopSwinging()
    {
        Destroy(joint);
        isSwinging = false;
    }

    public Vector3 getGrapplePoint()
    {
        return grapplePoint;
    }

    public Boolean isGrapple()
    {
        return isSwinging;
    }
    public Boolean isGrapplingg()
    {
        return grappling;
    }

    private void StartGrappling()
    {
        if (grapplingCdTimer > 0) return;
        grappling = true;
        StopSwinging();
        RaycastHit hit;
        if (predictionHit.point != Vector3.zero)
        {
            grapplePoint = predictionHit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);

        }
        else
        {
            grapplePoint = camera.position + camera.forward * maxDistance;
            Invoke(nameof(StopGrappling), grappleDelayTime);
        }
        }
    
    private void ExecuteGrapple()
    {
        _player.freeze = false;
        AudioSource.PlayClipAtPoint(grap, transform.position);
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        _player.JumpToPosition(grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrappling), 0.8f);
        

    }
    public void StopGrappling()
    { 
        _player.freeze = false;
        grappling = false;
        grapplingCdTimer = grapplingCd;
    }
    //Movement script that allows you to do a lot more with your swinging, called odmMovement as a nod to Attack on Titan :)
    public void odmMovement()
    {
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horinontalThrustForce * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horinontalThrustForce * Time.deltaTime);
        if (Input.GetKey(KeyCode.W)) rb.AddForce(-orientation.forward * forwardThrustForce * Time.deltaTime);
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = grapplePoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);
            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * .25f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;
            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * .25f;
        }


    }
    private void checkForSwingPoint()
    {
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(camera.position, predictionSphereCastRadius, camera.forward, out sphereCastHit, maxDistance, grappleObject);

        RaycastHit raycastHit;
        Physics.Raycast(camera.position, camera.forward, out raycastHit, maxDistance, grappleObject);

        Vector3 realHitPoint;
        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        else
        {
            realHitPoint = Vector3.zero;
        }
        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
            
            
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;

    }


    

    
}
