using UnityEngine;

/// <summary>
/// オブジェクトプールを使わず、短い間隔で敵を大量生成する比較用スポナー。
/// </summary>
public class Spawner3D_NoPool : MonoBehaviour
{
    // 毎回Instantiateする敵Prefab。
    public GameObject enemyPrefab;

    // 次の生成までの経過時間。
    float timer;

    // 0.01秒ごとに敵を10体生成し、プールなし生成の負荷を確認できるようにする。
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.01f)
        {
            for (int i = 0; i < 10; i++)
            {
                float x = Random.Range(-5f, 5f);
                Vector3 pos = new Vector3(x, 0, 15);

                Instantiate(enemyPrefab, pos, Quaternion.identity);
            }

            timer = 0f;
        }
    }
}
