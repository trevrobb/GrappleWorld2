using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    /*this script is what actually renders my rope and applies the sin animation to it, what it does it basically break my line up into individual segments and applies the animation to each segment, while lerping the rope to
      the desired position over time. It also came from a tutorial, but not the same one as where I got my swinging or grappling functionality. */
    private Spring spring;
    private LineRenderer lr;
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
        if (!GrapplingGun.instance.isGrapple() && !GrapplingGun.instance.isGrapplingg())
        {
            currentGrapplePosition = GrapplingGun.instance.gunTip.position;
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

        var grapplePoint = GrapplingGun.instance.getGrapplePoint();
        var gunTipPosition = GrapplingGun.instance.gunTip.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, GrapplingGun.instance.getGrapplePoint(), Time.deltaTime * 8f);
            
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
