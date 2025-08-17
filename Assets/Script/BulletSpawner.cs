using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Pattern")]
    [SerializeField] private int shots = 8;                
    [SerializeField] private float muzzleRadius = 0.6f;    
    [SerializeField] private float forwardOffset = 0.5f;   

    [Header("Projectile Params")]
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private float turnRateDegPerSec = 360f;
    [SerializeField] private float lifeTime = 4f;

    public void FireBurst(Vector3 origin, Transform player, IReadOnlyList<Enemy> targets)
    {
        if (projectilePrefab == null || player == null)
        {
            return;
        }

        Vector3 f = player.forward.normalized;
        Vector3 r = player.right.normalized;
        Vector3 u = player.up.normalized;

        int shotsToFire = Mathf.Max(1, shots);
        int locked = (targets != null) ? Mathf.Max(1, targets.Count) : 1;

        float twoPi = Mathf.PI * 2f;
        float step = twoPi / shotsToFire;

        for (int i = 0; i < shotsToFire; i++)
        {
            float angle = step * i;
            Vector3 radial = Mathf.Cos(angle) * r + Mathf.Sin(angle) * u;

            Vector3 spawnPos = origin + radial * muzzleRadius + f * forwardOffset;
            Quaternion spawnRot = Quaternion.LookRotation(radial.normalized); 

            var go = Instantiate(projectilePrefab, spawnPos, spawnRot);

            Enemy e = (targets != null && targets.Count > 0) ? targets[i % locked] : null;
            Transform tgt = (e != null) ? e.transform : null;

            if (go.TryGetComponent<HomingProjectile>(out var homing))
            {
                homing.SetParams(projectileSpeed, turnRateDegPerSec, lifeTime);
                homing.SetTarget(tgt); 
            }
            else
            {
                if (go.TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 dir = radial.normalized;
                    rb.linearVelocity = dir * projectileSpeed;
                    go.transform.rotation = Quaternion.LookRotation(dir);
                }

                Destroy(go, lifeTime);
            }
        }
    }

    public void SetProjectileParams(float speed, float turnRateDeg, float life)
    {
        projectileSpeed = speed;
        turnRateDegPerSec = turnRateDeg;
        lifeTime = life;
    }

    public void SetPattern(int shotCount, float radius, float fwdOffset)
    {
        shots = Mathf.Max(1, shotCount);
        muzzleRadius = Mathf.Max(0f, radius);
        forwardOffset = fwdOffset;
    }
}
