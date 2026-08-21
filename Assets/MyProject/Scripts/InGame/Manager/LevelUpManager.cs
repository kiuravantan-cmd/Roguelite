using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using TPSRoguelite.InGame.Player;
using Core.MasterData;

namespace TPSRoguelite.InGame.Manager
{
    // ボタンとテキストをセットで管理するためのクラス
    [System.Serializable]
    public class SkillButtonUI
    {
        public Button button;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
    }

    public class LevelUpManager : MonoBehaviour
    {
        public static LevelUpManager Instance { get; private set; }

        [Header("UI設定")]
        [SerializeField] private GameObject skillSelectPanel;
        [SerializeField] private SkillButtonUI[] skillButtons = new SkillButtonUI[3];

        private PlayerInputActions inputActions;
        private PlayerController playerController;

        private void Awake ()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start ()
        {
            Time.timeScale = 1f;
            if (skillSelectPanel != null)
            {
                skillSelectPanel.SetActive(false);
            }
        }

        /// <summary>
        /// レベルアップ時の処理を行うメソッド
        /// </summary>
        public void OnLevelUp(PlayerInputActions currentInput, PlayerController player)
        {
            inputActions = currentInput;
            playerController = player;

            // スキルをランダムに3つ選択してUIに表示する
            var allSkills = MasterDataAccessor.Instance.GetAll<SkillDataRecord>();
            var choiceSkills = allSkills.OrderBy(v => System.Guid.NewGuid()).Take(3).ToArray();

            // UIにスキル情報を表示する
            for (int i = 0; i < 3; i++)
            {
                var skill = choiceSkills[i];
                var ui = skillButtons[i];

                ui.nameText.text = skill.SkillName;
                ui.descText.text = skill.Description;

                // 古いリスナーを削除してから新しいリスナーを追加する
                ui.button.onClick.RemoveAllListeners();
                ui.button.onClick.AddListener(() => OnSkillSelected(skill));
            }

            // 画面を表示して時間を止める
            if (skillSelectPanel != null)
            {
                skillSelectPanel.gameObject.SetActive(true);
            }

            Time.timeScale = 0f;

            // マウスカーソルを解放し、ActionMapを切り替える
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (inputActions != null)
            {
                inputActions.Player.Disable();
            }
        }

        /// <summary>
        /// スキルが選択されたときの処理を行うメソッド
        /// </summary>
        private void OnSkillSelected(SkillDataRecord selectedSkill)
        {
            // スキルをプレイヤーに付与する
            if (playerController != null)
            {
                playerController.ApplySkill(selectedSkill);
            }

            // 画面を非表示にして時間を再開する
            if (skillSelectPanel != null)
            {
                skillSelectPanel.gameObject.SetActive(false);
            }

            Time.timeScale = 1f;

            // マウスカーソルをロックし、ActionMapを切り替える
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (inputActions != null)
            {
                inputActions.Player.Enable();
            }
        }
    }
}
