using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PossibleMatch
{

	
		public PossibleMatch (GridController controller)
		{
				Controller = controller;
				MatchObjects = new List<GridObject> ();
				Switch = new Switch ();
		}

		public PossibleMatch (PossibleMatch original)
		{
				Controller = original.Controller;
				Switch = original.Switch.Duplicate();
				MatchObjects = new List<GridObject> (original.MatchObjects);
		}

		public GridController Controller { get; set; }
	
		public Switch Switch { get; set; }

		public List<GridObject> MatchObjects { get; set; }

		public void AddMatch (GridObject match)
		{
				MatchObjects.Add (match);
		}

		public bool Matches (GridObject obj)
		{

				if (MatchObjects.Count () == 0) {
						return true;
				} else	if (MatchObjects.Contains (obj)) {
						return false;
				} else {
						return MatchObjects.First ().Matches (obj);
				}
		}

		public PossibleMatch Duplicate ()
		{
				return new PossibleMatch (this);
		}

		public override bool Equals (object obj)
		{
				if (obj == null)
						return false;
				PossibleMatch objAsPart = obj as PossibleMatch;
				if (objAsPart == null)
						return false;
				else
						return Equals (objAsPart);
		}


		public bool EqualsOrContains (PossibleMatch other) {
			return Equals (other) || Contains (other);
		}

		public bool Contains (PossibleMatch other) {
			if (other.MatchObjects.Count >= MatchObjects.Count) {
				return false;
			}
			int matchingIndex = MatchObjects.FindIndex (obj => obj.Equals(other.MatchObjects.First ()));
			if (matchingIndex == -1 || MatchObjects.Count - matchingIndex < other.MatchObjects.Count) {
				return false;
			}
			
		GridObject[] matchObjectsPart = new GridObject[other.MatchObjects.Count ()];
		MatchObjects.CopyTo (matchingIndex, matchObjectsPart, 0, other.MatchObjects.Count () + matchingIndex - 1);
		return new List<GridObject> (matchObjectsPart).SequenceEqual (other.MatchObjects);	
		}

		public override int GetHashCode () {
			return Switch.GetHashCode ();
		}
		public bool Equals (PossibleMatch other)
		{
				return (SameMove(other) && MatchObjects.SequenceEqual (other.MatchObjects));
		}

		public bool SameMove (PossibleMatch other)
		{
				return Switch.Equals(other.Switch);
		}

		public bool SwitchesGridObject (GridObject obj)
		{
				return Switch.Contains(obj);
		}

		public bool HasPotentialPossibleMatch ()
		{
				return MatchObjects.Count >= Controller.sequenceSize - 1;
		}

		public bool IncludesSequence (List<GridObject> otherSequence)
		{
				int startIndex = MatchObjects.FindIndex (obj => obj.Equals (otherSequence.First ()));
				if (!(startIndex >= 0)) {
						return false;
				} 
				if (otherSequence.Count () > MatchObjects.Count () - startIndex) {
						return false;
				}

				GridObject[] matchObjectsPart = new GridObject[otherSequence.Count ()];
				MatchObjects.CopyTo (startIndex, matchObjectsPart, 0, otherSequence.Count () + startIndex - 1);
				return new List<GridObject> (matchObjectsPart).SequenceEqual (otherSequence);
					
		}

		public override string ToString ()
		{
				string description = "a PossibleMatch: " + Switch.ToString();

				if (MatchObjects != null) {
						description = description + " Match size: " + MatchObjects.Count.ToString ();
				}

				if (MatchObjects.Count > 0) {
						description = description + " from: " + MatchObjects.First ().Position ().ToString () + " to: " + MatchObjects.Last ().Position ().ToString ();
				}

				return description;
		}
		
}
