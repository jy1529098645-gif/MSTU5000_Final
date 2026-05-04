using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MissionUIController : MonoBehaviour
{
    [System.Serializable]
    public class MissionStep
    {
        [Header("Mission Text")]
        public string missionTitle;

        [TextArea(2, 5)]
        public string missionDescription;

        [TextArea(2, 5)]
        public string missionInstruction;

        [TextArea(1, 4)]
        public string objectiveText;

        public string nextMissionText;

        [Header("Objects")]
        public GameObject[] enableWhenMissionStarts;
        public GameObject[] disableWhenMissionStarts;

        [Header("Events")]
        public UnityEvent onMissionStarted;
        public UnityEvent onMissionConfirmed;
        public UnityEvent onMissionCompleted;
    }

    [Header("Panels")]
    public GameObject announcementPanel;
    public GameObject compactHUDPanel;

    [Header("Announcement Text")]
    public TMP_Text announcementTitleText;
    public TMP_Text announcementDescriptionText;
    public TMP_Text announcementInstructionText;
    public TMP_Text announcementObjectiveText;
    public Button confirmButton;

    [Header("Compact HUD Text")]
    public TMP_Text compactTitleText;
    public TMP_Text compactObjectiveText;
    public TMP_Text compactStatusText;
    public TMP_Text compactNextText;

    [Header("Mission Sequence")]
    public MissionStep[] missions;
    public int startingMissionIndex = 0;
    public bool showMissionOnStart = true;
    public bool autoAdvanceMission = true;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color successColor = Color.green;
    public Color warningColor = Color.yellow;

    [Header("Timing")]
    public float successDisplayDuration = 2f;

    [Header("Debug")]
    public int currentMissionIndex = -1;
    public bool missionConfirmed;
    public bool missionCompleted;
    public string currentMissionName;

    private Coroutine advanceRoutine;

    private void Awake()
    {
        HideAllUI();

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmAnnouncement);
            confirmButton.onClick.AddListener(ConfirmAnnouncement);
        }
    }

    private void Start()
    {
        if (showMissionOnStart)
        {
            StartMission(startingMissionIndex);
        }
    }

    public void StartMission(int missionIndex)
    {
        if (missions == null || missions.Length == 0)
        {
            Debug.LogWarning("MissionUIController: Missions list is empty.");
            HideAllUI();
            return;
        }

        if (missionIndex < 0 || missionIndex >= missions.Length)
        {
            Debug.LogWarning("MissionUIController: Invalid mission index: " + missionIndex);
            HideAllUI();
            return;
        }

        if (advanceRoutine != null)
        {
            StopCoroutine(advanceRoutine);
            advanceRoutine = null;
        }

        currentMissionIndex = missionIndex;
        missionConfirmed = false;
        missionCompleted = false;

        MissionStep mission = missions[currentMissionIndex];
        currentMissionName = mission.missionTitle;

        EnableObjects(mission.enableWhenMissionStarts);
        DisableObjects(mission.disableWhenMissionStarts);

        SetText(announcementTitleText, mission.missionTitle);
        SetText(announcementDescriptionText, mission.missionDescription);
        SetText(announcementInstructionText, mission.missionInstruction);
        SetText(announcementObjectiveText, "Objective:\n" + mission.objectiveText);

        SetText(compactTitleText, mission.missionTitle);
        SetText(compactObjectiveText, FormatObjectiveIncomplete(mission.objectiveText));
        SetText(compactStatusText, "Status: In Progress");
        SetText(compactNextText, "Next: " + mission.nextMissionText);

        SetCompactColor(normalColor);

        if (announcementPanel != null)
            announcementPanel.SetActive(true);

        if (compactHUDPanel != null)
            compactHUDPanel.SetActive(false);

        mission.onMissionStarted.Invoke();

        Debug.Log("Mission Started: " + mission.missionTitle);
    }

    public void ConfirmAnnouncement()
    {
        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Length)
            return;

        missionConfirmed = true;

        if (announcementPanel != null)
            announcementPanel.SetActive(false);

        if (compactHUDPanel != null)
            compactHUDPanel.SetActive(true);

        missions[currentMissionIndex].onMissionConfirmed.Invoke();

        Debug.Log("Mission Confirmed: " + missions[currentMissionIndex].missionTitle);
    }

    public void CompleteCurrentMission()
    {
        if (missionCompleted)
            return;

        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Length)
            return;

        missionCompleted = true;

        MissionStep mission = missions[currentMissionIndex];

        SetText(compactObjectiveText, FormatObjectiveComplete(mission.objectiveText));
        SetText(compactStatusText, "Status: Success");
        SetCompactColor(successColor);

        mission.onMissionCompleted.Invoke();

        Debug.Log("Mission Completed: " + mission.missionTitle);

        if (advanceRoutine != null)
            StopCoroutine(advanceRoutine);

        advanceRoutine = StartCoroutine(AdvanceMissionAfterDelay());
    }

    private IEnumerator AdvanceMissionAfterDelay()
    {
        yield return new WaitForSeconds(successDisplayDuration);

        if (!autoAdvanceMission)
            yield break;

        int nextMissionIndex = currentMissionIndex + 1;

        if (nextMissionIndex < missions.Length)
        {
            StartMission(nextMissionIndex);
        }
        else
        {
            ShowAllMissionsComplete();
        }
    }

    private void ShowAllMissionsComplete()
    {
        if (announcementPanel != null)
            announcementPanel.SetActive(false);

        if (compactHUDPanel != null)
            compactHUDPanel.SetActive(true);

        SetText(compactTitleText, "All Tasks Complete");
        SetText(compactObjectiveText, "✅ Safety sequence completed");
        SetText(compactStatusText, "Status: Complete");
        SetText(compactNextText, "Next: Wait for official instructions.");

        SetCompactColor(successColor);

        Debug.Log("All missions complete.");
    }

    public void HideAllUI()
    {
        if (announcementPanel != null)
            announcementPanel.SetActive(false);

        if (compactHUDPanel != null)
            compactHUDPanel.SetActive(false);
    }

    public void ShowAnnouncementPanel()
    {
        if (announcementPanel != null)
            announcementPanel.SetActive(true);

        if (compactHUDPanel != null)
            compactHUDPanel.SetActive(false);
    }

    public void ShowCompactHUDPanel()
    {
        if (announcementPanel != null)
            announcementPanel.SetActive(false);

        if (compactHUDPanel != null)
            compactHUDPanel.SetActive(true);
    }

    public void GoToNextMission()
    {
        StartMission(currentMissionIndex + 1);
    }

    public void RestartMissionSequence()
    {
        StartMission(startingMissionIndex);
    }

    public void MarkCurrentMissionSuccess()
    {
        CompleteCurrentMission();
    }

    private void EnableObjects(GameObject[] objects)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void DisableObjects(GameObject[] objects)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private string FormatObjectiveIncomplete(string objective)
    {
        if (string.IsNullOrEmpty(objective))
            return "⬜ Objective";

        string[] lines = objective.Split('\n');
        string result = "";

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                result += "⬜ " + line.Trim() + "\n";
        }

        return result.TrimEnd();
    }

    private string FormatObjectiveComplete(string objective)
    {
        if (string.IsNullOrEmpty(objective))
            return "✅ Objective";

        string[] lines = objective.Split('\n');
        string result = "";

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                result += "✅ " + line.Trim() + "\n";
        }

        return result.TrimEnd();
    }

    private void SetCompactColor(Color color)
    {
        if (compactTitleText != null)
            compactTitleText.color = color;

        if (compactObjectiveText != null)
            compactObjectiveText.color = color;

        if (compactStatusText != null)
            compactStatusText.color = color;

        if (compactNextText != null)
            compactNextText.color = color;
    }

    private void SetText(TMP_Text textObject, string value)
    {
        if (textObject != null)
            textObject.text = value;
    }
}