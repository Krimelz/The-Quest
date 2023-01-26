using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
	public string dbName; 
	private string _path; 
	[Space]
	public Text sceneText; 
	public float printSpeed = 0.1f; 
	private Coroutine _printTextCoroutine;
	[Space]
	public Button[] actions; 

	private SqliteConnection _dbConnection; 
	private SqliteCommand _dbCommand; 
	private SqliteDataReader _dbReader; 

	[SerializeField]
	private int currentScene = 1; 

	public void Start()
	{
		SetConnection();
		NextScene(1);
	}

	public void SetConnection()
	{
		_path = Application.dataPath + "/StreamingAssets/" + dbName;

		_dbConnection = new SqliteConnection("URI=file:" + _path);
		_dbConnection.Open();

		if (_dbConnection.State == ConnectionState.Open)
		{
			_dbCommand = new SqliteCommand();
			_dbCommand.Connection = _dbConnection;
		}
	}

	public void NextScene(int nextSceneId)
	{
		currentScene = nextSceneId;

		DisableButtons(actions.Length);
		SetTitle();
		SetActions();
		Canvas.ForceUpdateCanvases();
	}

	private void SetTitle()
	{
		if (_printTextCoroutine != null)
		{
			StopCoroutine(_printTextCoroutine);
		}

		sceneText.text = "";

		_dbCommand.CommandText = "SELECT Scenes.title FROM Scenes WHERE Scenes.id=" + currentScene.ToString();
		_dbReader = _dbCommand.ExecuteReader();
		_dbReader.Read();

		_printTextCoroutine = StartCoroutine(PrintText(_dbReader.GetString(0)));
		_dbReader.Close();
	}

	private IEnumerator PrintText(string str)
	{
		for (int i = 0; i < str.Length; i++)
		{
			sceneText.text += str[i]; // TODO: Заменить на StringBuilder
			yield return new WaitForSeconds(printSpeed);
		}
	}

	private void SetActions()
	{
		_dbCommand.CommandText = 
			"SELECT Actions.text, Actions.next_scene_id FROM Actions WHERE Actions.scene_id="
			+ currentScene.ToString();
		_dbReader = _dbCommand.ExecuteReader();

		int c = 0;
		while (_dbReader.Read())
		{
			actions[c].onClick.RemoveAllListeners();
			actions[c].gameObject.SetActive(true);
			actions[c].GetComponentInChildren<Text>().text = _dbReader.GetString(0);

			int nextSceneId;

			try
			{
				nextSceneId = _dbReader.GetInt32(1);
				actions[c].onClick.AddListener(() => NextScene(nextSceneId));
			}
			catch
			{
				nextSceneId = 0;
				actions[c].onClick.AddListener(() => SceneManager.LoadScene("Main"));
			}

			c++;
		}

		_dbReader.Close();
	}

	private void DisableButtons(int count)
	{
		for (int i = 0; i < count; i++)
		{
			actions[i].gameObject.SetActive(false);
		}
	}
	public void ExitToMain()
	{
		SceneManager.LoadScene("Main");
	}
}
