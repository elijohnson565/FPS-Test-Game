using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
public class Recoil : NetworkBehaviour
{

    private Vector3 currentRotationCam;
    private Vector3 currentRotationGun;
    private Vector3 targetRotation;

    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    [SerializeField] private float gunSnappiness;
    [SerializeField] private float returnSpeed;
    [SerializeField] private CamController camScript;

    [SerializeField] private GameObject gunModel;

    [SerializeField] private float kickSpeed;

    Vector3 pointA;
    Vector3 pointB;
    // Start is called before the first frame update
    void Start()
    {
        pointA = gunModel.transform.localPosition;
        pointB = gunModel.transform.localPosition + new Vector3(0, 0, .03f);
    }

    // Update is called once per frame
    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotationGun = Vector3.Slerp(currentRotationGun, targetRotation, gunSnappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotationGun);
    }



    public void RecoilFire()
    {
        float time = Mathf.PingPong(Time.time * kickSpeed, 1);
        gunModel.transform.localPosition = Vector3.Lerp(pointA, pointB, time);
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
