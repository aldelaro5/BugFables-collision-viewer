//Initial script by Foulco
//Initial viable version : 2020-01-20
//
//INFORMATIONS
//Used to categorize the collision
//
//MODIFICATIONS
//2020-XX-XX    <Informations>
//

[System.Flags]
public enum CollisionViewerParam : int
{
  //All or None
  ALL = -1,
  NONE = 0,

  //Solid or Trigger
  SOLID = 1,
  TRIGGER = 2,

  //Shape
  CUBE = 4,
  CAPSULE = 8,
  CYLINDER = 16,
  POLYGON = 32,
  SPHERE = 64,

  //Category (Not Yet 100% Implemented)
  UNKNOWN_OR_DEFAULT_CATEGORY = 256,
  NEXT_LOADING_ZONE = 512,
  RESPAWN_LOCATION = 1024,    //Exemple : If previously touched this collision, if fall out of bounds, it will be respawned there instead of the start of the room 
  CUTSCENE = 2048,            //Exemple : Entering this collision => Start a cutscene
  CAMERA_MANIPULATION = 4096, //Exemple : Camera stop moving, Camera rotate "X" angle, etc
  FAKE_WALL = 8192,           //Exemple : To go fight Scarlet
  STEALTH_CHECK = 16384,      //Not sure for this one yet
}