using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TPSRoguelite.UI
{
    public class TitleView : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;

        // ボタンが押されたことを外部（Presenter）に知らせるためのイベント
        public event UnityAction OnStartClickedAction;
        public event UnityAction OnExitClickedAction;

        private void Awake ()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => OnStartClickedAction?.Invoke());
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(() => OnExitClickedAction?.Invoke());
            }
        }
    }
}
