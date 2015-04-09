using UnityEngine;
using System.Collections;

public class Switch {


	public Switch (){}
	public Switch(Switch other) {
		Trigger = other.Trigger;
		Inhibitor = other.Inhibitor;
	}

	public GridObject Trigger { get; set;}
	public GridObject Inhibitor { get; set;}


	public override bool Equals (object obj)
	{
		if (obj == null)
			return false;
		Switch objAsSwitch = obj as Switch;
		if (objAsSwitch == null)
			return false;
		else
			return Equals (objAsSwitch);
	}
	
	public bool Equals(Switch other) {
		return (Trigger.Equals (other.Trigger) && Inhibitor.Equals (other.Inhibitor)) || (Trigger.Equals (other.Inhibitor) && Inhibitor.Equals (other.Trigger));
	}

	public override int GetHashCode() {
		if (Trigger != null) {
			return (int)(Trigger.color.g * 10000);
		} else {
			return 0;
		}
	}

	public bool Contains(GridObject obj) {
		return Trigger == obj || Inhibitor == obj;
	}

	public Switch Duplicate() {
		return new Switch (this);
	}

	public override string ToString () {
		string desc = "a Switch: ";
		if (Trigger != null) {
			desc = desc +" Trigger: " +Trigger.Position();
		}
		
		if (Inhibitor != null) {
			desc = desc +" Inhibitor: " +Inhibitor.Position();
		}
		return desc;
	}

}
