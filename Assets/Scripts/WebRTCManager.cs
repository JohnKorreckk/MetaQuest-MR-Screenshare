using System;
using System.Collections;
using System.Threading;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class WebRTCManager : MonoBehaviour
{

    RTCPeerConnection localPeerConnection;
    RTCPeerConnection remotePeerConnection;

    // Video components
    public RawImage localVideoImage;
    public RawImage remoteVideoImage;

    MediaStream localStream;

    private void Awake()
    {
        StartCoroutine(WebRTC.Update());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        var configuration = new RTCConfiguration
        {
            iceServers = new RTCIceServer[]
            {
                new RTCIceServer
                {
                    urls = new string[]
                    {
                        // Google Stun server
                        "stun:stun.l.google.com:19302"
                    },
                }
            },
        };

        localPeerConnection = new RTCPeerConnection(ref configuration);

        localPeerConnection.OnIceCandidate = candidate => {
            if (candidate != null)
            {
                // offerCandidates.Add(candidate);
            }
        };


        localPeerConnection.OnTrack = e => {
            if (e.Track.Kind == TrackKind.Video)
            {
                // remoteVideoImage.texture = e.Streams[0].GetVideoTracks()[0].Texture;
            }
        };

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
