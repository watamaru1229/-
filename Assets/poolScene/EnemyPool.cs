using System.Collections.Generic;
using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// 敵Prefabを種類ごとに再利用し、重み付き抽選で出現させる敵プール。
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        // Spawnerなどから敵を取得するための共有インスタンス。
        public static EnemyPool Instance;

        // 出現候補の敵データ、Prefab、出現重みの一覧。
        public EnemyEntry[] enemies;

        // 敵の種類ごとに初期生成する数。
        public int poolSize = 10;

        // 生成済みの全敵エントリ。
        private readonly List<PooledEnemy> _pool = new List<PooledEnemy>(10);

        // 敵データごとに使用可能な敵を管理するキュー。
        private readonly Dictionary<EnemyData.EnemyData, Queue<PooledEnemy>> _availableByData =
            new Dictionary<EnemyData.EnemyData, Queue<PooledEnemy>>(8);

        // Enemy3Dコンポーネントから対応するプールエントリを引くための辞書。
        private readonly Dictionary<Enemy3D, PooledEnemy> _poolByEnemy =
            new Dictionary<Enemy3D, PooledEnemy>(32);

        // 全敵の出現重みの合計値。
        private int _totalSpawnWeight;

        // プール内で1体の敵オブジェクトと状態を保持する内部クラス。
        private class PooledEnemy
        {
            // 敵のGameObject。
            public GameObject gameObject;

            // 敵の動作コンポーネント。
            public Enemy3D enemy3D;

            // この敵に対応する敵データ。
            public EnemyData.EnemyData data;

            // 現在プール内で使用可能かどうか。
            public bool isAvailable;
        }

        // 共有インスタンスを登録する。
        private void Awake()
        {
            Instance = this;
        }

        // 敵ごとの初期プールを作成し、出現重みの合計を計算する。
        private void Start()
        {
            var enemyCount = enemies?.Length ?? 0;
            var expectedCapacity = enemyCount * poolSize;

            if (_pool.Capacity < expectedCapacity)
            {
                _pool.Capacity = expectedCapacity;
            }

            for (var enemyIndex = 0; enemyIndex < enemyCount; enemyIndex++)
            {
                if (enemies != null)
                {
                    var enemy = enemies[enemyIndex];
                    _totalSpawnWeight += enemy.spawnWeight;

                    for (var i = 0; i < poolSize; i++)
                    {
                        var pooledEnemy = CreatePooledEnemy(enemy);
                        ReturnEnemy(pooledEnemy);
                    }
                }
            }
        }

        // 指定された敵データに対応する使用可能な敵を取得する。
        // data: 取得したい敵データ。
        // 戻り値: 利用する敵GameObject。取得できない場合はnull。
        private GameObject GetEnemy(EnemyData.EnemyData data)
        {
            if (data == null)
            {
                return null;
            }

            var available = GetAvailableQueue(data);

            while (available.Count > 0)
            {
                var pooledEnemy = available.Dequeue();
                pooledEnemy.isAvailable = false;

                if (pooledEnemy.gameObject == null)
                {
                    continue;
                }

                pooledEnemy.enemy3D.Initialize(data);

                return pooledEnemy.gameObject;
            }

            var enemyCount = enemies?.Length ?? 0;

            for (var i = 0; i < enemyCount; i++)
            {
                if (enemies != null)
                {
                    var entry = enemies[i];

                    if (entry.data != data)
                    {
                        continue;
                    }

                    var pooledEnemy = CreatePooledEnemy(entry);
                    pooledEnemy.enemy3D.Initialize(data);
                    return pooledEnemy.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// 出現重みに従ってランダムに敵を1体取得する。
        /// </summary>
        /// <returns>出現させる敵GameObject。候補がない場合はnull。</returns>
        public GameObject GetRandomEnemy()
        {
            if (enemies == null ||
                enemies.Length == 0 ||
                _totalSpawnWeight <= 0)
            {
                return null;
            }

            var randomValue =
                Random.Range(0, _totalSpawnWeight);

            var currentWeight = 0;

            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                currentWeight += enemy.spawnWeight;

                if (randomValue < currentWeight)
                {
                    return GetEnemy(enemy.data);
                }
            }

            return null;
        }

        /// <summary>
        /// Enemy3Dから対応するプールエントリを探して返却する。
        /// </summary>
        /// <param name="enemy3D">返却する敵コンポーネント。</param>
        public void ReturnEnemy(Enemy3D enemy3D)
        {
            if (enemy3D == null ||
                !_poolByEnemy.TryGetValue(enemy3D, out var pooledEnemy))
            {
                return;
            }

            ReturnEnemy(pooledEnemy);
        }

        // 敵エントリを使用可能状態にして、種類ごとのキューへ戻す。
        // pooledEnemy: 返却する敵エントリ。
        private void ReturnEnemy(PooledEnemy pooledEnemy)
        {
            if (pooledEnemy == null ||
                pooledEnemy.isAvailable ||
                pooledEnemy.data == null)
            {
                return;
            }

            pooledEnemy.isAvailable = true;
            GetAvailableQueue(pooledEnemy.data).Enqueue(pooledEnemy);
        }

        // EnemyEntryから敵オブジェクトを生成し、プール管理用の情報を登録する。
        // entry: 敵のPrefabとデータを持つ設定。
        // 戻り値: 作成したプール用敵エントリ。
        private PooledEnemy CreatePooledEnemy(EnemyEntry entry)
        {
            var obj =
                Instantiate(entry.prefab);

            obj.SetActive(false);

            var enemy3D =
                obj.GetComponent<Enemy3D>();

            var pooledEnemy = new PooledEnemy
            {
                gameObject = obj,
                enemy3D = enemy3D,
                data = entry.data,
                isAvailable = false
            };

            enemy3D.InitializePool(this);
            enemy3D.Initialize(entry.data);

            _pool.Add(pooledEnemy);
            _poolByEnemy.Add(enemy3D, pooledEnemy);

            return pooledEnemy;
        }

        // 敵データに対応する使用可能キューを取得し、なければ新しく作る。
        // data: キューを取得する敵データ。
        // 戻り値: 使用可能な敵を入れるキュー。
        private Queue<PooledEnemy> GetAvailableQueue(EnemyData.EnemyData data)
        {
            if (_availableByData.TryGetValue(data, out var available))
            {
                return available;
            }

            available = new Queue<PooledEnemy>(poolSize);
            _availableByData.Add(data, available);
            return available;
        }
    }
}
