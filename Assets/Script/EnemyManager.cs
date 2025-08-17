using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;
    public IReadOnlyList<Enemy> AllEnemies => _enemies;

    private readonly List<Enemy> _enemies = new List<Enemy>();
    private readonly List<Enemy> _locked = new List<Enemy>();

    [Header("Selection")]
    [Tooltip("ビューポート中心からの距離で選別。画面外は除外。")]
    public float viewportMargin = 0.05f; 
    [Tooltip("必要なら前方に限定（dot > 0）。")]
    public bool requireFront = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(Enemy e)
    {
        if (e != null && !_enemies.Contains(e))
        {
            _enemies.Add(e);
        }
    }

    public void Unregister(Enemy e)
    {
        if (e == null) return;
        _enemies.Remove(e);
        if (_locked.Remove(e))
        {
            e.SetLocked(false);
        }
    }

    public IReadOnlyList<Enemy> GetLocked() => _locked;

    public void ClearLocks()
    {
        for (int i = 0; i < _locked.Count; i++)
        {
            if (_locked[i] != null) _locked[i].SetLocked(false);
        }
        _locked.Clear();
    }

    public void SelectLocks(Camera cam, Transform player, int maxCount)
    {
        if (cam == null || player == null) return;

        var candidates = ListPool<ScoredEnemy>.Get();
        candidates.Clear();

        for (int i = 0; i < _enemies.Count; i++)
        {
            var e = _enemies[i];
            if (e == null || !e.isActiveAndEnabled) continue;

            Vector3 vp = cam.WorldToViewportPoint(e.LockPoint.position);
            Debug.Log($"{e.name} → vp=({vp.x:F2}, {vp.y:F2}, {vp.z:F2})");
            if (vp.z <= 0f) continue; 

            if (requireFront)
            {
                Vector3 to = (e.LockPoint.position - cam.transform.position).normalized;
                if (Vector3.Dot(cam.transform.forward, to) <= 0f) continue;
            }

            float margin = viewportMargin;
            if (vp.x < -margin || vp.x > 1f + margin || vp.y < -margin || vp.y > 1f + margin)
                continue;

            float dx = vp.x - 0.5f;
            float dy = vp.y - 0.5f;
            float score = dx * dx + dy * dy; 

            candidates.Add(new ScoredEnemy { enemy = e, score = score });
        }

        candidates.Sort((a, b) => a.score.CompareTo(b.score));

        var newLocked = ListPool<Enemy>.Get();
        newLocked.Clear();

        for (int i = 0; i < candidates.Count && i < maxCount; i++)
        {
            newLocked.Add(candidates[i].enemy);
        }

        for (int i = 0; i < _locked.Count; i++)
        {
            var e = _locked[i];
            if (e != null && !newLocked.Contains(e))
                e.SetLocked(false);
        }
        for (int i = 0; i < newLocked.Count; i++)
        {
            var e = newLocked[i];
            if (e != null && !_locked.Contains(e))
                e.SetLocked(true);
        }

        _locked.Clear();
        _locked.AddRange(newLocked);

        ListPool<ScoredEnemy>.Release(candidates);
        ListPool<Enemy>.Release(newLocked);
    }

    struct ScoredEnemy
    {
        public Enemy enemy;
        public float score;
    }
}

public static class ListPool<T>
{
    static readonly Stack<List<T>> pool = new Stack<List<T>>();

    public static List<T> Get()
    {
        return pool.Count > 0 ? pool.Pop() : new List<T>(64);
    }

    public static void Release(List<T> list)
    {
        list.Clear();
        pool.Push(list);
    }
}
