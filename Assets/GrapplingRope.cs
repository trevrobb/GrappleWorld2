using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    private Spring spring;
    private LineRenderer lr;
    public GrapplingGun grapplingGun;
    private Vector3 currentGrapplePosition;
    [SerializeField] float damper;
    [SerializeField] int quality;
    [SerializeField] float strength;
    [SerializeField] float velocity;
    [SerializeField] float waveCount;
    [SerializeField] float waveHeight;
    [SerializeField] AnimationCurve affectCurve;
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }
    void DrawRope()
    {
        if (!grapplingGun.isGrapple() && !grapplingGun.isGrapplingg())
        {
            currentGrapplePosition = grapplingGun.gunTip.position;
            spring.Reset();
            if (lr.positionCount > 0)
            {
                lr.positionCount = 0;
            }
            return;
        }
        
        if (lr.positionCount == 0)
        {    
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }
        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var grapplePoint = grapplingGun.getGrapplePoint();
        var gunTipPosition = grapplingGun.gunTip.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplingGun.getGrapplePoint(), Time.deltaTime * 8f);
            
        for (int i = 0; i < quality+1; i++)
        {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);
            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }


    }
    private void LateUpdate()
    {
        DrawRope();
    }

   
}
