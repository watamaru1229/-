using PoolScene.ItemData.WeaponData;
using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// フィールドに落ちているアイテムを表し、プレイヤー接触時にインベントリへ追加するクラス。
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        // このピックアップが持っているアイテムデータ。
        private global::ItemData _itemData;

        /// <summary>
        /// ドロップ生成時に、拾えるアイテムの内容を設定する。
        /// </summary>
        /// <param name="item">このオブジェクトで取得できるアイテム。</param>
        public void Initialize(global::ItemData item)
        {
            _itemData = item;
        }

        // プレイヤーが触れたら、武器ならランダム性能を作って装備し、通常アイテムならそのまま追加する。
        // other: 接触したCollider。
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var player =
                other.GetComponent<Player3D>();

            if (_itemData is WeaponData weapon)
            {
                var weaponInstance =
                    WeaponInstance.CreateRandom(weapon);

                Inventory.Instance.AddWeapon(weaponInstance);

                if (player != null)
                {
                    player.EquipWeapon(weaponInstance);
                }
            }
            else
            {
                Inventory.Instance.AddItem(_itemData);
            }

            Destroy(gameObject);
        }
    }
}
