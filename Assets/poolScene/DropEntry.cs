namespace poolScene
{
    /// <summary>
    /// 敵撃破時にドロップするアイテム候補と抽選重みをまとめた設定クラス。
    /// </summary>
    [System.Serializable]
    public class DropEntry
    {
        // ドロップ候補のアイテムデータ。
        public global::ItemData itemData;

        // このアイテムが選ばれる確率に使う重み。
        public int weight;
    }
}
