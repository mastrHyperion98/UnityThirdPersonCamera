/*
 * Created By: mastrHyperion98
 * Created For: Unity 2019.4.x
 *
 * ThirdPersonCamera script that follows the player and avoid obstacles of the defined layer type.
 * Has a large variety of control over interpolation smoothness, camera speed, input sensitivity,
 * minimum and maximum angles and the ability to set a minimum distance that the camera can be from the target. 
 */
using UnityEngine;

namespace ThirdPerson {
    public class ThirdPersonCamera : MonoBehaviour {
        // Declare and properties
        [SerializeField] private Transform target;
        [SerializeField] private Transform lookAt;
        [SerializeField] private float cameraSpeed = 2.0f;
        [SerializeField] private float smooth = 10.0f;
        [SerializeField] private float minimumAngle = 0.0f;
        [SerializeField] private float maximumAngle = 60.0f;
        
        [SerializeField, Range(1,20)] private float distance = 1.0f;
        [SerializeField, Range(0,10)] private float minimumDistance = 0.0f;
        [SerializeField, Range(0.0f, 1.0f)] private float horizontalSensitivity = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float verticaSensitivity =1.0f;
        [SerializeField] private Camera camera;
        [SerializeField] private float clippingConstant = 2f;
        [SerializeField] private LayerMask collisionLayers;
        private bool isColliding = false;
        private float adjustedDistance;
        private Vector3[] clippingPoints;
        private float inputX, inputY;
        

        private void Start(){
            Cursor.visible = false;
            camera.transform.position = target.position - new Vector3(0, 0, distance);
            camera.transform.LookAt(lookAt);
            // 1 for each corner of the clipping plane and an additional one for the centre of the camera
            clippingPoints = new Vector3[5];
        }
         private void Update() {
             // Here we update the Input values
             UpdateInputData();
         }
         
         private void LateUpdate() {
              UpdateClippingPoints(); 
         }

         private void FixedUpdate() {
             // Perform the actual movement update in FixedUpdate
             UpdateCameraPosition();
         }
         private void UpdateInputData() {
             // TODO update for new input system
             inputX += Input.GetAxis("Mouse X") * horizontalSensitivity * cameraSpeed; 
             inputY -= Input.GetAxis("Mouse Y") * verticaSensitivity * cameraSpeed; 
             inputY = Mathf.Clamp(inputY, minimumAngle, maximumAngle);
         }
         private void UpdateCameraPosition() {
             var rotation = Quaternion.Euler(inputY, inputX, 0);
             // Compute the position offset and the new position from the rotation
             Vector3 positionOffset;
             
             // check for any collisions
             CheckCollision();
             if (isColliding) {
                 positionOffset = rotation * new Vector3(0,0,adjustedDistance);
             }
             else {
                 positionOffset = rotation * new Vector3(0, 0, distance);
             }
             
             Vector3 newPosition = target.position - positionOffset;
             
             // Linear Interpolation between old and new position
             camera.transform.position = Vector3.Lerp(transform.position, newPosition, smooth * Time.deltaTime);
             transform.LookAt(lookAt);
         }
         
         private void UpdateClippingPoints() {
             // compute variables
             float z = camera.nearClipPlane;
             float x = Mathf.Tan(camera.fieldOfView / clippingConstant) * z;
             float y = x / camera.aspect;

             Vector3 position = camera.transform.position;
             // Top-Left
             clippingPoints[0] = new Vector3(-x, y, -z) + position;
             // Top-Right
             clippingPoints[1] = new Vector3(x, y, -z) + position;
             // Lower-Left
             clippingPoints[2] = new Vector3(-x, -y, -z) + position;
             // Lower-Right
             clippingPoints[3] = new Vector3(x, -y, -z) + position;
             // Camera pos
             clippingPoints[4] = position - transform.forward;
         }
         
         private bool RayCastCollision() {
             float minDistance = -1;
             bool isColliding = false;
            
             foreach (var points in clippingPoints) {
                 var position = target.position;
                 Ray ray = new Ray(position, points - position);
                 RaycastHit hit;
                 if (Physics.Raycast(ray, out hit, distance, collisionLayers)) {
                     // Compute Minimum Distance
                     if (hit.transform.CompareTag("Player")) {
                         Debug.Log("HIT PLAYER");
                     }
                     if (minDistance == -1) {
                         minDistance = hit.distance;
                     }
                     else if (hit.distance < minDistance) {
                         minDistance =hit.distance;
                     }
                    
                     isColliding = true;
                 }
             }

             if (minDistance != -1) {
                 adjustedDistance = minDistance;
             } 
             
             if (adjustedDistance < minimumDistance - 0.005f) {
                 adjustedDistance = minimumDistance;
             }
             
             return  isColliding;
         }
         
         private void CheckCollision() {
             isColliding = RayCastCollision();
         }
    }
}

