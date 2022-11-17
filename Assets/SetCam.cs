using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SetCam : MonoBehaviour
{
    [SerializeField] Transform camPosition;
    // Update is called once per frame
    void Update()
    {
        transform.position = camPosition.position;
    }
}
