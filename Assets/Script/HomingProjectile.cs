using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;
    public float turnRateDegPerSec = 360f;
    public float lifeTime = 6f;
    public float hitRadius = 0.5f;

    private float _life;

    public void SetTarget(Transform t) => target = t;
    public void SetParams(float s, float turn, float life)
    {
        speed = s;
        turnRateDegPerSec = turn;
        lifeTime = life;
    }

    void OnEnable()
    {
        _life = lifeTime;
    }

    void Update()
    {
        _life -= Time.deltaTime;
        if (_life <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 desiredDir = target != null
            ? (target.position - transform.position).normalized
            : transform.forward;

        Vector3 currentDir = transform.forward;
        float maxRadians = Mathf.Deg2Rad * turnRateDegPerSec * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(currentDir, desiredDir, maxRadians, 0f);

        transform.position += newDir * speed * Time.deltaTime;

        if (newDir != Vector3.zero)
        {
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, newDir).normalized;
            up = Vector3.Cross(newDir, right).normalized;

            Matrix4x4 rotMatrix = new Matrix4x4();
            rotMatrix.SetColumn(0, right);
            rotMatrix.SetColumn(1, up);
            rotMatrix.SetColumn(2, newDir);
            rotMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));

            transform.rotation = rotMatrix.rotation;
        }

        if (target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            if (toTarget.sqrMagnitude < hitRadius * hitRadius)
            {
                var enemy = target.GetComponent<Enemy>();
                if (enemy != null) enemy.PlayHitReaction();

                Destroy(gameObject);
            }
        }
    }

}
