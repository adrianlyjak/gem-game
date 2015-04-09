using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Grid : MonoBehaviour
{

		public float unitEdge = 2.0f;
		public int units = 10;
		public Transform gridObjectPrefab;
		public GridController controller;
		public List<Vector3> topCells;
		private bool isRegenerating = false;
		private List<GridObject> fallingObjects = new List<GridObject> ();
		public Dictionary<float, List<GridObject>> objectsByColumn = new Dictionary<float, List<GridObject>> ();
		public List<List<Vector3>> rowsByVertical;
		public int rotationIndex = 0;
		public Vector2 gravityDirection;

		abstract public float unitWidth{ get; }

		abstract public List<Vector3> allCells { get; }

		abstract public float unitHeight { get; }
	
		abstract public Vector3 NearestGridUnitCenter (Vector3 point);

		abstract public List<Vector3> AdjacentGridLocations (Vector3 point);

		abstract public List<List<Vector3>> Successions ();

		abstract public List<List<List<Vector3>>> RowsByDirection ();

		abstract public void InitRowsByVertical ();

		abstract public bool IsBounded (Vector3 point);

		abstract public Vector3 Bounded (Vector3 point);

		virtual public float MaxY (float x)
		{
				return max.y;
		}

		virtual public float MinY (float x)
		{
				return min.y;
		}

		virtual public void Initialize ()
		{
				InitRowsByVertical ();
				InitGravityDirection ();
				InitTopCells ();
				InitObjectsByCol ();
				InitCameraPosition ();
				InitBoard ();
				StartRegenerating ();		
		}

		virtual public void InitTopCells ()
		{
				topCells = new List<Vector3> ();
				foreach (List<Vector3> col in Columns()) {
						topCells.Add (col.Last ());
				}
		}

		public void Update ()
		{
				if (isRegenerating) {
						if (fallingObjects.Count () == 0) {
								Debug.Log ("Stop Regenerating");
								StopRegenerating ();							
						}
				}
		}

		virtual public void InitCameraPosition ()
		{
				float bigger = Mathf.Max (max.x, max.y);
				Camera.main.transform.position = new Vector3 ((max.x / 2f) + (unitWidth / 4f), (max.y / 2) + (unitHeight / 4f), -1f * bigger);
		}

		virtual public void InitObjectsByCol ()
		{
				foreach (Vector3 point in topCells) {
						objectsByColumn.Add (ColForPoint (point), new List<GridObject> ());
				}

		}

		virtual public void InitGravityDirection ()
		{
				List<Vector3> column = Columns ().ElementAt (1);
				gravityDirection = (column.ElementAt (0) - column.ElementAt (1));
		}

		virtual public void ResetBoard ()
		{
				foreach (List<GridObject> col in objectsByColumn.Values) {
						foreach (GridObject obj in col) {
								Destroy (obj.gameObject);
						}
				}
				objectsByColumn = new Dictionary<float, List<GridObject>> ();
				InitObjectsByCol ();
				StartRegenerating ();
		}

		virtual public void InitBoard ()
		{
				int times = 0;
				foreach (List<Vector3> col in rowsByVertical) {
						foreach (Vector3 point in col) {
								SpawnObjectAt (point);
						}
				}
				while (times < 15 && controller.Matches().Count != 0) {
						times = times + 1;
						foreach (List<GridObject> match in controller.Matches()) {
								foreach (GridObject obj in match) {
										obj.SetColor ();
								}
						}
						controller.cachedMatches = null;
				}
		}

		virtual public void StopRegenerating ()
		{
				isRegenerating = false;
				controller.CheckMatches ();
		}
	
		virtual public void StartRegenerating ()
		{
				Debug.Log ("start regenerating");
				isRegenerating = true;
				foreach (Vector3 point in topCells) {
						if (!IsObjectAt (point)) {
								SpawnObjectAtCol (ColForPoint (point));
						}
				}
		}

		virtual public float ColForPoint (Vector3 point)
		{
				if (gravityDirection.x == 0f) {
						return point.x;
				} else {
						return Mathf.Round ((point.y - ((gravityDirection.y / gravityDirection.x) * point.x)) * 100);
				}
		}

		virtual public GridObject SpawnObjectAtCol (float col)
		{
			//	Debug.Log ("looking for object at: " + col);
				if (TryObjectNear (TopCellAt (col)) == null) {
						GridObject gridObject = SpawnObjectAt (TopCellAt (col));
						gridObject.StartFalling ();
						gridObject.StartFadingIn ();
						return gridObject;
				} else {
						return null;
				}
		}

		virtual public GridObject SpawnObjectAt (Vector3 point)
		{
				Transform newTransform = GameObject.Instantiate (gridObjectPrefab) as Transform;
				newTransform.parent = gameObject.transform;
				newTransform.position = point;
				GridObject gridObject = newTransform.gameObject.GetComponent<GridObject> ();
				gridObject.grid = this;
				objectsByColumn [ColForPoint (point)].Add (gridObject);
				gridObject.SetColor ();
				return gridObject;
		}

		virtual	public void OnGridObjectsDestroyed ()
		{
				controller.HideHints ();
				foreach (KeyValuePair<float, List<GridObject>> keyVal in objectsByColumn) {
						bool foundFloat = false;
						
						List<GridObject> column = new List<GridObject> (keyVal.Value);
						foreach (GridObject obj in column) {
								if (!foundFloat) {
										if (obj.IsFloating ()) {
												obj.StartFalling ();
												foundFloat = true;
												isRegenerating = true;
										}
								}
						}
				}
				if (!isRegenerating && controller.score.MovesMaxed ()) {
						controller.score.Reset ();
						ResetBoard ();
				}
				// RegenerateBoard (); 
		}

		public void RemoveObject (GridObject obj)
		{					
				bool wasAtTop = obj.IsAtTop ();
				objectsByColumn [ColForPoint (obj.Position ())].Remove (obj);
				Destroy (obj.gameObject);
				if (wasAtTop) {
						StartRegenerating ();
				}
		}

		virtual public void AddFalling (GridObject fallingObject)
		{
				fallingObjects.Add (fallingObject);
		}
	
		virtual public void RemoveFalling (GridObject fallingObject)
		{
				fallingObjects.Remove (fallingObject);
		}

		public Vector3 NextGridLocation (GridObject obj)
		{
				Vector3 point = obj.gameObject.transform.position;
				Vector2 nextLocation = (gravityDirection) + new Vector2(point.x, point.y);
			//	Debug.Log ("NextGridLocation (" + point + ") = " + nextLocation + " gravityDirection = " + gravityDirection + " offset = " + (gravityDirection * (unitHeight / 2.0f)));
				return nextLocation;
				//return NearestGridUnitCenter (new Vector3 (nextLocation.x, nextLocation.y, point.z));
		}

		public GridObject PreviousGridObject (GridObject obj)
		{
				List<GridObject> column = objectsByColumn [ColForPoint (obj.Position ())];
				int index = column.IndexOf (obj);
				if (index < column.Count () - 1) {
						//PreviousGridObject (column.ElementAt (index + 1));
						return column.ElementAt (index + 1);
				} else {
						return null;
				}
		}

		public Vector3 TopCellAt (float col)
		{
				string topCellsString = "";
				foreach (Vector3 point in topCells) {
					topCellsString = topCellsString + point.ToString() + " -> " + ColForPoint(point).ToString()+",";
				}
			//	Debug.Log ("TopCellAt(" + col + ") topCells: " + topCellsString + " = " + topCells.Find (point => ColForPoint (point) == col));
				return topCells.Find (point => ColForPoint(point) == col);
		}
	public Vector3 BottomCellAt (float col)
	{
		return Columns ().Find (column => ColForPoint(column.First ()) == col).First ();
	}

		public List<GridObject> AdjacentGridObjects (GridObject gridObject)
		{
				List<GridObject> adjacentGridObjects = new List<GridObject> ();
				foreach (Vector3 point in AdjacentGridLocations (gridObject.Position())) {
						GridObject adjacent = TryObjectAt (point);
						if (adjacent != null) {
								adjacentGridObjects.Add (adjacent);
						}
				}
				return adjacentGridObjects;
		}

		public List<GridObject> AllGridObjects ()
		{
				List<GridObject> list = new List<GridObject> ();
				foreach (KeyValuePair<float,List<GridObject>> col in objectsByColumn) {
						list.AddRange (col.Value);
				}
				return list;
		}

		public void Switch (GridObject objOne, GridObject objTwo)
		{
				Vector3 posOne = objOne.Position ();
				Vector3 posTwo = objTwo.Position ();
				float colOne = ColForPoint (posOne);
				float colTwo = ColForPoint (posTwo);
				int indOne = objectsByColumn [colOne].IndexOf (objOne);
				int indTwo = objectsByColumn [colTwo].IndexOf (objTwo);
				objectsByColumn [colOne] [indOne] = objTwo;
				objectsByColumn [colTwo] [indTwo] = objOne;
				if (!isRegenerating) {
						objOne.MoveTo (posTwo);
						objTwo.MoveTo (posOne);
				}
		}

		public void SetTopTo (int direction)
		{
				List<List<GridObject>> newObjectsByColumn = new List<List<GridObject>> ();
				foreach (List<Vector3> col in RowsByDirection().ElementAt(direction)) {
						List<GridObject> colOfObjects = new List<GridObject> ();
						newObjectsByColumn.Add (colOfObjects);
						foreach (Vector3 point in col) {
								colOfObjects.Add (ObjectAt (point));
						}
				}
				rotationIndex = direction;
				InitGravityDirection ();
				Dictionary<float, List<GridObject>> newObjectsByColumnDictionary = new Dictionary<float, List<GridObject>> ();
				foreach (List<GridObject> col in newObjectsByColumn) {
						newObjectsByColumnDictionary.Add (ColForPoint (col.First ().Position ()), col);
				}
				objectsByColumn = newObjectsByColumnDictionary;
				InitTopCells ();
		}

		public List<List<Vector3>> Columns() {
			return RowsByDirection ()[rotationIndex];
		}

		public Vector3 SquareBounded (Vector3 point)
		{
				float x = point.x;
				float y = point.y;
		
				// first put it within our bounds
				if (x > max.x) {
						x = max.x;
				}
				if (y > max.y) {
						y = max.y;
				}
				if (x < min.x) {
						x = min.x;
				}
				if (point.y < min.y) {
						y = min.y;
				}
				return new Vector3 (x, y, point.z);
		}

		public bool IsSquareBounded (Vector3 point)
		{
				return (point.y <= max.y && point.y >= min.y && point.x <= max.x && point.x >= min.x);
		}

		public GridObject TryObjectAt (Vector3 pos)
		{			
				Vector3 roundedPos = NearestGridUnitCenter (pos);
				GridObject gridObject = objectsByColumn [ColForPoint (pos)].Find (obj => Vector3.Distance (obj.Position (), roundedPos) < 0.25f);
				return gridObject;
		}

		public GridObject TryObjectNear (Vector3 pos)
		{
				// Debug.Log ("ColForPoint(" + pos + ") = " + ColForPoint (pos));
				GridObject gridObject = objectsByColumn [ColForPoint (pos)].Find (obj => Vector3.Distance (obj.transform.position, pos) < 0.75f);
				return gridObject;
		}
		
		public GridObject ObjectAt (Vector3 pos)
		{
				GridObject obj = TryObjectAt (pos);
				if (obj == null) {
						throw new System.NullReferenceException ("could not find Grid Object at point:" + pos);
				}
				return obj;
	
		}

		public  bool IsObjectAt (Vector3 pos)
		{
				return TryObjectAt (pos) != null;
		}

		public bool IsRegenerating ()
		{
				return isRegenerating;
		}
		// For rectangular grids. Returns the world location of the bottom left corner.
		virtual public Vector3 min {
				get {
						Vector3 position = gameObject.transform.position;
						return new Vector3 (position.x + (unitWidth / 2), position.y + (unitHeight / 2), position.z);
				}
		}


		// For rectangular grids. Returns the world location of the top right corner.
		virtual public Vector3 max {
				get {
						return new Vector3 (min.x + (unitWidth * (xUnits - 1)), min.y + (unitHeight * (yUnits - 1)), min.z);
				}
		}
	
		virtual public Vector3 topLeft {
				get {
						return new Vector3 (min.x, max.y, min.z);
				}
		}

		virtual public int xUnits {
				get {
						return units;
				}
		}

		virtual public int yUnits {
				get {
						return units;
				}
		}
	
		virtual public Vector3 bottomRight {
				get {
						return new Vector3 (max.x, min.y, min.z);
				}
		}

		virtual public float RoundToMultiple (float num, float multiple)
		{
				float offset = num % multiple;
				bool isRoundUp = ((Mathf.Abs (offset)) >= (multiple / 2.00f));
				bool isNegative = num < 0;
				if ((isRoundUp != isNegative) != isNegative) {
						int flip = 1;
						if (isNegative) {
								flip = -1;
						}
						return num + (flip * (multiple - Mathf.Abs (offset)));
				} else {
						return num - offset;
				}
		}

}
