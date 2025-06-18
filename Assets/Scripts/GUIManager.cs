using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.WebRTC;
using Firebase.Firestore;
using System.Threading.Tasks;
using NUnit.Framework;

[FirestoreData]
public struct Call {
    [FirestoreProperty]
    public Dictionary<string, string> offer { get; set; }

    [FirestoreProperty]
    public Dictionary<string, string> answer { get; set; }
};

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

    [SerializeField] private GameObject LobbyScreen;
    [SerializeField] private GameObject RoomScreen;

    private string callPath = "calls/";

    private FirebaseFirestore firestore;
    Firebase.FirebaseApp app;

    [SerializeField] string offerDescription = string.Empty;
    [SerializeField] string answerDescription = string.Empty;

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

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;

            UnityEngine.Debug.LogError(System.String.Format("Entered Async Function"));


            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;

                UnityEngine.Debug.LogError(System.String.Format(
                    "HOUSTON WE HAVE AN APP"));
                // Firebase Unity SDK is not safe to use here.

                //StartCoroutine(DeferredInit());

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        UnityEngine.Debug.LogError(System.String.Format("Lobby Screen Shown"));

        ShowLobbyScreen();
        
        
    }

    //private IEnumerator DeferredInit()
    //{
    //    yield return null;
    //}

    public void ShowLobbyScreen()
    {
        LobbyScreen.SetActive(true);
        RoomScreen.SetActive(false);
    }

    public void ShowRoomScreen()
    {
        LobbyScreen.SetActive(false);
        RoomScreen.SetActive(true);
        UnityEngine.Debug.LogError(System.String.Format("Room Screen Shown"));
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

    public async void OnJoinMeetingRequested(string meetingID)
    {
        UnityEngine.Debug.LogError(System.String.Format("Join Meeting Requested"));
        try
        {
            var callDoc = firestore.Collection(callPath).Document(meetingID);
            var answerCandidates = callDoc.Collection("answerCandidates");
            var offerCandidates = callDoc.Collection("offerCandidates");

            UnityEngine.Debug.LogError(System.String.Format("Successfully called for candidates"));
        }

        catch (Exception ex) {
            UnityEngine.Debug.LogError(System.String.Format("FAILED CALLING CANDIDATES: "));
            UnityEngine.Debug.LogError(ex);
        }

        Call? callData = await GetData(meetingID);

        if (callData != null)
        {
            ShowRoomScreen();
        }
        else
        {
            Debug.LogError("Call data invalid or missing 'sdp'");
        }
    }

    public async Task<Call?> GetData(string meetingID)
    {
        string request = callPath + meetingID;

        try
        {
            var snapshot = await firestore.Document(request).GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogError("Document does not exist: " + request);
                return null;
            }

            Call callData = snapshot.ConvertTo<Call>();

            try
            {
                offerDescription = callData.offer["sdp"];
            }
            catch (Exception ex)
            {
                Debug.LogError("Error accessing offer data: " + ex.Message);
            }

            return callData;
        }
        catch (Exception e)
        {
            Debug.LogError("Error fetching document: " + e.Message);
            return null;
        }
    }
}
