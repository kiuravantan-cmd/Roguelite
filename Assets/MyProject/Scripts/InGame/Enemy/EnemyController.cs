using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        private Transform targetPlayer = null;

        private void Awake ()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
            else
            {
                Debug.LogError("Playerタグのついたオブジェクトが見つかりません！");
            }
        }

        private void Update ()
        {
            if (targetPlayer != null && navMeshAgent != null)
            {
                navMeshAgent.SetDestination(targetPlayer.position);
            }
        }
    }
}
