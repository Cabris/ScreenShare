@script AddComponentMenu ("Forces/Local/Mouse Spherical Force")
@script RequireComponent(Rigidbody)


var Force: Transform;
var ForceSpeed: float = 1;


function OnMouseDrag   (){
               
ActiveForce();

}



function ActiveForce(){

var GameObjectsWindForce = Force.transform.position - gameObject.transform.position;

GameObjectsWindForce = GameObjectsWindForce.normalized;

gameObject.rigidbody.AddForce (GameObjectsWindForce *ForceSpeed);
}