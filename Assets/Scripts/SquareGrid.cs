using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareGrid : Grid
{

		public List<List<Vector3>> rowsByHorizontal;

		override public void Initialize ()
		{
				InitRowsByHorizontal ();
				base.Initialize ();
		}

		private void InitRowsByHorizontal ()
		{
				rowsByHorizontal = new List<List<Vector3>> ();
				List<Vector3> column;
				for (int y = 0; y < yUnits; y++) {
						column = new List<Vector3> ();
						rowsByHorizontal.Add (column);
						for (int x = 0; x < xUnits; x++) {
								column.Add (new Vector3 (min.x + (x * unitWidth), min.y + (y * unitHeight), min.z));
						}
				}
		}
	
		override public void InitRowsByVertical ()
		{
				rowsByVertical = new List<List<Vector3>> ();
				List<Vector3> row;
				for (int x = 0; x < xUnits; x++) {
						row = new List<Vector3> ();
						rowsByVertical.Add (row);
						for (int y = 0; y < yUnits; y++) {
								row.Add (new Vector3 (min.x + (x * unitWidth), min.y + (y * unitHeight), min.z));
						}
				}
		}

		override public bool IsBounded (Vector3 point)
		{
				return IsSquareBounded (point);
		}
		// Returns the point, but bounded within the grid
		override public Vector3 Bounded (Vector3 point)
		{
				return SquareBounded (point);
		}


		// Returns the nearest world location on the grid to <point>
		override public Vector3 NearestGridUnitCenter (Vector3 point)
		{
				Vector3 bounded = Bounded (point);
				float xOffset = bounded.x - min.x;
				float yOffset = bounded.y - min.y;
				xOffset = RoundToMultiple (xOffset, unitWidth);
				yOffset = RoundToMultiple (yOffset, unitHeight);
				return new Vector3 (min.x + xOffset, min.y + yOffset, point.z);
		}
		
		override public List<Vector3> AdjacentGridLocations (Vector3 point)
		{
				List<Vector3> adjacentPoints = new List<Vector3> ();
				adjacentPoints.Add (new Vector3 (point.x, point.y + unitHeight, point.z));
				adjacentPoints.Add (new Vector3 (point.x + unitWidth, point.y, point.z));
				adjacentPoints.Add (new Vector3 (point.x, point.y - unitHeight, point.z));
				adjacentPoints.Add (new Vector3 (point.x - unitWidth, point.y, point.z));
				return new List<Vector3> (adjacentPoints.Where (p => IsBounded (p)));
		}
		
		override public float unitWidth {
				get {
						return unitEdge;
				}
		}
	
		override public float unitHeight {
				get {
						return unitEdge;
				}
		}
	
		override public List<Vector3> allCells {
				get {
						List<Vector3> allCells = new List<Vector3> ();
						foreach (List<Vector3> row in rowsByHorizontal) {
								allCells.AddRange (row);
						}
						return allCells;
				}
		
		}

		override public List<List<Vector3>> Successions ()
		{
				return new List<List<Vector3>> (rowsByHorizontal.Concat (rowsByVertical));
		}

		override public List<List<List<Vector3>>> RowsByDirection ()
		{
				List<List<List<Vector3>>> rowsByDirection = new List<List<List<Vector3>>> ();
				List<List<Vector3>> rowsByVerticalCopy = new List<List<Vector3>> ();
				for (int i = rowsByVertical.Count() - 1; i >= 0; i--) {
						List <Vector3> newRow = new List<Vector3> (rowsByVertical [i]);
						newRow.Reverse ();
						rowsByVerticalCopy.Add (newRow);
				}
				List<List<Vector3>> rowsByHorizontalCopy = new List<List<Vector3>> ();
				for (int i = rowsByHorizontal.Count() - 1; i >= 0; i--) {
						List <Vector3> newRow = new List<Vector3> (rowsByHorizontal [i]);
						newRow.Reverse ();
						rowsByHorizontalCopy.Add (newRow);
				}
				rowsByHorizontalCopy.Reverse ();
				rowsByDirection.Add (rowsByVertical);
				rowsByDirection.Add (rowsByHorizontal);
				rowsByDirection.Add (rowsByVerticalCopy);
				rowsByDirection.Add (rowsByHorizontalCopy);
				return rowsByDirection;
		}
}
