using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TPSRoguelite.InGame.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }
    }
}
