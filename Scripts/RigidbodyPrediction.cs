using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

/*
* 
* See TransformPrediction.cs for more detailed notes.
* 
*/

namespace FishNet.Example.Prediction.Rigidbodies
{

    public class RigidbodyPrediction : NetworkBehaviour
    {
        #region Types.
        public struct MoveData
        {
            public bool Jump;
            public float Horizontal;
            public float Vertical;
            public bool Rotate;
            public float RotY;
            public Quaternion NewRot;
            public MoveData(bool jump, bool rotate, float horizontal, float vertical, float rotY, Quaternion newRot)
            {
                Jump = jump;
                Rotate = rotate;
                Horizontal = horizontal;
                Vertical = vertical;
                RotY = rotY;
                NewRot = newRot;
            }
        }
        public struct ReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;
            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
            {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                AngularVelocity = angularVelocity;
            }
        }
        #endregion

        #region Serialized.
        [SerializeField]
        private float _jumpForce = 15f;
        [SerializeField]
        private float _moveRate = 15f;

        [SerializeField]
        private bool isGrounded;

        [SerializeField]
        private Transform groundCheck;

        [SerializeField]
        private LayerMask groundMask;

        [SerializeField] private float sensitivity;

        [SerializeField] private float rotSpeed;

        [SerializeField] private CamController camScript;
        #endregion

        #region Private.
        /// <summary>
        /// Rigidbody on this object.
        /// </summary>
        private Rigidbody _rigidbody;
        /// <summary>
        /// Next time a jump is allowed.
        /// </summary>
        private float _nextJumpTime;
        /// <summary>
        /// True to jump next frame.
        /// </summary>
        private bool _jump;

        private float horizontal;
        private float vertical;

        private Quaternion newRot;

        private bool rotate;

        private Camera cam;

        private Vector3 hitPoint;
        private Vector3 hitNormal;

        #endregion

        float rotY = 0f;


        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
            {
                cam = Camera.main;
            }

        }

        private void Awake()
        {
            newRot = transform.rotation;
            _rigidbody = GetComponent<Rigidbody>();
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
            }
        }
        private void Teleport()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (Physics.Raycast(ray, out hit, 200))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    rotate = true;
                }
            }
       
        }


        private void Update()
        {
            if (base.IsOwner)
            {
               
                if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextJumpTime)
                {
                    _nextJumpTime = Time.time + 1f;
                    _jump = true;
                }

                Teleport();

                

            }
        }

        private void GroundCheck()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.50f, groundMask);
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconciliation(default, false);
                CheckInput(out MoveData md);
                Move(md, false);
            }
            if (base.IsServer)
            {
                Move(default, true);
            }
        }


        private void TimeManager_OnPostTick()
        {
            if (base.IsServer)
            {
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity);
                Reconciliation(rd, true);
            }
        }

        private void CheckInput(out MoveData md)
        {
            md = default;

            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
            rotY += Input.GetAxis("Mouse X");

            if (horizontal == 0f && vertical == 0f && !_jump && !rotate && rotY == 0)
                return;

          

            md = new MoveData(_jump, rotate, horizontal, vertical, rotY, newRot);
            _jump = false;
            rotate = false;
        }

        [Replicate]
        private void Move(MoveData md, bool asServer, bool replaying = false)
        {
            if (md.Rotate)
            {
                newRot = Quaternion.FromToRotation(transform.up, hitNormal) * transform.rotation;
            }

            GroundCheck();

            _rigidbody.AddForce((transform.forward * md.Vertical) * _moveRate);
            _rigidbody.AddForce((transform.right * md.Horizontal) * _moveRate);
            transform.rotation = newRot * Quaternion.Euler(0, md.RotY * sensitivity, 0);

            if (md.Rotate)
            {
                _rigidbody.position = hitPoint;
            }

            if (!isGrounded)
            {
                _rigidbody.AddForce(-transform.up * 85, ForceMode.Force);
            }

            if (md.Jump)
                _rigidbody.AddForce(transform.up * _jumpForce, ForceMode.Force);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _rigidbody.velocity = rd.Velocity;
            _rigidbody.angularVelocity = rd.AngularVelocity;
        }


    }


}