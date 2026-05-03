using UnityEngine;

public class ProjectorSwitch : MonoBehaviour
{
    public Light projectorLight;
    public Renderer screenRenderer;
    public Material onMaterial;
    public Material offMaterial;

    private bool isOn = true;
    private Renderer switchRenderer;

    void Start()
    {
        switchRenderer = GetComponent<Renderer>();
        // URP 需要用 _BaseColor
        switchRenderer.material.SetColor("_BaseColor", Color.green);
    }

    public void ToggleProjector()
    {
        isOn = !isOn;

        if (projectorLight != null)
            projectorLight.enabled = isOn;

        if (screenRenderer != null)
            screenRenderer.material = isOn ? onMaterial : offMaterial;

        // URP 颜色切换
        switchRenderer.material.SetColor("_BaseColor", isOn ? Color.green : Color.red);

        Debug.Log("Projector is now: " + (isOn ? "On" : "Off"));
    }
}