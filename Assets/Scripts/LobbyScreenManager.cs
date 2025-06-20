using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

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
        string meetingID = "J5S2yyKJN6F91o6Ki7bt";
        StartCoroutine(GUIManager.Instance.OnJoinMeetingRequested(meetingID));
    }
}