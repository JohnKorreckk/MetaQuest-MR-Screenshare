using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Meta.XR.MRUtilityKit.SceneDecorator;
using NUnit.Framework;
using PassthroughCameraSamples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

[FirestoreData]
public struct Call {
    [FirestoreProperty]
    public Dictionary<string, string> offer { get; set; }

    [FirestoreProperty]
    public Dictionary<string, string> answer { get; set; }
};

public static class TaskExtensions
{
    public static IEnumerator AsYieldInstruction(this Task task)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            Debug.LogError(task.Exception?.Flatten().InnerException?.Message);
    }
}


public class GUIManager : MonoBehaviour
{

    public GameObject previewCube;

    // Globally accessible GUI object
    public static GUIManager Instance { get; private set; }

    // Screen fields
    [SerializeField] private GameObject LobbyScreen;
    [SerializeField] private GameObject RoomScreen;

    // Firebase and Firestore instantiations
    private FirebaseFirestore firestore;
    Firebase.FirebaseApp app;

    // Ice Servers
    RTCConfiguration config = new RTCConfiguration
    {
        iceServers = new RTCIceServer[]
        {
            new RTCIceServer
            {
                urls = new string[]
                {
                    "stun:stun1.l.google.com:19302",
                    "stun:stun2.l.google.com:19302"
                }
            }
        },
        iceCandidatePoolSize = 10
    };

    // Global state
    RTCPeerConnection pc;
    MediaStream localStream = null;
    MediaStream remoteStream = null;

    // SDPs
    [SerializeField] Dictionary<string, string> offerDescription;
    [SerializeField] Dictionary<string, string> answerDescription;

    // Global paths
    private string callPath = "calls/";

    // Candidate HashSet
    private HashSet<string> processedOfferCandidateIds = new HashSet<string>();

    // Initialize Passthrough Camera video feed
    WebCamTextureManager webCamTextureManager = null;
    WebCamTexture webCamTexture = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        StartCoroutine(WebRTC.Update());
    }

    private IEnumerator Start()
    {

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {

            // Initiate Firebase
            var dependencyStatus = task.Result;

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to FirebaseApp
                app = Firebase.FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;

                // StartCoroutine(DeferredInit());
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });


        // Set the peer connection
        pc = new RTCPeerConnection(ref config);
        UnityEngine.Debug.LogError(System.String.Format("Successfully set peer connection"));





        UnityEngine.Debug.LogError(System.String.Format("Searching/set WebCamTexture"));

        do
        {
            yield return null;

            if (webCamTextureManager == null)
            {
                webCamTextureManager = FindFirstObjectByType<WebCamTextureManager>();
            }
            else
            {
                webCamTexture = webCamTextureManager.WebCamTexture;
            }
        } while (webCamTexture == null);

        // Apply local stream to Cube object
        var renderer = previewCube.GetComponent<Renderer>();
        renderer.material.mainTexture = webCamTexture;

        // Add passthrough video to localStream
        localStream = new MediaStream();
        localStream.AddTrack(new VideoStreamTrack(webCamTexture));

        foreach (var track in localStream.GetTracks())
        {
            pc.AddTrack(track, localStream);
            UnityEngine.Debug.LogError(System.String.Format("Successfully added track to peerconnection"));
        }

        // remoteStream = new MediaStream();

        ShowLobbyScreen();
    }

    private IEnumerator DeferredInit()
    {
        yield return null;
        StartCoroutine(OnJoinMeetingRequested("00LNgq74XUE03BfklbWK"));
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

    public void ListenForOfferCandidates(CollectionReference offerCandidates)
    {
        offerCandidates.Listen(snapshot =>
        {
            foreach (var doc in snapshot.Documents)
            {
                if (!processedOfferCandidateIds.Contains(doc.Id))
                {
                    processedOfferCandidateIds.Add(doc.Id);

                    var data = doc.ToDictionary();

                    if (data.TryGetValue("candidate", out object candidateObj) &&
                        data.TryGetValue("sdpMid", out object sdpMidObj) &&
                        data.TryGetValue("sdpMLineIndex", out object sdpMLineIndexObj))
                    {
                        string candidateStr = candidateObj as string;
                        string sdpMidStr = sdpMidObj as string;
                        int sdpMLineIndex = Convert.ToInt32(sdpMLineIndexObj);

                        if (!string.IsNullOrEmpty(candidateStr) && !string.IsNullOrEmpty(sdpMidStr))
                        {
                            var iceCandidateInit = new RTCIceCandidateInit
                            {
                                candidate = candidateStr,
                                sdpMid = sdpMidStr,
                                sdpMLineIndex = sdpMLineIndex
                            };

                            var iceCandidate = new RTCIceCandidate(iceCandidateInit);
                            pc.AddIceCandidate(iceCandidate);

                            Debug.Log("Added new ICE candidate from offerCandidates collection.");
                        }
                    }
                }
            }
        });
    }


    public IEnumerator OnJoinMeetingRequested(string meetingID)
    {
        var callDoc = firestore.Collection(callPath).Document(meetingID);
        var answerCandidates = callDoc.Collection("answerCandidates");
        var offerCandidates = callDoc.Collection("offerCandidates");

        pc.OnIceCandidate = e =>
        {
            Debug.Log("OnIceCandidate called");
            if (e.Candidate != null)
            {
                var data = new Dictionary<string, object>
                {
                    { "candidate", e.Candidate },
                    { "sdpMid", e.SdpMid },
                    { "sdpMLineIndex", e.SdpMLineIndex }
                };
                answerCandidates.AddAsync(data);
            }
        };

        Task<Call?> callTask = GetData(meetingID);
        while (!callTask.IsCompleted) yield return null;
        var callData = callTask.Result;

        if (callData == null)
        {
            Debug.LogError("Call data is null");
            yield break;
        }

        string offerSdp = callData.Value.offer["sdp"];
        string offerType = callData.Value.offer["type"];

        var offerDesc = new RTCSessionDescription
        {
            type = RTCSdpType.Offer,
            sdp = offerSdp
        };
        pc.SetRemoteDescription(ref offerDesc);

        var answerOp = pc.CreateAnswer();
        yield return answerOp;

        if (answerOp.IsError)
        {
            Debug.LogError($"CreateAnswer failed: {answerOp.Error.message}");
            yield break;
        }

        var answerDesc = answerOp.Desc;
        pc.SetLocalDescription(ref answerDesc);

        var answerData = new Dictionary<string, object>
        {
            { "type", "answer" },
            { "sdp", answerDesc.sdp }
        };

        yield return callDoc.UpdateAsync(new Dictionary<string, object> { { "answer", answerData } }).AsYieldInstruction();

        ListenForOfferCandidates(offerCandidates);

        ShowRoomScreen();
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
                offerDescription = callData.offer;
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
