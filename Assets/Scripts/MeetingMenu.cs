using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using PlayFab.DataModels;
using PlayFab;

public static class ButtonExtension
{
	public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
	{
		button.onClick.AddListener(delegate () {
			OnClick(param);
		});
	}
}

public class MeetingMenu : MonoBehaviour
{
	public GameObject buildingDropDown;
	public InputField meetingIdField;
	public InputField passwordField;
	public InputField meetingNameField;
	public Text feedbackText;

	private int buildingId;

	public GameObject meetingEntryPrefab;
	public Player player;


    [SerializeField] Dictionary<string, PlayFab.DataModels.ObjectResult> myMeetings;


    void Start()
	{
		player = GameObject.Find("Local").GetComponentInChildren<Player>();
		myMeetings = new Dictionary<string, ObjectResult>();

		LoadMeetings();
	}

	private void LoadMeetings()
    {
		GameObject meetingEntry;

		

		PlayFabDataAPI.GetObjects(new GetObjectsRequest()
		{
			Entity = player.playerEntity
		}, (getResult) =>
		{
			myMeetings = getResult.Objects;
			foreach (KeyValuePair<string, PlayFab.DataModels.ObjectResult> entry in myMeetings)
			{
				Meeting obj = JsonUtility.FromJson<Meeting>(entry.Value.DataObject.ToString());

				meetingEntry = Instantiate(meetingEntryPrefab, transform);
				meetingEntry.transform.GetChild(0).GetComponent<Text>().text = entry.Key.ToString();
				if (obj.building_id != 0) meetingEntry.transform.GetChild(3).GetComponent<Text>().text = buildingDropDown.transform.GetComponent<Dropdown>().options[obj.building_id].text;
				meetingEntry.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () {
					JoinClicked(obj);
				});
				meetingEntry.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate () {
					DeleteClicked(entry.Key);
				});
			}
		}, error =>
		{
			Debug.LogError(error);
		}
		);
	}

	private void ReloadMeetings()
    {
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}

		GameObject meetingEntry;

		foreach (KeyValuePair<string, PlayFab.DataModels.ObjectResult> entry in myMeetings)
		{
			Meeting obj = JsonUtility.FromJson<Meeting>(entry.Value.DataObject.ToString());

			Debug.Log(obj.meeting_id);
			Debug.Log(obj.password);
			Debug.Log(obj.building_id);

			meetingEntry = Instantiate(meetingEntryPrefab, transform);
			meetingEntry.transform.GetChild(0).GetComponent<Text>().text = entry.Key.ToString();
			if (obj.building_id != 0) meetingEntry.transform.GetChild(3).GetComponent<Text>().text = buildingDropDown.transform.GetComponent<Dropdown>().options[obj.building_id].text;
			meetingEntry.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () {
				JoinClicked(obj);
			});
			meetingEntry.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate () {
				DeleteClicked(entry.Key);
			});
		}
	}

	private void JoinClicked(Meeting meeting)
	{
		if (!string.IsNullOrEmpty(meeting.password)) GUIUtility.systemCopyBuffer = meeting.password;
		Application.ExternalEval("window.open(\"http://umass-amherst.zoom.us/j/" + meeting.meeting_id + "\")");
	}

	public void BuildingSelected(Dropdown dropdown)
	{
		buildingId = dropdown.value;
	}

	public void AddMeeting()
	{
		feedbackText.text = "";

		if (String.IsNullOrEmpty(meetingIdField.text) || String.IsNullOrEmpty(meetingNameField.text))
        {
			feedbackText.text = "Must complete all of the required fields to add a meeting";
			return;
        }

		meetingNameField.text = meetingNameField.text.Replace(" ", "");

		List<SetObject> meetingList = new List<SetObject>();

		Dictionary<string, object> meetingData = new Dictionary<string, object>()
		{
			{ "meeting_id", meetingIdField.text },
			{ "building_id", buildingId },
			{ "meeting_password", passwordField.text }
		};

		string meetingJson = JsonUtility.ToJson(meetingData);
		SetObject meeting = new SetObject() { ObjectName = meetingNameField.text, DataObject = meetingData };
		ObjectResult entry = new ObjectResult() { ObjectName = meetingNameField.text, DataObject = meetingJson };

		meetingList.Add(meeting);
		myMeetings.Add(meetingNameField.text, entry);

		PlayFabDataAPI.SetObjects(new SetObjectsRequest()
		{
			Entity = LoginManager.instance.playerEntity,
			Objects = meetingList
		}, (setResult) =>
		{
			ReloadMeetings();

			meetingNameField.text = "";
			meetingIdField.text = "";
			passwordField.text = "";
			buildingDropDown.GetComponent<Dropdown>().value = 0;
			buildingId = 0;

			feedbackText.color = new Color(103f, 255f, 157f);
			feedbackText.text = "Meeting added successfully!";

		}, (error) =>
		{
			Debug.LogError(error.GenerateErrorReport());
			feedbackText.color = new Color(255f, 81f, 81f);
			feedbackText.text = "Too many meetings, you have to delete one first.";
		}
		);
	}

	private void DeleteClicked(string key)
    {
		meetingNameField.text = "";
		meetingIdField.text = "";
		passwordField.text = "";
		buildingDropDown.GetComponent<Dropdown>().value = 0;
		buildingId = 0;

		feedbackText.text = "";

		myMeetings.Remove(key);

		List<SetObject> meetingList = new List<SetObject>();

		SetObject meeting = new SetObject() { ObjectName = key, DeleteObject = true };

		meetingList.Add(meeting);

		PlayFabDataAPI.SetObjects(new SetObjectsRequest()
		{
			Entity = player.playerEntity,
			Objects = meetingList
		}, (setResult) =>
		{
			feedbackText.text = "Meeting deleted successfully";
			ReloadMeetings();

		}, (error) =>
		{
			Debug.LogError(error.GenerateErrorReport());

            feedbackText.color = new Color(255f, 81f, 81f);
			feedbackText.text = "Couldn't delete meeting.";
        }
		);
	}
}