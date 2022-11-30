using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
public class CamController : NetworkBehaviour
{
    [SerializeField] private float minX = -60f;
    [SerializeField] private float maxX = 60f;

    [SerializeField] private float sensitivity;
    [HideInInspector] public Camera cam;

    [SerializeField] private float rotSpeed;
    float rotY = 0f;
    float rotX = 0f;

    [SerializeField] private float camYOffset;
    [SerializeField] private float camZOffset;

    [HideInInspector] public GameObject spawnedObject;

    [SerializeField] private GameObject playerTeleportModel;

    private Quaternion newRot;

    [SerializeField] private GameObject recoilPoint;
    [SerializeField] private GameObject camPoint;
    [SerializeField] private GameObject camSpawn;

    [SerializeField] Rigidbody body;

    [SerializeField] private float targetHorizontalAngle;

    [SerializeField] private float _tmpRotationVelocity;

    [SerializeField] private float _rotationSmoothTime;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            cam = Camera.main;

            cam.transform.rotation = transform.rotation;
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + camYOffset, transform.position.z + camZOffset);
            cam.transform.SetParent(transform);
        }
        else
        {
            gameObject.GetComponent<CamController>().enabled = false;
            
        }
    }

    // Start is called before the first frame update
    void Start()
    { 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        newRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        ApplyRotation();
        ChangeCursorLockState();

        if (Input.GetKeyDown(KeyCode.P) && spawnedObject == null)
        {
            //SpawnObject(this);
        }
       
        rotY += Input.GetAxis("Mouse X") * sensitivity;
        rotX += Input.GetAxis("Mouse Y") * sensitivity;

        rotX = Mathf.Clamp(rotX, minX, maxX);

        //TeleportModel();

        Teleport();
    }

    private void ChangeCursorLockState()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void TeleportModel()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 200))
        {
            if (spawnedObject != null)
            {
                spawnedObject.transform.localRotation = Quaternion.FromToRotation(playerTeleportModel.transform.up, hit.normal) * playerTeleportModel.transform.rotation;
                spawnedObject.transform.position = hit.point;
            }

        }
    }

    private void Teleport()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetKeyUp(KeyCode.F))
        {
            if (Physics.Raycast(ray, out hit, 200))
            {
                newRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.position = hit.point;
            }
        }
    }
/*
    [ServerRpc]
    public void SpawnObject(CamController script)
    {
        GameObject spawned = Instantiate(playerTeleportModel, new Vector3(0, .5f, 0), transform.rotation);
        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }

    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawned, CamController script)
    {
        script.spawnedObject = spawned;
        Debug.Log(script.spawnedObject);
    }
*/
    private void ApplyRotation()
    {
        //targetHorizontalAngle = Mathf.SmoothDampAngle(body.rotation.eulerAngles.y, rotY, ref _tmpRotationVelocity, _rotationSmoothTime, float.MaxValue, Time.fixedDeltaTime);

        transform.rotation = newRot * Quaternion.Euler(0, rotY, 0);
        //body.MoveRotation(targetRotation);
        cam.transform.localRotation = Quaternion.Euler(-rotX, 0, 0);
    }

}
