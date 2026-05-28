using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// 敵プールから敵を取り出し、一定間隔でフィールド奥に出現させるクラス。
    /// </summary>
    public class Spawner3D : MonoBehaviour
    {
        // 敵を取得するためのプール参照。
        public EnemyPool enemyPool;

        // 次のスポーンまでの経過時間。
        private float _timer;

        // 敵を出現させる間隔。時間経過で短くなる。
        public float interval = 1f;

        // ゲーム中のみスポーン間隔を短くしながら敵を生成する。
        private void Update()
        {
            if (GameManager3D.Instance.isGameOver) return;
            _timer += Time.deltaTime;

            // 難易度上昇
            interval -= Time.deltaTime * 0.01f;
            interval = Mathf.Clamp(interval, 0.1f, 1f);

            if (!(_timer > interval)) return;
            SpawnEnemy();
            _timer = 0f;
        }

        // 敵プールからランダムな敵を取得し、ランダムなX座標に配置して有効化する。
        // ReSharper disable Unity.PerformanceAnalysis
        private static void SpawnEnemy()
        {
            var enemy =
                EnemyPool.Instance.GetRandomEnemy();

            if (enemy == null) return;
            var x = Random.Range(-5f, 5f);

            enemy.transform.position =
                new Vector3(x, 0, 15);

            enemy.SetActive(true);
        }
    }
}
