@script AddComponentMenu ("Forces 2D/Global/Spherical Force")
 
var Active: boolean = true;
var OnMouse: boolean = false;
var OnCollision: boolean = false;
var OnTrigger: boolean = false;
var ObjectsTagName: String;
var ForcePower: float = 1 ;
var ForceRange: float = 1;


function Update () {

if (Active == true){

ActiveForce();
}

if (OnMouse == true){

FunctionOnMouse();
}
}


function ActiveForce () {

if (Active == true){

var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

     
for (var i = 0; i < GameObjects.length; i++)
        {
        
          
 var range = Vector2.Distance( GameObjects[i].transform.position,this.transform.position );



       if ( range <= ForceRange ){  
       
var GameObjectsForce = this.transform.position - GameObjects[i].transform.position;

GameObjectsForce = GameObjectsForce.normalized;

GameObjects[i].rigidbody2D.AddForce (GameObjectsForce *ForcePower);

}
}
}
}

//OnMouse
function FunctionOnMouse(){


if(Input.GetMouseButton(1)){

if (OnMouse == true){

var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

     
for (var i = 0; i < GameObjects.length; i++)
        {
        
          
 var range = Vector2.Distance( GameObjects[i].transform.position,this.transform.position );



       if ( range <= ForceRange ){  
       
var GameObjectsForce = this.transform.position - GameObjects[i].transform.position;

GameObjectsForce = GameObjectsForce.normalized;

GameObjects[i].rigidbody2D.AddForce (GameObjectsForce *ForcePower);

}
}
}
}
}

//On Collision
function OnCollisionEnter2D(collision : Collision2D) {	
		
if (OnCollision == true){

var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

     
for (var i = 0; i < GameObjects.length; i++)
        {
        
          
 var range = Vector2.Distance( GameObjects[i].transform.position,this.transform.position );



       if ( range <= ForceRange ){  
       
var GameObjectsForce = this.transform.position - GameObjects[i].transform.position;

GameObjectsForce = GameObjectsForce.normalized;

GameObjects[i].rigidbody2D.AddForce (GameObjectsForce *ForcePower);

}
}
}
}

//On Trigger
function OnTriggerStay2D(collision : Collider2D) {	
		
if (OnTrigger == true){


var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

     
for (var i = 0; i < GameObjects.length; i++)
        {
        
          
 var range = Vector2.Distance( GameObjects[i].transform.position,this.transform.position );



       if ( range <= ForceRange ){  
       
var GameObjectsForce = this.transform.position - GameObjects[i].transform.position;

GameObjectsForce = GameObjectsForce.normalized;

GameObjects[i].rigidbody2D.AddForce (GameObjectsForce *ForcePower);

}
}
}
}

//DRAW GIZMOS

	function OnDrawGizmosSelected () {
		
		var Radius = ForceRange;
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, Radius);
	}