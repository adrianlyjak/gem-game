using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Score : MonoBehaviour {
	public int score = 0;
	public int moves = 0;
	public int hints = 4;
	public int matchesCombo = 0;
	public GridController controller;
	public int maxMoves = 20;
	public float timeAvailable = 20.0f;
	private float startTime = -1f;
	private int sequenceScore = 0;
	private bool inSequence = false;
	public void Start() {

	}
	private void StartClock() {
		startTime = Time.time;
	}

	public void AddMove() {
		if (moves == 0) {
			StartClock ();
		}
		moves = moves + 1;
	}

	public void AddForMatch(List<GridObject> match) {
		int scoreAddition = (match.Count - (controller.sequenceSize - 1)) + matchesCombo;
		Debug.Log ("Added " + scoreAddition + " for match of size " + match.Count + " current matchesCombo " + matchesCombo);
		score = score + scoreAddition;
		sequenceScore = sequenceScore + scoreAddition;
		Debug.Log ("new score! " + score);
		matchesCombo++;
	}
	public int Reset() {
		int scoreCopy = score;
		score = 0;
		moves = 0;
		hints = 4;
		return scoreCopy;
	}
	public bool MovesMaxed() {
		return moves == maxMoves;
	}

	public float TimeLeft() {
		if (startTime != -1f) {
			return timeAvailable - (Time.time - startTime);
		} else {
			return timeAvailable;
		}
	}

	public void StartSequence() {
		sequenceScore = 0;
		inSequence = true;
	}

	public void EndSequence() {
		inSequence = false;
	}
	void OnGUI() {
		GUI.Box (new Rect ( 0.5f, 0.5f, 100f, 25f), "Score: " + score.ToString ());
		GUI.Box (new Rect ( 0.5f, 27.5f, 100f, 25f), "Moves: " + moves.ToString ());
		GUI.Box (new Rect ( 0.5f, 55.5f, 100f, 25f), "Time Left: " + TimeLeft ().ToString ("0.0"));
		if (GUI.Button (new Rect (0.5f,82.5f,100f,25f), "Hints: " + hints.ToString()) && hints > 0) {
			print ("You clicked the hint button!");
			controller.ShowHint();
			hints--;
		}
		GUI.Box (new Rect ( 0.5f, 109.5f, 100f, 25f), "Last Move: " + sequenceScore.ToString () + "!");

	}
}
