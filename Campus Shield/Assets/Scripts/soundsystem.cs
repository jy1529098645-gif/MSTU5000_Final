using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SoundSystem : MonoBehaviour
{
    public AudioClip gunshot;
    public AudioClip shouting;
    public AudioClip broadcast;

    [Header("3D Audio Settings")]
    public bool use3D = true;
    [Tooltip("Max range at which the sound is audible. Volume interpolates from full at source to 0 at this distance.")]
    [Range(1f, 200f)]
    public float audibleRange = 50f;
    [Tooltip("Controls falloff curve: 1 = linear, >1 = faster falloff, <1 = slower falloff.")]
    [Range(0.1f, 4f)]
    public float falloffExponent = 1f;
    [Header("Gizmo Visualization")]
    [Tooltip("Show colored falloff gizmo in the Scene view when selected.")]
    public bool showFalloffGizmo = true;
    [Tooltip("Number of concentric rings used to visualize falloff (editor only).")]
    [Range(3, 24)]
    public int falloffSteps = 8;
    [Tooltip("Color at the source (full volume).")]
    public Color innerGizmoColor = new Color(0f, 1f, 0f, 0.25f);
    [Tooltip("Color at the audible range (zero volume).")]
    public Color outerGizmoColor = new Color(0f, 0.6f, 1f, 0.15f);

    [Header("Runtime UI")]
    [Tooltip("Show a simple UI overlay (OnGUI) with distance and volume info at runtime.")]
    public bool showRuntimeUI = true;
    [Tooltip("Screen X,Y offset in pixels for the overlay's top-left corner.")]
    public Vector2 uiOffset = new Vector2(10f, 10f);
    [Tooltip("Width of the overlay box in pixels.")]
    public int uiWidth = 240;
    [Tooltip("Background color for the overlay box.")]
    public Color uiBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [Tooltip("Fill color for the volume bar.")]
    public Color uiFillColor = new Color(0f, 0.8f, 0f, 1f);

    [Header("Volume")]
    [Tooltip("Master volume multiplier (0 = silent, 1 = original volume).")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Header("Playback")]
    public bool allowOverlap = true; // use PlayOneShot to allow overlapping clips
    public float broadcastDuration = 20f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = use3D ? 1f : 0f; // 1 = fully 3D, 0 = 2D

        StartCoroutine(PlaySoundsInOrder());
    }

    void Update()
    {
        // If a looping clip (e.g. broadcast) is playing on the source, update its volume each frame
        if (audioSource != null && audioSource.isPlaying && audioSource.loop)
        {
            audioSource.volume = VolumeForPosition(transform.position) * masterVolume;
        }
    }

    Transform GetListenerTransform()
    {
        var listener = FindObjectOfType<AudioListener>();
        if (listener != null) return listener.transform;
        if (Camera.main != null) return Camera.main.transform;
        return null;
    }

    float VolumeForPosition(Vector3 sourcePosition)
    {
        if (!use3D) return 1f;
        var listenerT = GetListenerTransform();
        if (listenerT == null) return 1f;
        float dist = Vector3.Distance(sourcePosition, listenerT.position);
        if (audibleRange <= 0.0001f) return 1f;
        float t = Mathf.Clamp01(1f - (dist / audibleRange));
        return Mathf.Pow(t, falloffExponent);
    }

    void OnDrawGizmosSelected()
    {
        if (!showFalloffGizmo)
        {
            Gizmos.color = outerGizmoColor;
            Gizmos.DrawWireSphere(transform.position, audibleRange);
        }
        else
        {
#if UNITY_EDITOR
            // Draw concentric rings/spheres to visualize falloff with color interpolation
            for (int i = 0; i < falloffSteps; i++)
            {
                float t = (float)i / (float)(falloffSteps - 1); // 0..1
                float r = Mathf.Lerp(0f, audibleRange, t);
                Color c = Color.Lerp(innerGizmoColor, outerGizmoColor, t);
                Handles.color = c;
                Handles.DrawWireDisc(transform.position, Vector3.up, r);
                Handles.DrawWireDisc(transform.position, Vector3.right, r);
                Handles.DrawWireDisc(transform.position, Vector3.forward, r);
            }

            // Label at outer edge
            Handles.color = new Color(outerGizmoColor.r, outerGizmoColor.g, outerGizmoColor.b, 1f);
            Handles.Label(transform.position + Vector3.up * audibleRange, $"Audible Range: {audibleRange:F1}\nFalloff: {falloffExponent:F2}");
#else
            Gizmos.color = outerGizmoColor;
            Gizmos.DrawWireSphere(transform.position, audibleRange);
#endif
        }
    }

    void OnGUI()
    {
        if (!showRuntimeUI) return;
        if (!Application.isPlaying) return;

        Transform listenerT = GetListenerTransform();
        float dist = 0f;
        if (listenerT != null) dist = Vector3.Distance(transform.position, listenerT.position);
        float vol = VolumeForPosition(transform.position) * masterVolume;

        // Box
        int boxH = 62;
        Rect box = new Rect(uiOffset.x, uiOffset.y, uiWidth, boxH);

        // Background
        Color prevColor = GUI.color;
        GUI.color = uiBackgroundColor;
        GUI.Box(box, GUIContent.none);

        GUI.color = Color.white;
        GUI.Label(new Rect(box.x + 8, box.y + 6, box.width - 16, 18), $"Source: {gameObject.name}");
        GUI.Label(new Rect(box.x + 8, box.y + 24, box.width - 16, 18), $"Distance: {dist:F1} m   Volume: {vol * 100f:F0}%");

        // Volume bar
        Rect barBg = new Rect(box.x + 8, box.y + 42, box.width - 16, 12);
        GUI.Box(barBg, GUIContent.none);
        Rect barFill = new Rect(barBg.x + 1, barBg.y + 1, Mathf.Clamp((barBg.width - 2) * vol, 0f, barBg.width - 2), barBg.height - 2);
        GUI.color = uiFillColor;
        GUI.DrawTexture(barFill, Texture2D.whiteTexture);

        GUI.color = prevColor;
        // Master volume slider (runtime)
        Rect sliderRect = new Rect(box.x + 8, box.y + box.height + 6, box.width - 16, 18);
        float newMaster = GUI.HorizontalSlider(sliderRect, masterVolume, 0f, 1f);
        if (!Mathf.Approximately(newMaster, masterVolume)) masterVolume = newMaster;
    }

    IEnumerator PlaySoundsInOrder()
    {
        // 等待开始
        yield return new WaitForSeconds(7f);

        // 播放枪声（允许重叠时使用 PlayOneShot）
        float vol = VolumeForPosition(transform.position) * masterVolume;
        if (allowOverlap)
            audioSource.PlayOneShot(gunshot, vol);
        else
        {
            audioSource.volume = vol;
            audioSource.clip = gunshot;
            audioSource.Play();
        }
        Debug.Log("Playing: Gunshot");

        // 等10秒
        yield return new WaitForSeconds(10f);

        // 第一遍喊叫声
        vol = VolumeForPosition(transform.position) * masterVolume;
        if (allowOverlap)
            audioSource.PlayOneShot(shouting, vol);
        else
        {
            audioSource.volume = vol;
            audioSource.clip = shouting;
            audioSource.Play();
        }
        Debug.Log("Playing: Shouting 1/2");
        yield return new WaitForSeconds(shouting.length);

        // 第二遍喊叫声
        vol = VolumeForPosition(transform.position) * masterVolume;
        if (allowOverlap)
            audioSource.PlayOneShot(shouting, vol);
        else
        {
            audioSource.volume = vol;
            audioSource.clip = shouting;
            audioSource.Play();
        }
        Debug.Log("Playing: Shouting 2/2");

        // 第二遍播放3秒后播放广播
        yield return new WaitForSeconds(3f);

        // 播放广播声（使用同一 AudioSource，设为 loop）
        audioSource.clip = broadcast;
        audioSource.loop = true;
        audioSource.volume = VolumeForPosition(transform.position) * masterVolume;
        audioSource.Play();
        Debug.Log("Playing: Broadcast");

        // 广播播放指定时长后停止
        yield return new WaitForSeconds(broadcastDuration);
        audioSource.Stop();
        audioSource.loop = false;
        Debug.Log("Broadcast finished!");
    }
}