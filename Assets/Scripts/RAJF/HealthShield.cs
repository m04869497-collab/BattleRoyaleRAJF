using System;
using UnityEngine;

namespace RAJF
{
    public sealed class HealthShield : MonoBehaviour
    {
        public event Action<HealthShield> Changed;
        public event Action<HealthShield> Died;

        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float startingShield = 0f;

        private float baseShield;
        private float temporaryShield;
        private float temporaryShieldTime;
        private bool isDead;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public float CurrentShield => Mathf.Max(0f, baseShield + temporaryShield);
        public bool IsDead => isDead;

        private void Awake()
        {
            if (CurrentHealth <= 0f)
                Initialize(maxHealth, startingShield);
        }

        private void Update()
        {
            if (temporaryShieldTime <= 0f)
                return;

            temporaryShieldTime -= Time.deltaTime;
            if (temporaryShieldTime <= 0f)
            {
                temporaryShield = 0f;
                NotifyChanged();
            }
        }

        public void Initialize(float health, float shield)
        {
            maxHealth = Mathf.Max(1f, health);
            startingShield = Mathf.Max(0f, shield);
            CurrentHealth = maxHealth;
            baseShield = startingShield;
            temporaryShield = 0f;
            temporaryShieldTime = 0f;
            isDead = false;
            NotifyChanged();
        }

        public void ApplyDamage(float amount)
        {
            if (isDead || amount <= 0f)
                return;

            float remaining = amount;
            float fromTemporary = Mathf.Min(temporaryShield, remaining);
            temporaryShield -= fromTemporary;
            remaining -= fromTemporary;

            float fromBase = Mathf.Min(baseShield, remaining);
            baseShield -= fromBase;
            remaining -= fromBase;

            if (remaining > 0f)
                CurrentHealth = Mathf.Max(0f, CurrentHealth - remaining);

            NotifyChanged();
            if (CurrentHealth <= 0f && !isDead)
            {
                isDead = true;
                Died?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (isDead || amount <= 0f)
                return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            NotifyChanged();
        }

        public void AddTemporaryShield(float amount, float duration)
        {
            if (isDead || amount <= 0f || duration <= 0f)
                return;

            temporaryShield += amount;
            temporaryShieldTime = Mathf.Max(temporaryShieldTime, duration);
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke(this);
        }
    }
}
