using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MasterData
{
    [Serializable]
    public class SkillDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string SkillName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }

        /// <summary>
        /// どのステータスを上げるか（SkillTypeの数字）
        /// </summary>
        [field: SerializeField] public int SkillType { get; private set; }

        /// <summary>
        /// どれくらい上げるか（例：0.1 なら 10%アップ、5 なら 5発アップ）
        /// </summary>
        [field: SerializeField] public float Value { get; private set; }
    }

    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData")]
    public class SkillData : ScriptableObject, IMasterDataContainer<SkillDataRecord>
    {
        [field: SerializeField] public List<SkillDataRecord> Records { get; private set; }
    }
}
