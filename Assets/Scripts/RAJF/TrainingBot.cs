using UnityEngine;

namespace RAJF
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(HealthShield))]
    public sealed class TrainingBot : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private HealthShield health;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.2f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
        [SerializeField, Min(0.1f)] private float attackInterval = 1.25f;
        [SerializeField, Min(0f)] private float attackDamage = 8f;

        private GameSession session;
        private HealthShield targetHealth;
        private Transform target;
        private Vector3 spawnPosition;
        private float nextAttackTime;
        private float verticalVelocity;
        private float maxHealth = 60f;
        private bool isDead;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (health == null)
                health = GetComponent<HealthShield>();
            if (health != null)
                health.Died += HandleDeath;
            spawnPosition = transform.position;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= HandleDeath;
        }

        public void Initialize(GameSession owner, HealthShield playerHealth, Transform playerTransform)
        {
            session = owner;
            targetHealth = playerHealth;
            target = playerTransform;
            spawnPosition = transform.position;
        }

        public void PrepareForMatch(GameSession owner, HealthShield playerHealth,
            Transform playerTransform, float healthAmount)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            session = owner;
            targetHealth = playerHealth;
            target = playerTransform;
            maxHealth = Mathf.Max(1f, healthAmount);
            isDead = false;
            nextAttackTime = Time.time + 1f;
            verticalVelocity = 0f;
            transform.position = spawnPosition;
            if (health != null)
                health.Initialize(maxHealth, 0f);
        }

        public void ApplyDamage(float amount)
        {
            if (!isDead && health != null)
                health.ApplyDamage(amount);
        }

        private void Update()
        {
            if (isDead || session == null || !session.IsPlaying ||
                target == null || targetHealth == null || targetHealth.IsDead ||
                characterController == null)
                return;

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > attackRange)
            {
                Vector3 direction = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector3.zero;
                if (characterController.isGrounded)
                    verticalVelocity = -2f;
                else
                    verticalVelocity += Physics.gravity.y * Time.deltaTime;
                Vector3 movement = direction * moveSpeed;
                movement.y = verticalVelocity;
                characterController.Move(movement * Time.deltaTime);
                if (direction.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            else if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackInterval;
                targetHealth.ApplyDamage(attackDamage);
            }
        }

        private void HandleDeath(HealthShield ignored)
        {
            if (isDead)
                return;
            isDead = true;
            if (session != null)
                session.NotifyBotEliminated(this);
            gameObject.SetActive(false);
        }
    }
}
