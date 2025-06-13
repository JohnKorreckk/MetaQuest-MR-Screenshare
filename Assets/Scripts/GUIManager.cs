using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Threading;
using Unity.WebRTC;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

    [SerializeField] private GameObject LobbyScreen;
    [SerializeField] private GameObject RoomScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ShowLobbyScreen();
    }

    public void ShowLobbyScreen()
    {
        LobbyScreen.SetActive(true);
        RoomScreen.SetActive(false);
    }

    public void ShowRoomScreen()
    {
        LobbyScreen.SetActive(false);
        RoomScreen.SetActive(true);
    }

    public void ToggleCanvases()
    {
        bool isLobbyScreenActive = LobbyScreen.activeSelf;

        LobbyScreen.SetActive(!isLobbyScreenActive);
        RoomScreen.SetActive(isLobbyScreenActive);
    }

    public void OnCreateMeetingRequested()
    {
        // Handle create logic
        ShowRoomScreen();
    }

    public void OnJoinMeetingRequested(string meetingID)
    {
        // Handle join logic
        ShowRoomScreen();
    }
}

