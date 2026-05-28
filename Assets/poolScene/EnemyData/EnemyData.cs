using PoolScene;
using UnityEngine;

namespace EnemyData
{
    /// <summary>
    /// 敵の基本性能、見た目、射撃間隔、ドロップ候補を定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(menuName = "Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        // 敵の移動速度。
        public float speed = 5f;

        // 敵の最大HP。
        public int hp = 1;

        // 撃破時に加算されるスコア。
        public int score = 10;

        // 敵の表示色。
        public Color color = Color.white;

        // 敵弾を撃つ間隔。
        public float shootInterval = 2f;

        // 撃破時に抽選されるドロップ候補。
        public DropEntry[] drops;
    }
}
