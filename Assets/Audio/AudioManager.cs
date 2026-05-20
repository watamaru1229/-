using UnityEngine;

/// <summary>
/// 効果音用AudioSourceを共有し、ゲーム内のSE再生をまとめて扱うクラス。
/// </summary>
public class AudioManager : MonoBehaviour
{
    // 他のクラスからSEを再生するための共有インスタンス。
    public static AudioManager Instance;

    // 効果音を再生するAudioSource。
    public AudioSource seSource;

    // 最初に見つかったAudioManagerを共有インスタンスとして登録する。
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 指定された効果音を一度だけ再生する。
    /// </summary>
    /// <param name="clip">再生するAudioClip。</param>
    public void PlaySe(AudioClip clip)
    {
        seSource.PlayOneShot(clip);
    }
}
