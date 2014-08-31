@script AddComponentMenu ("Forces/Local/Trigger Directional Force")

var Force: Transform;
var ForceSpeed: float = 1;


function OnTriggerStay (other : Collider) { 
        

var MyObjectsForce : Vector3  = Force.transform.TransformDirection (0, 1, 0);      

//GameObjects

gameObject.rigidbody.AddForce (MyObjectsForce * ForceSpeed);

}