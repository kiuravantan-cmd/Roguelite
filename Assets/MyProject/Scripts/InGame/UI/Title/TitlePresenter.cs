using TPSRoguelite.InGame.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSRoguelite.UI
{
    public class TitlePresenter : MonoBehaviour
    {
        // ※自分のメインシーンの名前に合わせる
        private const string IN_GAME_SCENE_NAME = "InGameScene";
        [SerializeField] private TitleView titleView;
        private TitleModel titleModel;

        private void Start ()
        {
            if (titleView == null)
            {
                return;
            }

            titleModel = new TitleModel();
            titleModel.Initialize();

            // Viewの「ボタンが押されたよイベント」を耳打ち（購読）して、遷移処理をセットする
            titleView.OnStartClickedAction += GoToMainGame;
            titleView.OnExitClickedAction += ExitGame;
        }

        private void GoToMainGame()
        {
            // 次のプレイのために、古いGameManagerを破壊してリセットする
            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }

            SceneManager.LoadScene(IN_GAME_SCENE_NAME);
        }

        private void ExitGame ()
        {
            // アプリケーションを終了する
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnApplicationQuit()
        {
            // アプリケーション終了時に、GameManagerを破壊してリセットする
            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }
        }

        private void OnDestroy ()
        {
            if (titleView != null)
            {
                titleView.OnStartClickedAction -= GoToMainGame;
                titleView.OnExitClickedAction -= ExitGame;
            }
        }
    }
}
