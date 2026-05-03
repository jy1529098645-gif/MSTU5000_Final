using UnityEngine;
using System.Collections;

public class SoundSystem : MonoBehaviour
{
    public AudioClip gunshot;
    public AudioClip shouting;
    public AudioClip broadcast;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlaySoundsInOrder());
    }

    IEnumerator PlaySoundsInOrder()
    {
        // 游戏开始等10秒
        yield return new WaitForSeconds(7f);

        // 播放枪声
        audioSource.clip = gunshot;
        audioSource.Play();
        Debug.Log("Playing: Gunshot");

        // 等10秒
        yield return new WaitForSeconds(10f);

        // 第一遍喊叫声
        audioSource.clip = shouting;
        audioSource.Play();
        Debug.Log("Playing: Shouting 1/2");
        yield return new WaitForSeconds(shouting.length);

        // 第二遍喊叫声
        audioSource.clip = shouting;
        audioSource.Play();
        Debug.Log("Playing: Shouting 2/2");

        // 第二遍播放3秒后播放广播
        yield return new WaitForSeconds(3f);

        // 播放广播声
        audioSource.clip = broadcast;
        audioSource.Play();
        Debug.Log("Playing: Broadcast");

        // 广播播放20秒后停止
        yield return new WaitForSeconds(20f);
        audioSource.Stop();
        Debug.Log("Broadcast finished!");
    }
}