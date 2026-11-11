using System;
using UnityEngine;

namespace RAJF
{
    [Serializable]
    public sealed class CharacterDefinition
    {
        [SerializeField] private CharacterId id;
        [SerializeField] private string displayName = "Scavenger";
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float maxShield = 20f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 5.5f;
        [SerializeField, Min(1f)] private float weaponDamage = 18f;
        [SerializeField, Min(1f)] private float weaponRange = 40f;
        [SerializeField, Min(0f)] private float abilityDuration = 3f;
        [SerializeField, Min(0.1f)] private float abilityCooldown = 12f;
        [SerializeField, Min(0f)] private float abilityPower = 1.5f;

        public CharacterId Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? id.ToString() : displayName;
        public float MaxHealth => Mathf.Max(1f, maxHealth);
        public float MaxShield => Mathf.Max(0f, maxShield);
        public float MoveSpeed => Mathf.Max(0.1f, moveSpeed);
        public float WeaponDamage => Mathf.Max(1f, weaponDamage);
        public float WeaponRange => Mathf.Max(1f, weaponRange);
        public float AbilityDuration => Mathf.Max(0f, abilityDuration);
        public float AbilityCooldown => Mathf.Max(0.1f, abilityCooldown);
        public float AbilityPower => Mathf.Max(0f, abilityPower);
        public string AbilityDescription
        {
            get
            {
                switch (id)
                {
                    case CharacterId.Scavenger:
                        return "Sprint: +50% speed for " + Mathf.CeilToInt(AbilityDuration) + "s";
                    case CharacterId.Phantom:
                        return "Cloak: hidden for " + Mathf.CeilToInt(AbilityDuration) + "s";
                    case CharacterId.MysticMedic:
                        return "Heal: restore " + Mathf.RoundToInt(AbilityPower) + " health";
                    case CharacterId.HeavyDefender:
                        return "Shield: +" + Mathf.RoundToInt(AbilityPower) + " for " +
                               Mathf.CeilToInt(AbilityDuration) + "s";
                    default:
                        return "Special ability";
                }
            }
        }

        public CharacterDefinition(
            CharacterId characterId,
            string name,
            float health,
            float shield,
            float speed,
            float damage,
            float range,
            float duration,
            float cooldown,
            float power)
        {
            id = characterId;
            displayName = name;
            maxHealth = health;
            maxShield = shield;
            moveSpeed = speed;
            weaponDamage = damage;
            weaponRange = range;
            abilityDuration = duration;
            abilityCooldown = cooldown;
            abilityPower = power;
        }
    }
}
