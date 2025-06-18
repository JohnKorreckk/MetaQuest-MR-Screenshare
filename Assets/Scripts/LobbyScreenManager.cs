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
        UnityEngine.Debug.Log(System.String.Format("Create Button Pressed"));
        GUIManager.Instance.OnCreateMeetingRequested();
    }

    void onJoinMeetingButtonPressed()
    {
        UnityEngine.Debug.Log(System.String.Format("Join Button Pressed"));
        // string meetingID = meetingIdInput.text.Trim();
        string meetingID = "UYVwXEfG4t1tl02H4LeB";
        StartCoroutine(GUIManager.Instance.OnJoinMeetingRequested(meetingID));
    }
}
