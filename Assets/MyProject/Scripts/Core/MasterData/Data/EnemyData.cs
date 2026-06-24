using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MasterData
{
    /// <summary>
    /// CSVの1行分に相当するレコードデータ
    /// SOではなく通常のクラスにし、シリアライズ可能にする
    /// </summary>
    [Serializable]
    public class EnemyDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }
    }

    /// <summary>
    /// レコードのリストを保持する1つのSO
    /// CSVファイル名（EnemyData.csv）とこのクラス名が一致することでツールが自動認識する
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
    public class EnemyData : ScriptableObject, IMasterDataContainer<EnemyDataRecord>
    {
        [field: SerializeField] public List<EnemyDataRecord> Records { get; private set; } = new List<EnemyDataRecord>();
    }
}
