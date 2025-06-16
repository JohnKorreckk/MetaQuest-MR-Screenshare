using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LobbyScreenManager : MonoBehaviour
{
    [SerializeField] private Button joinMeetingButton;
    [SerializeField] private Button createMeetingButton;
    [SerializeField] private TMP_InputField meetingIdInput;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joinMeetingButton.onClick.AddListener(onJoinMeetingButtonPressed);
        createMeetingButton.onClick.AddListener(onCreateMeetingButtonPressed);
    }

    void OnDestroy()
    {
        joinMeetingButton.onClick.RemoveListener(onJoinMeetingButtonPressed);
        createMeetingButton.onClick.RemoveListener(onCreateMeetingButtonPressed);
    }

    void onCreateMeetingButtonPressed()
    {
        GUIManager.Instance.OnCreateMeetingRequested();
    }

    void onJoinMeetingButtonPressed()
    {
        // string meetingID = meetingIdInput.text.Trim();
        string meetingID = "Hello";
        GUIManager.Instance.OnJoinMeetingRequested(meetingID);
    }
}
