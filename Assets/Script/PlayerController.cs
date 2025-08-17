using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;      
    public float strafeSpeed = 10f;   
    public float ascendSpeed = 8f;    

    [Header("Rotation (Arrow keys)")]
    public float yawSpeed = 120f;      
    public float pitchSpeed = 90f;    
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Lock-on & Firing")]
    public int maxLocks = 8;
    public KeyCode lockKey = KeyCode.Z;
    public BulletSpawner spawner;
    public Camera viewCamera;

    private float pitchAccum = 0f;

    void Awake()
    {
        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }
        if (spawner == null)
        {
            var spawner = Object.FindFirstObjectByType<BulletSpawner>();
        }
    }

    void Update()
    {
        HandleMove();
        HandleRotate();
        HandleLockAndFire();
    }

    void HandleMove()
    {
        Vector3 v = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) v += Vector3.forward * moveSpeed;
        if (Input.GetKey(KeyCode.S)) v += Vector3.back * moveSpeed;
        if (Input.GetKey(KeyCode.A)) v += Vector3.left * strafeSpeed;
        if (Input.GetKey(KeyCode.D)) v += Vector3.right * strafeSpeed;

        if (Input.GetKey(KeyCode.Q)) v += Vector3.up * ascendSpeed;
        if (Input.GetKey(KeyCode.E)) v += Vector3.down * ascendSpeed;

        transform.Translate(v * Time.deltaTime, Space.Self);
    }

    void HandleRotate()
    {
        float yaw = 0f;
        float pitch = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) yaw -= yawSpeed;
        if (Input.GetKey(KeyCode.RightArrow)) yaw += yawSpeed;
        if (Input.GetKey(KeyCode.DownArrow)) pitch += pitchSpeed;
        if (Input.GetKey(KeyCode.UpArrow)) pitch -= pitchSpeed;

        transform.Rotate(0f, yaw * Time.deltaTime, 0f, Space.Self);

        pitchAccum += pitch * Time.deltaTime;
        pitchAccum = Mathf.Clamp(pitchAccum, minPitch, maxPitch);

        Vector3 euler = transform.localEulerAngles;

        float currentYaw = euler.y;
        transform.localRotation = Quaternion.Euler(pitchAccum, currentYaw, 0f);
    }

    void HandleLockAndFire()
    {
        if (Input.GetKey(lockKey))
        {
            EnemyManager.Instance.SelectLocks(viewCamera, transform, maxLocks);
        }

        if (Input.GetKeyUp(lockKey))
        {
            var locked = EnemyManager.Instance.GetLocked();
            if (locked.Count > 0 && spawner != null)
            {
                spawner.FireBurst(transform.position, transform, locked);
            }
            EnemyManager.Instance.ClearLocks();
        }
    }
}
