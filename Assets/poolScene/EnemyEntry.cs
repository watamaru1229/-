using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// 敵プールで使う、敵データ・Prefab・出現重みをまとめた設定クラス。
    /// </summary>
    [System.Serializable]
    public class EnemyEntry
    {
        // 敵のHP、速度、スコアなどを持つデータ。
        public EnemyData.EnemyData data;

        // 生成する敵Prefab。
        public GameObject prefab;

        // ランダム出現時に選ばれやすさを決める重み。
        public int spawnWeight = 1;
    }
}
