using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CameraController : MonoBehaviour
{
    
   [SerializeField] Camera mainCam;
   [SerializeField] Camera blackCam;

   private float speed = 8f;
   private float returnSpeed = .01f;

   private Vector3 originalPosition;
   private Quaternion originalRotation;


    /// <summary>
    /// Called when camera controller is active
    /// </summary>
   private void Awake()
   {
        originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        originalRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
   }

    /// <summary>
    /// Activate the main cam
    /// </summary>
    public void SetMainCamera()
    {
        switch(Client.Instance.player)
        {
            case 1:
                mainCam.enabled = false;
                blackCam.enabled = true;
                break;
            default:
                mainCam.enabled = true;
                blackCam.enabled = false;
                break;
        }

    }

    /// <summary>
    /// Toggle between cameras
    /// </summary>
    public void SwapCameras()
    {

        if (mainCam.enabled)
        {
            mainCam.enabled = false;
            blackCam.enabled = true;
        }
        else
        {
            mainCam.enabled = true;
            blackCam.enabled = false;
        }
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    /// <summary>
    /// Called once per frame.
    /// 
    /// Handles the camera movement
    /// </summary>
    void Update()
    {

        if (!Input.GetKey(KeyCode.LeftArrow) && 
            !Input.GetKey(KeyCode.RightArrow) &&
            !Input.GetKey(KeyCode.UpArrow) &&
            !Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, returnSpeed) ;
            
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, returnSpeed);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
      
            transform.Rotate(new Vector3(speed * 5 * Time.deltaTime, 0, 0));
            transform.position =  new Vector3(transform.position.x, transform.position.y, (transform.position.z + speed * Time.deltaTime));
        }

        if(Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0, speed * 5 * Time.deltaTime, 0));

        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, -speed * 5 * Time.deltaTime, 0));
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(new Vector3(-speed * 5 * Time.deltaTime, 0, 0));
            transform.position =  new Vector3(transform.position.x, transform.position.y, (transform.position.z - speed * Time.deltaTime * .5f));

        }

        if (Input.GetKey(KeyCode.C))
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
 }
}
