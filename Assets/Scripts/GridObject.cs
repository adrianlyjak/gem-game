using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridObject : MonoBehaviour
{

		public Grid grid;
		public Color color;
		public bool selected = false;
		private bool isMoving = false;
		public bool isFalling = false;
		private bool isFadingIn = false;
		private Vector3 startPosition;
		private Vector3 endPosition;
		private float startTime;
		private float speed;
		private float journeyLength;
		public float timePerUnit = 0.005f;
		private bool checkedGravity = false;
		private bool isGlowing = false;
		private float glowStartTime;
		private float glowInterval = 0.25f;
		private float fadeInStartTime;
		public int column;

		public void SetColor ()
		{
				color = grid.controller.RandomColor ();
				gameObject.renderer.material.color = color;
		}

		void Update ()
		{
				if (isMoving) {
						Move ();
				}
				if (isGlowing) {
						GlowPulse ();
				}
				if (isFadingIn) {
						FadeIn ();
				}
		}

		void Move ()
		{
				float distCovered = (Time.time - startTime) * speed;
				float fracJourney = distCovered / journeyLength;
				Vector3 newPosition = Vector3.Lerp (startPosition, endPosition, fracJourney); 
				if (fracJourney < 1.0f) {
						gameObject.transform.position = newPosition;
						if (isFalling && fracJourney > 0.5f) {
								GravityCheck ();
						}
				} else { 
						gameObject.transform.position = endPosition;
						if (isFalling && !grid.IsObjectAt (grid.NextGridLocation (this)) && !IsAtBottom ()) {
								checkedGravity = false;
								GravityCheck ();
								checkedGravity = false;
								ContinueFalling ();
						} else {
								isMoving = false;
								if (isFalling) {
										isFalling = false;
										checkedGravity = false;
										GravityCheck ();
										checkedGravity = false;
										grid.RemoveFalling (this);
								}
						}
				}
				
		}

		void GlowPulse ()
		{
				/*
		 time location is between 0 and 4.0 
		 0 to 1.0 the value is increasing
		 1.0 to 2.0 the value is reducing back to the original color
		 2.0 to 3.0 the value is decreasing below the original color
		 3.0 to 4.0 the value is increasing back to the original color

		*/
				float timeLocation = ((Time.time - glowStartTime) % (glowInterval * 4.0f)) / glowInterval;

				float percentColorOffset = timeLocation % 1.0f;
				if (timeLocation % 2.0f > 1.0f) {
						percentColorOffset = 1.0f - percentColorOffset;
				}
				float rOffset;
				float gOffset;
				float bOffset;
				if (timeLocation < 2.0f) {
						// increase value
						rOffset = Mathf.Min ((1f - color.r), 0.18f);
						gOffset = Mathf.Min ((1f - color.g), 0.18f);
						bOffset = Mathf.Min ((1f - color.b), 0.18f);
				} else {
						//decrease value
						rOffset = Mathf.Max ((color.r * -1f), -0.15f);
						gOffset = Mathf.Max ((color.g * -1f), -0.15f);
						bOffset = Mathf.Max ((color.b * -1f), -0.15f);
				}
				gameObject.renderer.material.color = new Color (color.r + (rOffset * percentColorOffset), color.g + (gOffset * percentColorOffset), color.b + (bOffset * percentColorOffset), gameObject.renderer.material.color.a);
		}

		private void FadeIn ()
		{
				Color cc = gameObject.renderer.material.color;
				float percentProgress = (Time.time - fadeInStartTime) / (timePerUnit * 2.0f);
				if (percentProgress >= 1.0f) {
						isFadingIn = false;
						gameObject.renderer.material.color = new Color (cc.r, cc.g, cc.b, 1.0f);
				} else {
						gameObject.renderer.material.color = new Color (cc.r, cc.g, cc.b, percentProgress);
				}
		}

		public void StartFalling ()
		{
				if ((!grid.IsObjectAt (grid.NextGridLocation (this)) && !IsAtBottom ())) {
						grid.AddFalling (this);
						isFalling = true;
						ContinueFalling ();
				}
		}

		public void StartFadingIn ()
		{
				isFadingIn = true;
				fadeInStartTime = Time.time;
				Color cc = gameObject.renderer.material.color;
				gameObject.renderer.material.color = new Color (cc.r, cc.g, cc.b, 0.0f);
		}

		private void ContinueFalling ()
		{

				MoveTo (grid.NextGridLocation (this));
				
		}

		void GravityCheck ()
		{
				if (!checkedGravity) {
						GridObject prevObject = grid.PreviousGridObject (this);
						if (prevObject != null) {
								if (!prevObject.isFalling) {
										prevObject.StartFalling ();
										checkedGravity = true;

								}
						} else if (!(IsAtTop ())) {
								grid.SpawnObjectAtCol (grid.ColForPoint (Position ()));
						}
				}
		}
	
		public void MoveTo (Vector3 pos)
		{
				isMoving = true;
				startPosition = gameObject.transform.position;
				endPosition = pos;
				startTime = Time.time;
				journeyLength = Vector3.Distance (startPosition, endPosition);
				speed = journeyLength / timePerUnit;
		}
	
		public Vector3 Position ()
		{
				if (isMoving) {
						return endPosition;
				} else {
						return gameObject.transform.position;
				}
		}

		void OnMouseOver ()
		{
				if (Input.GetMouseButtonDown (1)) {
						foreach (PossibleMatch matchObj in grid.controller.PossibleMatches().Matching(this)) {
								Debug.Log ("me at:" + Position () + " matches: " + matchObj.ToString ());
						}
				}
				grid.controller.PreventInput ();
				if (Input.GetMouseButtonDown (0) && !grid.IsRegenerating ()) {
						ToggleSelected ();

						Debug.Log ("me at:" + Position ());

				}
				if (isGlowing && Input.GetMouseButtonDown (1)) {
						grid.controller.ShowMatches (this);
				}

		}

		void OnMouseExit ()
		{
				grid.controller.AllowInput ();

		}

		void ToggleSelected ()
		{ 
				selected = !selected;
				if (selected) {
						gameObject.renderer.material.color = new Color (color.r, color.g, color.b, 0.5f);
						grid.controller.AddSelected (this);
				} else {
						grid.audio.PlayOneShot (grid.controller.deselectSound);
						grid.controller.RemoveSelected (this);
						BeNotSelected ();
				}
		}

		public void  Glow ()
		{
				isGlowing = true;
				glowStartTime = Time.time;


		}

		public void StopGlow ()
		{
				isGlowing = false;
				gameObject.renderer.material.color = color;
		}

		public void BeNotSelected ()
		{
				selected = false;
				gameObject.renderer.material.color = color;
		}
		
		public bool IsFloating ()
		{
				return !IsAtBottom () && !grid.IsObjectAt (grid.NextGridLocation (this));
				
		}
		
		public bool IsAtTop ()
		{
				//		Debug.Log ("In IsAtTop, top cell for col, " + grid.ColForPoint (Position ()) + ": " + grid.TopCellAt (grid.ColForPoint (Position ())) + " =? " + gameObject.transform.position + " = " + (grid.TopCellAt (grid.ColForPoint (Position ())) == gameObject.transform.position));
				return grid.TopCellAt (grid.ColForPoint (Position ())) == gameObject.transform.position;
		}

		public bool IsAtBottom ()
		{
				//		Debug.Log ("grid.BottomCellAt(grid.ColForPoint(" + Position () + ")) = " + grid.BottomCellAt (grid.ColForPoint (Position ())));
				return grid.BottomCellAt (grid.ColForPoint (Position ())) == Position ();
		}

		public bool Matches (GridObject other)
		{
				return other.color == color;
		}

		public bool IsFalling ()
		{
				return isFalling;
		}
}	