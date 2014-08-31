@script AddComponentMenu ("Forces/Local/Trigger Spherical Force")
@script RequireComponent(Rigidbody)

var Force: Transform;
var ForceSpeed: float = 1;


function OnTriggerStay (other : Collider) { 
        
        
var GameObjects1WindForce = Force.transform.position - gameObject.transform.position;

GameObjects1WindForce = GameObjects1WindForce.normalized;


gameObject.rigidbody.AddForce (GameObjects1WindForce *ForceSpeed);



}