using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TPSRoguelite.UI
{
	public class TitleView : MonoBehaviour
	{
		[SerializeField] private Button playButton;
		[SerializeField] private Button exitButton;

		public event UnityAction OnPlayAction;
		public event UnityAction OnExitAction;

        private void Awake() {
            if (playButton != null) {
				playButton.onClick.AddListener(() => OnPlayAction?.Invoke());
			}

			if (exitButton != null) {
				exitButton.onClick.AddListener(() => OnExitAction?.Invoke());
			}
        }

        private void OnDestroy() {
            if (playButton != null) {
				playButton.onClick.RemoveAllListeners();
			}

			if (exitButton != null) {
				exitButton.onClick.RemoveAllListeners();
			}
        }
    }
}