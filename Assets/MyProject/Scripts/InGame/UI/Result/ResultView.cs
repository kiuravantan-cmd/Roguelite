using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace TPSRoguelite.UI
{
    public class ResultView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button returnTitleButton;

        /// <summary>
        /// ボタンが押されたときに、外部（Presenter）に知らせるためのイベント
        /// </summary>
        public event UnityAction OnRetryClickedAction;
        public event UnityAction OnReturnTitleClickedAction;

        private void Awake ()
        {
            // ボタンが押されたら、イベントを発火する
            if (returnTitleButton != null)
            {
                returnTitleButton.onClick.AddListener(() => OnReturnTitleClickedAction?.Invoke());
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(() => OnRetryClickedAction?.Invoke());
            }
        }

        /// <summary>
        /// Presenterから命令されて、文字を画面に表示するだけのメソッド
        /// </summary>
        public void SetResultText (string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
        }
    }
}
