using TPSRoguelite.InGame.Manager;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSRoguelite.UI {
    public class TitlePresenter : MonoBehaviour {
        private const string IN_GAME_SCENE = "InGameScene";

        [SerializeField] private TitleView titleView;
        private TitleModel titleModel;

        private void Start() {
            if (titleView == null) {
                return;
            }

            titleModel = new TitleModel();
            titleModel.Initialize();

            titleView.OnPlayAction += PlayGame;
            titleView.OnExitAction += ExitGame;
        }

        private void OnDestroy() {
            if (titleView != null) {
                titleView.OnPlayAction -= PlayGame;
                titleView.OnExitAction -= ExitGame;
            }

        }

        private void PlayGame() {
            if (GameManager.Instance != null) {
                Destroy(GameManager.Instance.gameObject);
            }

            SceneManager.LoadScene(IN_GAME_SCENE);
        }

        private void ExitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}