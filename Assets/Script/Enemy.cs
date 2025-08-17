using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class Enemy : MonoBehaviour
{
    [Header("Visual")]
    public Color baseColor = Color.white;
    public Color lockedColor = Color.red;
    public Transform lockPoint;

    [Header("Float Motion")]
    public bool enableFloat = true;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1.0f;
    public float swayAmplitude = 0.25f;
    public float swayFrequency = 0.5f;

    [Header("Reaction")]
    public float reactionDuration = 0.5f; 
    public float shakeMagnitude = 0.05f;

    private Renderer _renderer;
    private Material _material;
    private bool _locked = false;
    private bool _isReacting = false;
    private Vector3 _startPos;
    private float _seed;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material = new Material(_renderer.material);

        if (lockPoint == null) lockPoint = transform;
        _startPos = transform.localPosition;
        _seed = Random.Range(0f, 100f);
        ApplyColor(baseColor);
    }

    void OnEnable()
    {
        StartCoroutine(DeferredRegister());
    }

    IEnumerator DeferredRegister()
    {
        while (!EnemyManager.HasInstance)
            yield return null;

        EnemyManager.Instance.Register(this);
        ApplyColor(baseColor);
    }

    void OnDisable()
    {
        if (EnemyManager.HasInstance)
        {
            EnemyManager.Instance.Unregister(this);
        }
    }

    void Update()
    {
        if (enableFloat && !_isReacting)
        {
            float yOffset = Mathf.Sin((Time.time + _seed) * floatFrequency * Mathf.PI * 2f) * floatAmplitude;
            float xOffset = Mathf.Sin((Time.time + _seed) * swayFrequency * Mathf.PI * 2f) * swayAmplitude;

            transform.localPosition = _startPos + transform.right * xOffset + Vector3.up * yOffset;
        }
    }

    public Transform LockPoint => lockPoint;

    public void SetLocked(bool value)
    {
        if (_locked == value) return;
        _locked = value;
        ApplyColor(_locked ? lockedColor : baseColor);
    }

    private void ApplyColor(Color c)
    {
        _material.color = c;
    }

    public void PlayHitReaction()
    {
        if (_isReacting) return;
        StartCoroutine(HitReactionRoutine());
    }

    IEnumerator HitReactionRoutine()
    {
        _isReacting = true;
        enableFloat = false;

        ApplyColor(Color.yellow);
        Vector3 originalPos = _startPos;

        float elapsed = 0f;
        while (elapsed < reactionDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = originalPos + transform.right * x + Vector3.up * y;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        ApplyColor(_locked ? lockedColor : baseColor);
        enableFloat = true;
        _isReacting = false;
    }
}
