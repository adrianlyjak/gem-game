using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridController : MonoBehaviour
{

		public int numColors = 4;
		public Color[] colors;
		private GridObject selectedGridObject;
		public Grid grid;
		public int sequenceSize = 3;
		public Score score;
		public bool isHex = false;
		public bool preventInput = false;
		private bool isUserRotating = false;
		public List<List<GridObject>> cachedMatches = null;
		private List<GridObject> switchingObjects = new List<GridObject> ();
		private float originAngle;
		private float startZ;
		private bool isRotating = false;
		private float rotationStartTime;
		private float rotationDistance;
		private float rotationStartAngle;
		public float dragSpeed = 2;
		public AudioClip selectSound;
		public AudioClip rejectSound;
		public AudioClip matchSound;
		public AudioClip noMovesSound;
		public AudioClip deselectSound;


		private PossibleMatches CachedPossibleMatches {
			get;
			set;
		}
		void InitGrid ()
		{
		
				if (isHex) {
						grid = gameObject.GetComponent<HexGrid> ();
				} else {
						grid = gameObject.GetComponent<SquareGrid> ();
				}
				grid.controller = this;
				grid.Initialize ();
		}
	
		void InitScore ()
		{
				score = gameObject.GetComponent<Score> ();
				score.controller = this;
		}

		void InitCameraColor ()
		{
				Camera.main.backgroundColor = new Color (0.302f, 0.38f, 0.439f, 1f);

		}

		void InitColors ()
		{
				/*
				colors = new Color[numColors];
				for (float i = 0; i < numColors; i++) {
						float start = 1.0f - (i / (numColors - 1.0f));
						float middle = ((((float)numColors - 1) / 2.0f) - Mathf.Abs (i - (((float)numColors - 1) / 2.0f))) / (((float)numColors - 1) / 2.0f);
						float end = (i / (numColors - 1.0f));
						colors [(int)i] = new Color (end, end, start);
				} 		
				*/
			
				/*    
			 	colors = new Color [15];
			    colors [0] = (new Color (0.282f, 0.784f, 0.086f, 1));
				colors [1] = (new Color (0.878f, 0.349f, 0.616f, 1));
				colors [2] = (new Color (0.2f, 0.498f, 0.627f, 1));
				colors [3] = (new Color (0.898f, 0.561f, 0.176f, 1));
				colors [4] = (new Color (0.639f, 0.62f, 0.6f, 1));
				colors [5] = (new Color (0.686f, 1f, 0.141f, 1));
				colors [6] = (new Color (0.286f, 0.788f, 0.788f, 1));
				colors [7] = (new Color (0.286f, 0.788f, 0.788f, 1));
				colors [8] = (new Color (0.059f, 0.533f, 0.384f, 1));
				colors [9] = (new Color (0.58f, 0.18f, 0.439f, 1));
				colors [10] = (new Color (0.247f, 0.227f, 0.588f, 1));
				colors [11] = (new Color (0.675f, 0.247f, 0.173f, 1));
				colors [12] = (new Color (0.949f, 0.933f, 0.914f, 1));
				colors [13] = (new Color (0.216f, 0.216f, 0.216f, 1));
				colors [14] = (new Color (1f, 0.792f, 0.722f, 1)); 
		*/
				colors = new Color [11];

				//yellow
				colors [0] = (new Color (0.98f, 1f, 0.369f, 1));
				//lime
				colors [1] = (new Color (0.651f, 0.918f, 0.227f, 1));
				//orange
				colors [2] = (new Color (1f, 0.698f, 0.275f, 1));
				//mint
				colors [3] = (new Color (0.122f, 0.784f, 0.573f, 1));
				//soviet red
				colors [4] = (new Color (1f, 0.408f, 0.329f, 1));
				//aqua
				colors [5] = (new Color (0.11f, 0.647f, 0.741f, 1));
				//cherry red
				colors [6] = (new Color (0.933f, 0.267f, 0.333f, 1));
				// electric blue
				colors [7] = (new Color (0.286f, 0.678f, 1f, 1));
				//magenta
				colors [8] = (new Color (0.965f, 0.373f, 0.757f, 1));
				//sky blue
				colors [9] = (new Color (0.576f, 0.843f, 0.984f, 1));
				// pink
				colors [10] = (new Color (1f, 0.608f, 0.875f, 1));

		}

		void Start ()
		{
				InitCameraColor ();
				InitColors ();
				InitScore ();
				InitGrid ();
		}

		public Color RandomColor ()
		{
				return colors [Random.Range (0, numColors)];
		}
	

		// manage selected grid objects
		public void AddSelected (GridObject newlySelected)
		{
				if (selectedGridObject == null) {
						audio.PlayOneShot (selectSound);
						selectedGridObject = newlySelected;
				} else {
						if ((grid.AdjacentGridObjects (newlySelected).Contains (selectedGridObject))) {
								grid.Switch (selectedGridObject, newlySelected);
								switchingObjects.Add (selectedGridObject);
								switchingObjects.Add (newlySelected);
								newlySelected.BeNotSelected ();
								selectedGridObject.BeNotSelected ();
								selectedGridObject = null;
						} else {
								audio.PlayOneShot (selectSound);
								selectedGridObject.BeNotSelected ();
								selectedGridObject = newlySelected;
						}
				}
		}
	
		public void RemoveSelected (GridObject newlySelected)
		{
				selectedGridObject = null;
		}
		
		public void CheckMatches ()
		{		
				List<List<GridObject>> matches = Matches ();
				if (matches.Count () > 0) {
						audio.PlayOneShot (matchSound);
						Debug.Log ("checking matches");
						// Add the scores
						foreach (List<GridObject> matchSequence in matches) {
								score.AddForMatch (matchSequence);
						}
						// Then destroy the objects
						foreach (List<GridObject> matchSequence in matches) {
		
								foreach (GridObject gridObject in matchSequence) {
										grid.RemoveObject (gridObject);
								}
						}
						grid.OnGridObjectsDestroyed (); 
				} else if (!PossibleMatches().HasMatches()) {
						Debug.Log ("no matches");
						audio.PlayOneShot (noMovesSound);
						grid.ResetBoard ();
				} else {
						score.matchesCombo = 0;
				}
				cachedMatches = null;
				CachedPossibleMatches = null;
				score.EndSequence ();

				//PossibleMatches ();
		}
		
		public PossibleMatches PossibleMatches ()
		{

				if (CachedPossibleMatches == null) {
						CachedPossibleMatches = new PossibleMatches ();
						foreach (List<Vector3> row in grid.Successions ()) {
								bool isAtEndOfMatch = false;
								PossibleMatch currentMatch = new PossibleMatch (this);
								PossibleMatch nextMatch = null;
								foreach (Vector3 point in row) {
										
										GridObject currentObject = grid.ObjectAt (point);
										if (nextMatch == null || !nextMatch.MatchObjects.Contains (currentObject)) {
												bool currentMatchFromNextMatch = false;
												if (nextMatch != null) {
														if (nextMatch.MatchObjects.Count > 0) {
																Debug.Log ("setting current match from next match. next match start " + nextMatch.MatchObjects.First ().Position () + " next match end: " + nextMatch.MatchObjects.Last ().Position () + " current object: " + currentObject.Position ());
														} else {
																Debug.Log ("EMPTY: setting current match from next match EMPTY");
														}
														currentMatch = nextMatch;
														nextMatch = null;
														currentMatchFromNextMatch = true;
												}
												if (currentMatch.Matches (currentObject)) {
														currentMatch.AddMatch (currentObject);
												} else {
														isAtEndOfMatch = true;
												}

												int trailingIndex = row.IndexOf (point);
												if (row.Count () == trailingIndex) {
														isAtEndOfMatch = true;
												}
												// split
												if (isAtEndOfMatch) {
														bool foundSplit = false;
														if (trailingIndex + 1 < row.Count ()) {
																GridObject nextNextObj = grid.ObjectAt (row.ElementAt (trailingIndex + 1));
																if (currentMatch.Matches (nextNextObj)) {
																		PossibleMatch splitMatch = currentMatch.Duplicate ();
																		splitMatch.Switch.Inhibitor = currentObject;
																		splitMatch.AddMatch (nextNextObj);
																		List<GridObject> perpMatches = new List<GridObject> ();
																		foreach (GridObject matchObj in grid.AdjacentGridObjects (currentObject)) {
																				if (splitMatch.Matches (matchObj)) {
																						perpMatches.Add (matchObj);
																				}
																		}
																		// if we have a perpendicular match, see if there's enough for a split
																		if (perpMatches.Count () > 0) {
																				int sequenceIndex = trailingIndex;
																				bool isAtEnd = false;
																				nextMatch = new PossibleMatch (this);
																				nextMatch.AddMatch(nextNextObj);
																				while (!isAtEnd) {
																						sequenceIndex = sequenceIndex + 1;	
																						if (sequenceIndex < row.Count ()) {
																								nextNextObj = grid.ObjectAt (row.ElementAt (sequenceIndex));
																								if (splitMatch.Matches (nextNextObj)) {
																										splitMatch.AddMatch (nextNextObj);
																										nextMatch.AddMatch(nextNextObj);
																								} else {
																										isAtEnd = true;
																								}
																						} else {
																								isAtEnd = true;
																						}
																				}
																				if (splitMatch.HasPotentialPossibleMatch ()) {
																						foundSplit = true;
																						foreach (GridObject matchObj in perpMatches) {
																								PossibleMatch perpMatch = splitMatch.Duplicate ();
																								perpMatch.Switch.Trigger = matchObj;
																								CachedPossibleMatches.Add (perpMatch);
																						}
																				}
																		}
																}
														}

				 
														if (currentMatch.HasPotentialPossibleMatch ()) {
																// leading

																if (!currentMatchFromNextMatch) {
																		int leadingIndex = row.FindIndex (pt => pt == currentMatch.MatchObjects.First ().Position ()) - 1;
																		if (leadingIndex >= 0) {
																				GridObject leadingObj = grid.ObjectAt (row.ElementAt (leadingIndex));
														
																				foreach (GridObject matchObj in grid.AdjacentGridObjects (leadingObj)) {
																						if (currentMatch.Matches (matchObj)) {
																								PossibleMatch leadingMatch = currentMatch.Duplicate (); 
																								leadingMatch.Switch.Inhibitor = leadingObj;
																								leadingMatch.Switch.Trigger = matchObj;																				
																								CachedPossibleMatches.Add (leadingMatch);

																						}
																				}
																		}
																}

																// trailing
																if (!foundSplit) {
																		foreach (GridObject matchObj in grid.AdjacentGridObjects(currentObject)) {
																				if (currentMatch.Matches (matchObj)) {
																						PossibleMatch trailingMatch = currentMatch.Duplicate ();
																						trailingMatch.Switch.Inhibitor = currentObject;
																						trailingMatch.Switch.Trigger = matchObj;
																						CachedPossibleMatches.Add (trailingMatch);
																				}
																		}
																}
														}
														isAtEndOfMatch = false;
														currentMatch = new PossibleMatch (this);
														currentMatch.AddMatch (currentObject);
												}
										}
								}
								// have to check if the final match is big enough 
						}
				}
				return CachedPossibleMatches;
		}

		public List<List<GridObject>> Matches ()
		{
		
				//	if (cachedMatches == null) {
				cachedMatches = new List<List<GridObject>> ();
				foreach (List<Vector3> row in grid.Successions ()) {
						GridObject lastObject = null;
						List<GridObject> currentMatch = new List<GridObject> ();
						foreach (Vector3 point in row) {
								GridObject currentObject = grid.ObjectAt (point);
								if (lastObject == null || currentObject.Matches (lastObject)) {
										currentMatch.Add (currentObject);
								} else {
										// add the finished match if its big enough before we start a new one
										if (currentMatch.Count >= sequenceSize) {
												cachedMatches.Add (currentMatch);
										}
										currentMatch = new List<GridObject> ();
										currentMatch.Add (currentObject);
								}
				
								lastObject = currentObject;
						}
						// have to check if the final match is big enough 
						if (currentMatch.Count >= sequenceSize) {
								cachedMatches.Add (currentMatch);
						}
				}
				//	}
				return cachedMatches;
		}

		void Update ()
		{
				if (switchingObjects.Count () == 2) {
						GridObject first = switchingObjects.First ();
						if (first.Position () == first.gameObject.transform.position) {
								if (Matches ().Count > 0) {
										score.StartSequence ();
										score.AddMove ();
										CheckMatches ();
								} else {
										Debug.Log ("cant move there");
										audio.PlayOneShot (rejectSound);
										grid.Switch (first, switchingObjects [1]);
								}
								switchingObjects = new List<GridObject> ();
						}
				}
				if (!preventInput) {
						if (Input.GetMouseButtonDown (2)) {
								grid.ResetBoard ();
						}
						if (Input.GetMouseButtonDown (1)) {
								int newRotationIndex;
								if (grid.rotationIndex != (grid.RowsByDirection ().Count - 1)) {
										newRotationIndex = grid.rotationIndex + 1;
								} else {
										newRotationIndex = 0;
								}
								Debug.Log ("newRotationIndex: " + newRotationIndex);
								grid.SetTopTo (newRotationIndex);
						}
				
						if (!grid.IsRegenerating () && !isRotating) {
								GetRotationInput ();
						}

				}
				if (isRotating) {
						ContinueRotation ();
				}
		}

		public void RotateTo (float degrees)
		{
				isRotating = true;
				rotationStartTime = Time.time;
				rotationStartAngle = Camera.main.transform.localEulerAngles.z;
				rotationDistance = degrees - rotationStartAngle;
				if (rotationDistance > 180f) {
						rotationDistance = -1f * (360 - rotationDistance);
				}
		}
		
		public void ContinueRotation ()
		{
				Vector3 cameraRotation = Camera.main.transform.localEulerAngles;
				float distanceFraction = Mathf.Min (Time.time - rotationStartTime, 0.25f) / 0.25f;
				if (distanceFraction == 1f) {
						isRotating = false;
				}
				Camera.main.transform.rotation = Quaternion.Euler (cameraRotation.x, cameraRotation.y, rotationStartAngle + (distanceFraction * rotationDistance));			
		}

		public void ShowMatches (GridObject obj)
		{
				/*
				KeyValuePair<GridObject,GridObject> theKey = new KeyValuePair<GridObject,GridObject > ();
				foreach (KeyValuePair<GridObject,GridObject> key in PossibleMatches().Keys) {
						if (key.Key == obj) {
								theKey = key;
						}
				}
				theKey.Value.Glow ();
				foreach (GridObject seqMatch in PossibleMatches()[theKey]) {
						seqMatch.Glow ();
				}
				*/
		}

		public void GetRotationInput ()
		{

				if (Input.GetMouseButton (0)) {
						Vector2 screenCenter = new Vector2 (Screen.width / 2, Screen.height / 2);

						if (!isUserRotating) {
								Vector3 startMousePosition = Input.mousePosition;
								originAngle = Vector2.Angle (new Vector2 (0, 1), new Vector2 (startMousePosition.x - screenCenter.x, startMousePosition.y - screenCenter.y));
								if (startMousePosition.x > screenCenter.x) {
										originAngle = 360f - originAngle;
								}
								startZ = Camera.main.transform.localEulerAngles.z;
								isUserRotating = true;	
			
						}
						Vector3 mousePosition = Input.mousePosition;
						float currentAngle = Vector2.Angle (new Vector2 (0, 1), new Vector2 (mousePosition.x - screenCenter.x, mousePosition.y - screenCenter.y));
						if (mousePosition.x > screenCenter.x) {
								currentAngle = 360f - currentAngle;
						}
						float newRotation = (startZ + originAngle - currentAngle) % 360f;
						Camera.main.transform.rotation = Quaternion.Euler (0, 0, (newRotation));
				}
				if (Input.GetMouseButtonUp (0)) {
			
						int numDirections = grid.RowsByDirection ().Count ();
						int degreesPerDirection = 360 / numDirections;
						int directionIndex = (int)Mathf.Round ((360f - Camera.main.transform.localEulerAngles.z) / (float)degreesPerDirection);
						if (directionIndex == numDirections) {
								directionIndex = 0;
						}
						grid.SetTopTo (directionIndex);
						float rotateTo = 360f - (directionIndex * degreesPerDirection);
						RotateTo (rotateTo);
						isUserRotating = false;
			
				}

		}

		public void ShowHint ()
		{
	
				PossibleMatches().ShowMatches();
				//	PossibleMatches ().ElementAt (Random.Range (0, PossibleMatches ().Count ())).Key.Key.Glow ();
		}

		public void HideHints ()
		{
				foreach (GridObject obj in grid.AllGridObjects()) {
						obj.StopGlow ();
				}
		}

		public void PreventInput ()
		{
				if (!isUserRotating) {
						preventInput = true;
				}
		}

		public void AllowInput ()
		{
				preventInput = false;
		}
}
