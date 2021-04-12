using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeetingEntry : MonoBehaviour
{
    public string meetingName;
    public string meetingId;
    public string meetingPassword;

    public TextMeshProUGUI meetingNameText;

    public MeetingEntry(string meetingName, Meeting meeting)
    {
        if (meeting == null) return;

        this.meetingName = meetingName;
        meetingId = meeting.meeting_id;
        meetingPassword = meeting.password;
    }

    private void Start()
    {
        meetingNameText.text = meetingName;
    }

    public void joinMeeting()
    {
        if (!string.IsNullOrEmpty(meetingPassword)) GUIUtility.systemCopyBuffer = meetingPassword;
        Application.ExternalEval("window.open(\"http://umass-amherst.zoom.us/j/" + meetingId + "\")");
    }
}
