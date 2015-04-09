using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PossibleMatches
{


		public PossibleMatches ()
		{
				LookupMatchesBySwitch = new Dictionary<Switch,List<PossibleMatch>> ();
		}

		public Dictionary<Switch,List<PossibleMatch>> LookupMatchesBySwitch {
				get;
				set;
		}

		public List<PossibleMatch> Matching (Switch sw)
		{
				List<PossibleMatch> value;
				if (LookupMatchesBySwitch.TryGetValue (sw, out value)) {
						return value;
				} else {
						return new List<PossibleMatch> ();
				}
		}

		public List<PossibleMatch> Matching (GridObject obj)
		{
				List<PossibleMatch> matchesUsing = new List<PossibleMatch> ();
				foreach (List<PossibleMatch> matchList in LookupMatchesBySwitch.Values) {
						if (matchList.First ().Switch.Contains (obj)) {
								matchesUsing.AddRange (matchList);
						}
				}
			
				return matchesUsing;

		}

		public void Add (PossibleMatch possibleMatch)
		{
				List<PossibleMatch> value;
				if (LookupMatchesBySwitch.TryGetValue (possibleMatch.Switch, out value)) {
						if (!value.Any (otherMatch => otherMatch.EqualsOrContains (possibleMatch))) {
								IEnumerable<PossibleMatch> smaller = value.Where (otherMatch => possibleMatch.Contains (otherMatch));
								foreach (PossibleMatch smallerMatch in smaller) {
										value.Remove (smallerMatch);
								}
								value.Add (possibleMatch);
						}
				} else {
						value = new List<PossibleMatch> ();
						value.Add (possibleMatch);
						LookupMatchesBySwitch.Add (possibleMatch.Switch, value);
				}

		}

		public bool HasMatches ()
		{
				return LookupMatchesBySwitch.Count > 0;
		}

		public void ShowMatches ()
		{
				List<PossibleMatch> largestMatch = new List<PossibleMatch> ();
				foreach (List<PossibleMatch> matchList in LookupMatchesBySwitch.Values) {
						if (matchList.Count > largestMatch.Count) {
								largestMatch = matchList;
						}
				}
				largestMatch.First ().Switch.Trigger.Glow ();

		}

}
