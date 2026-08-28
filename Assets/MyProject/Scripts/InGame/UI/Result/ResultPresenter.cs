using TPSRoguelite.InGame.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSRoguelite.UI
{
    public class ResultPresenter : MonoBehaviour
    {
        private const string TITLE_SCENE_NAME = "TitleScene";
        private const string IN_GAME_SCENE_NAME = "InGameScene";

        [SerializeField] private ResultView resultView;
        private ResultModel resultModel;

        private void Start ()
        {
            if (resultView == null)
            {
                return;
            }

            // 1. Modelを生み出してデータを準備させる
            resultModel = new ResultModel();
            resultModel.Initialize();

            // 2. Modelのデータを使って、表示する文字を組み立てる
            string message = "";
            if (resultModel.IsClear)
            {
                message = $"GAME CLEAR!\n\n到達レベル: {resultModel.Level}";
            }
            else
            {
                int minutes = Mathf.FloorToInt(resultModel.SurvivedTime / 60F);
                int seconds = Mathf.FloorToInt(resultModel.SurvivedTime - minutes * 60);
                message = $"GAME OVER...\n\n生存時間: {minutes:00}:{seconds:00}\n到達レベル: {resultModel.Level}";
            }

            // 3. 組み立てた文字を、Viewに渡して表示させる
            resultView.SetResultText(message);

            // 4. Viewの「ボタンが押されたよイベント」を耳打ち（購読）して、遷移処理をセットする
            resultView.OnRetryClickedAction += RetryGame;
            resultView.OnReturnTitleClickedAction += ReturnToTitle;
        }

        private void RetryGame ()
        {
            // 次のプレイのために、古いGameManagerを破壊してリセットする
            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }

            // ゲームシーンを再読み込みして、最初からやり直す
            SceneManager.LoadScene(IN_GAME_SCENE_NAME);
        }

        private void ReturnToTitle()
        {
            // タイトル画面へ戻る
            SceneManager.LoadScene(TITLE_SCENE_NAME);
        }

        private void OnDestroy ()
        {
            // メモリのゴミを防ぐため、イベントの購読を解除しておく
            if (resultView != null)
            {
                resultView.OnRetryClickedAction -= RetryGame;
                resultView.OnReturnTitleClickedAction -= ReturnToTitle;
            }
        }
    }
}