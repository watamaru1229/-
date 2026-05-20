using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace poolScene
{
    /// <summary>
    /// 3Dゲーム全体の進行状態、スコア表示、HP表示、ゲームオーバー処理を管理するクラス。
    /// </summary>
    public class GameManager3D : MonoBehaviour
    {
        // 他のクラスからゲーム状態を参照するための共有インスタンス。
        public static GameManager3D Instance;

        // 現在のスコア。
        public int score;

        // ゲームオーバー中かどうか。
        public bool isGameOver;

        // スコアを表示するUIテキスト。
        public TextMeshProUGUI scoreText;

        // プレイヤーHPを表示するUIテキスト。
        public TMP_Text hpText;

        // ゲームオーバー時に表示するテキストオブジェクト。
        public GameObject gameOverText;

        // リトライ用ボタンのオブジェクト。
        public GameObject retryButton;

        // シーン内のGameManagerを共有インスタンスとして登録する。
        void Awake()
        {
            Instance = this;
        }

        // ゲームオーバー中にRキーが押されたら現在のシーンを再読み込みする。
        private void Update()
        {
            if (isGameOver &&
                Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }

        /// <summary>
        /// スコアを加算し、画面表示を更新する。
        /// </summary>
        /// <param name="value">加算するスコア量。</param>
        public void AddScore(int value)
        {
            score += value;
            scoreText.text = "Score: " + score;
        }

        /// <summary>
        /// ゲームオーバー状態に切り替え、関連UIを表示する。
        /// </summary>
        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;

            gameOverText.SetActive(true);
            retryButton.SetActive(true);
        }

        /// <summary>
        /// 現在プレイ中のシーンを再読み込みしてゲームをやり直す。
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// プレイヤーHPの表示を更新する。
        /// </summary>
        /// <param name="hp">表示する現在HP。</param>
        public void UpdateHp(int hp)
        {
            hpText.text = "HP : " + hp;
        }
    }
}
