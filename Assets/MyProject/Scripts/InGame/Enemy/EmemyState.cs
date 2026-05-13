using MyProject.Scripts.InGame.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MyProject.Scripts.InGame.Enemy
{
    public class EmemyState : MonoBehaviour, IDamageable
    {
        private const int MAX_HP = 100;
        public int CurrentHp { get; private set; }

        private void Awake ()
        {
            CurrentHp = MAX_HP;
        }

        public void TakeDamage(int damageAmount)
        {
            // マイナスのダメージ（回復）を防ぐ
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHp -= damageAmount;
            Debug.Log($"敵に{damageAmount}のダメージ。残りHP{CurrentHp}");

            if (CurrentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("敵を倒しました。");
            Destroy(gameObject);
        }
    }
}
