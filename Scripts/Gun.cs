using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
public class Gun : NetworkBehaviour
{

    public float range;
    public float damage;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private Recoil recoil;
    [SerializeField] private AudioSource gunSFX;
    [SerializeField] private float fireRate;
    private float nextTimeToFire;
    private float shotTimer = 3;
    [HideInInspector] public GameObject spawnedObject;
    public CamController camScript;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            GetComponent<Gun>().enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        nextTimeToFire = shotTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Shoot();
            recoil.RecoilFire();
        }

        if (nextTimeToFire < shotTimer)
        {
            nextTimeToFire += 1 * (Time.deltaTime * fireRate);
         
        }
       
    }

    [ServerRpc]
    public void SpawnObject(Gun script)
    {
        GameObject spawned = Instantiate(bulletHolePrefab, new Vector3(0, .5f, 0), transform.rotation);
        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }

    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawned, Gun script)
    {
        script.spawnedObject = spawned;
        Debug.Log(script.spawnedObject);
    }

    void Shoot()
    {
        RaycastHit hit;
        
        if(nextTimeToFire >= shotTimer)
        {
            if (Physics.Raycast(camScript.cam.transform.position, camScript.cam.transform.forward, out hit, range))
            {
                //Debug.Log(hit.transform.name);
                SpawnObject(this);
                spawnedObject.transform.position += Quaternion.LookRotation(hit.normal) * spawnedObject.transform.forward / 1000;
                Destroy(spawnedObject, 5f);
            }

            gunSFX.Play();
            nextTimeToFire = 0;
        }

        
    }


}
