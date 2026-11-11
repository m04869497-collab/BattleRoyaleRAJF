using UnityEngine;

namespace RAJF
{
    public sealed class StormController : MonoBehaviour
    {
        [SerializeField] private LineRenderer boundary;
        [SerializeField] private HealthShield playerHealth;
        [SerializeField] private Transform player;
        [SerializeField] private GameSession session;
        [SerializeField] private Vector3 center = Vector3.zero;
        [SerializeField, Min(1f)] private float startRadius = 32f;
        [SerializeField, Min(1f)] private float finalRadius = 6f;
        [SerializeField, Min(0f)] private float startDelay = 20f;
        [SerializeField, Min(1f)] private float shrinkDuration = 90f;
        [SerializeField, Min(0f)] private float damagePerSecond = 6f;

        private float elapsed;
        private bool started;

        public float CurrentRadius { get; private set; }
        public string StatusText
        {
            get
            {
                if (!started)
                    return "Storm in " + Mathf.CeilToInt(Mathf.Max(0f, startDelay - elapsed)) + "s";
                float remaining = Mathf.Max(0f, shrinkDuration - (elapsed - startDelay));
                return remaining > 0f
                    ? "Storm: " + Mathf.CeilToInt(remaining) + "s"
                    : "Storm: final zone";
            }
        }

        public void Configure(GameSession owner, HealthShield health, Transform playerTransform,
            LineRenderer line, Vector3 zoneCenter, float initialRadius, float endRadius,
            float delay, float duration, float damage)
        {
            session = owner;
            playerHealth = health;
            player = playerTransform;
            boundary = line;
            center = zoneCenter;
            startRadius = Mathf.Max(1f, initialRadius);
            finalRadius = Mathf.Clamp(endRadius, 1f, startRadius);
            startDelay = Mathf.Max(0f, delay);
            shrinkDuration = Mathf.Max(1f, duration);
            damagePerSecond = Mathf.Max(0f, damage);
            CurrentRadius = startRadius;
            DrawBoundary();
        }

        public void BeginMatch()
        {
            elapsed = 0f;
            started = false;
            CurrentRadius = startRadius;
            DrawBoundary();
        }

        private void Update()
        {
            if (session == null || !session.IsPlaying)
                return;

            elapsed += Time.deltaTime;
            if (elapsed >= startDelay)
            {
                started = true;
                float progress = Mathf.Clamp01((elapsed - startDelay) / shrinkDuration);
                CurrentRadius = Mathf.Lerp(startRadius, finalRadius, progress);
            }
            else
            {
                CurrentRadius = startRadius;
            }

            DrawBoundary();
            if (started && player != null && playerHealth != null && !playerHealth.IsDead)
            {
                Vector3 flatPlayer = player.position;
                flatPlayer.y = center.y;
                if (Vector3.Distance(flatPlayer, center) > CurrentRadius)
                    playerHealth.ApplyDamage(damagePerSecond * Time.deltaTime);
            }
        }

        private void DrawBoundary()
        {
            if (boundary == null)
                return;

            const int segments = 64;
            boundary.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                boundary.SetPosition(i, center + new Vector3(
                    Mathf.Cos(angle) * CurrentRadius, 0.1f, Mathf.Sin(angle) * CurrentRadius));
            }
        }
    }
}
