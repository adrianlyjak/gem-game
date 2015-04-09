using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HexGrid : Grid
{

		public List<List<Vector3>> rowsByForwDiag;
		public List<List<Vector3>> rowsByBackDiag;
		public Dictionary<float,float> maxYByColumn;
		public Dictionary<float,float> minYByColumn;
		private const float sqrt3 = 1.73205f;

		override public void Initialize ()
		{
				InitBounds ();
				InitRowsByForwDiag ();
				InitCellsByBackDiag ();
				base.Initialize ();
		}

		private void InitBounds ()
		{
				maxYByColumn = new Dictionary<float, float> ();
				minYByColumn = new Dictionary<float, float> ();
				for (int x = 0; x < (xUnits * 2) - 1; x++) {
						float col = (x * diagonalOffset.x) + min.x;
						maxYByColumn.Add (col, MaxY (col));
						minYByColumn.Add (col, MinY (col));
				}
		}

		override public void InitRowsByVertical ()
		{
				rowsByVertical = new List<List<Vector3>> ();
				for (int x = 0; x < xUnits; x++) {
						List<Vector3> column = new List<Vector3> ();
						
						for (int y = 0; y < yUnits; y++) {
								Vector3 point = new Vector3 (min.x + (x * unitWidth * 1.50f), min.y + (y * unitHeight), min.z);
								if (IsBounded (point)) {
										column.Add (point);
								}
						}
						rowsByVertical.Add (column);
						if (x < xUnits - 1) {
								column = new List<Vector3> ();
								for (int y = 0; y < yUnits - 1; y++) {
										Vector3 point = new Vector3 (min.x + (x * unitWidth * 1.50f) + diagonalOffset.x, min.y + (y * unitHeight) + diagonalOffset.y, min.z);
										if (IsBounded (point)) {
												column.Add (point);
										}
								}
								if (column.Count () > 0) {
										rowsByVertical.Add (column);
								}
						}
				}
		}

		private void InitRowsByForwDiag ()
		{
				rowsByForwDiag = new List<List<Vector3>> ();
				Vector3 realTopLeft = NearestGridUnitCenter (topLeft);
				for (int y = 0; y < yUnits; y++) {
						List<Vector3> row = new List<Vector3> ();
						float originX;
						float originY;
						if (y > yUnits / 2) {
								int halfRoundedUp = (yUnits + 1) / 2;
								int amountAboveHalf = y - halfRoundedUp;
								originX = realTopLeft.x + (amountAboveHalf * diagonalOffset.x);
								originY = realTopLeft.y - (halfRoundedUp * unitHeight) - (amountAboveHalf * diagonalOffset.y);
						} else {
								originX = realTopLeft.x;
								originY = realTopLeft.y - (y * unitHeight);
						}
						Vector3 origin = new Vector3 (originX, originY, realTopLeft.z);
						for (int x = 0; x <= yUnits; x++) {
								if ((y < yUnits) || (x / 2.0f >= ((float)y + 1.0f - (float)yUnits))) {
										Vector3 point = new Vector3 (origin.x + (diagonalOffset.x * x), origin.y + (diagonalOffset.y * x), origin.z);
										if (RoundToMultiple (point.x, 0.1f) == 2.5f && RoundToMultiple (point.y, 0.1f) == 10.4f) {
												Debug.Log ("point: " + point + " IsBounded: " + IsBounded (point) + " maxY: " + MaxY (point.x) + " max in collection: " + maxYByColumn [point.x]);
										}
										if (IsBounded (point)) {
												row.Add (point);
										} else {
												Debug.Log ("point: " + point + " is unbounded");
										}
								} 
						}
						if (row.Count () > 0) {
								rowsByForwDiag.Add (row);
						}
				}
		}

		private void InitCellsByBackDiag ()
		{
		
				rowsByBackDiag = new List<List<Vector3>> ();
				Vector3 realTopRight = NearestGridUnitCenter (max);
				for (int y = 0; y < yUnits; y++) {
						List<Vector3> row = new List<Vector3> ();
						Vector3 origin = new Vector3 (realTopRight.x, realTopRight.y - (y * unitHeight), realTopRight.z);
						for (int x = 0; x <= yUnits; x++) {
								if ((y < yUnits) || (x / 2.0f >= ((float)y + 1.0f - (float)yUnits))) {
										Vector3 point = new Vector3 (origin.x - (diagonalOffset.x * x), origin.y + (diagonalOffset.y * x), origin.z);
										if (IsBounded (point)) {
												row.Add (point);
										}
								}
						}
						if (row.Count () > 0) {
								rowsByBackDiag.Add (row);
						}
				}
		}

		override public float unitWidth {
				get {
						return unitEdge * 2;
				}
		}
	
		override public float unitHeight {
				get {
						return unitEdge * sqrt3;
				}
		}

		override public List<Vector3> allCells {
				get {
						List<Vector3> allCells = new List<Vector3> ();
						foreach (List<Vector3> row in rowsByVertical) {
								allCells.AddRange (row);
						}
						return allCells;
				}
		}

		override public Vector3 NearestGridUnitCenter (Vector3 point)
		{
				Vector3 bounded = Bounded (point);
				
				float xOffset = bounded.x - min.x;
				float yOffset = bounded.y - min.y;
				
				// in terms of "hex units"
				Vector2 xyHex = new Vector2 (xOffset / (unitWidth / 4), yOffset / (unitHeight / 2));
				
				float evenX = RoundToMultiple (xyHex.x, 6);
				float evenY = RoundToMultiple (xyHex.y, 2);
				float oddX = RoundToMultiple (xyHex.x + 3, 6) - 3; 
				float oddY = RoundToMultiple (xyHex.y + 1, 2) - 1;			
				
				Vector2 xyEven = new Vector2 (evenX, evenY);
				Vector2 xyOdd = new Vector2 (oddX, oddY);

				//Some Pythagorean theorems to figure out which is closer
				float distanceToEven = Mathf.Sqrt (Mathf.Pow (Mathf.Abs (xyHex.x - xyEven.x), 2) + Mathf.Pow (Mathf.Abs (xyHex.y - xyEven.y), 2));
				float distanceToOdd = Mathf.Sqrt (Mathf.Pow (Mathf.Abs (xyHex.x - xyOdd.x), 2) + Mathf.Pow (Mathf.Abs (xyHex.y - xyOdd.y), 2));

				Vector2 closerPoint;
				
				if (distanceToOdd > distanceToEven) {
						closerPoint = xyEven;
				} else {
						closerPoint = xyOdd;
				}
					
				// back into offset units, and then into world coordinates
				Vector3 worldUnits = new Vector3 ((closerPoint.x * (unitWidth / 4)) + min.x, (closerPoint.y * (unitHeight / 2)) + min.y, point.z);
				if (worldUnits.y > max.y) {
						return worldUnits - new Vector3 (0, unitHeight, 0);
				} else {
						return worldUnits;	
				}
		}

		override public List<Vector3> AdjacentGridLocations (Vector3 point)
		{
				List<Vector3> adjacentPoints = new List<Vector3> ();
				adjacentPoints.Add (new Vector3 (point.x, point.y + unitHeight, point.z));
				adjacentPoints.Add (new Vector3 (point.x + diagonalOffset.x, point.y + diagonalOffset.y, point.z));
				adjacentPoints.Add (new Vector3 (point.x + diagonalOffset.x, point.y - diagonalOffset.y, point.z));
				adjacentPoints.Add (new Vector3 (point.x, point.y - unitHeight, point.z));
				adjacentPoints.Add (new Vector3 (point.x - diagonalOffset.x, point.y - diagonalOffset.y, point.z));
				adjacentPoints.Add (new Vector3 (point.x - diagonalOffset.x, point.y + diagonalOffset.y, point.z));
				return new List<Vector3> (adjacentPoints.Where (p => IsBounded (p)));

		}

		override public float MaxY (float x)
		{
				float heightOffset;
				if (units % 2 == 0) {
						heightOffset = unitHeight / 2.0f;
				} else {
						heightOffset = 0f;
				}
				float boardWidth = (max.x - min.x);
				float relX = x - min.x;
				float boardUnit = boardWidth / sqrt3;
				float boardHeight = boardUnit * 2;
				float baseHeight = ((boardHeight - boardUnit) / 2.0f) + boardUnit;
				float offsetFromEdge;
				if (relX < (boardWidth / 2.0f)) {
						offsetFromEdge = relX;
				} else {
						offsetFromEdge = boardWidth - relX;
				}
				return min.y + heightOffset + baseHeight + (offsetFromEdge / sqrt3);
		}

		override public float MinY (float x)
		{
				float heightOffset;
				if (units % 2 == 0) {
						heightOffset = unitHeight / 2.0f;
				} else {
						heightOffset = 0f;
				}
				float boardWidth = (max.x - min.x);
				float relX = x - min.x;
				float boardUnit = boardWidth / sqrt3;
				float boardHeight = boardUnit * 2;
				float baseHeight = ((boardHeight - boardUnit) / 2.0f);
				float offsetFromEdge;
				if (relX < (boardWidth / 2.0f)) {
						offsetFromEdge = relX;
				} else {
						offsetFromEdge = boardWidth - relX;
				}
				return heightOffset + min.y + baseHeight - (offsetFromEdge / sqrt3);
		}

		override public bool IsBounded (Vector3 point)
		{
				//	return IsSquareBounded (point);
				return IsSquareBounded (point) && RoundToMultiple (point.y, 0.1f) >= RoundToMultiple (minYByColumn [point.x], 0.1f) && RoundToMultiple (point.y, 0.1f) <= RoundToMultiple (maxYByColumn [point.x], 0.1f);
		}

		// Returns the point, but bounded within the grid

		override public Vector3 Bounded (Vector3 point)
		{
				Vector3 squareBounded = SquareBounded (point);
				//	return squareBounded;
				float x = squareBounded.x;
				float y = squareBounded.y;
				float minY = minYByColumn [x];
				float maxY = maxYByColumn [x];
				if (RoundToMultiple (y, 0.1f) < RoundToMultiple (minY, 0.1f)) {
						y = minY;
				}
				if (RoundToMultiple (y, 0.1f) > RoundToMultiple (maxY, 0.1f)) {
						y = maxY;
				}
				return new Vector3 (x, y, squareBounded.z);
		}

		override public List<List<Vector3>> Successions ()
		{
				return new List<List<Vector3>> (rowsByVertical.Concat (rowsByForwDiag.Concat (rowsByBackDiag)));
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
				List<List<Vector3>> rowsByForwDiagCopy = new List<List<Vector3>> ();
				for (int i = rowsByForwDiag.Count() - 1; i >= 0; i--) {
						List <Vector3> newRow = new List<Vector3> (rowsByForwDiag [i]);
						newRow.Reverse ();
						rowsByForwDiagCopy.Add (newRow);
				}
				List<List<Vector3>> rowsByBackDiagCopy = new List<List<Vector3>> ();
				for (int i = rowsByBackDiag.Count() - 1; i >= 0; i--) {
						List <Vector3> newRow = new List<Vector3> (rowsByBackDiag [i]);
						newRow.Reverse ();
						rowsByBackDiagCopy.Add (newRow);
				}
				rowsByDirection.Add (rowsByVertical);
				rowsByDirection.Add (rowsByForwDiag);
				rowsByDirection.Add (rowsByBackDiagCopy);

				rowsByDirection.Add (rowsByVerticalCopy);
				rowsByDirection.Add (rowsByForwDiagCopy);
				rowsByDirection.Add (rowsByBackDiag);

				return rowsByDirection;
		}

		override public Vector3 max {
				get {
						return new Vector3 (min.x + (unitWidth * 1.5f * (xUnits - 1)), min.y + (unitHeight * (yUnits - 1)), min.z);
				}
		}

		private Vector2 diagonalOffset {
				get {
					
						return new Vector2 (unitWidth * 0.75f, unitHeight / 2.0f);
				}
		}

		override  public int yUnits {
				get {
						if (units % 2 == 0) {
								return units * 2;
						} else {
								return units + (units - 1);
						}
				}
		}

}
