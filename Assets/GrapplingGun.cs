using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    [SerializeField] LayerMask grappleObject;
    public Transform gunTip, camera, player;
    float maxDistance = 100f;
    [SerializeField] float grappleDelayTime;
    SpringJoint joint;
    [SerializeField] Player _player;
    private Boolean isGrappling;
    private Boolean grappling;
    [SerializeField] float overshootYAxis;

    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    public KeyCode grappleKey = KeyCode.Mouse1;

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
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
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


    void StartGrapple()
    {
        if (predictionHit.point == Vector3.zero) return;
        if (grappling == true) StopGrappling();
        _player.ResetRestrictions();

       
            isGrappling = true;
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

    void StopGrapple()
    {
        Destroy(joint);
        isGrappling = false;
    }

    public Vector3 getGrapplePoint()
    {
        return grapplePoint;
    }

    public Boolean isGrapple()
    {
        return isGrappling;
    }
    public Boolean isGrapplingg()
    {
        return grappling;
    }

    private void StartGrappling()
    {
        if (grapplingCdTimer > 0) return;
        grappling = true;
        StopGrapple();
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
