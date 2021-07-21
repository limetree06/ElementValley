
/*
 * Copyright (c) 2020 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace RW.MonumentValley
{
    // handles Player input and movement
    [RequireComponent(typeof(PlayerAnimation))]
    public class PlayerMover : MonoBehaviour
    {

        //  time to move one unit
        [Range(0.25f, 2f)]
        [SerializeField] private float moveTime = 0.5f;

        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private Graph graph;

        [SerializeField] private Node startNode;
        [SerializeField] private Node finishNode;

        [SerializeField] private List<Node> path;

        private GameObject button;
        private GameObject star;
        private GameObject mainCamera;
        private GameObject levelCamera;

        public UnityEvent buttonEvent;

        private Node currentNode;
        private Node nextNode;


        // flags
        private bool isMoving;
        private bool isControlEnabled;
        private bool isReachedButton;
        private bool isCameraMove;
        private int delay;
        private PlayerAnimation playerAnimation;

        

        private void Awake()
        {
            button = GameObject.FindGameObjectWithTag("Button");
            star = GameObject.FindGameObjectWithTag("Star");
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            levelCamera = GameObject.FindGameObjectWithTag("LevelCamera");

            pathfinder = FindObjectOfType<Pathfinder>();
            playerAnimation = GetComponent<PlayerAnimation>();

            if (pathfinder != null)
            {
                graph = pathfinder.GetComponent<Graph>();
            }

            isMoving = false;
            isControlEnabled = true;
            isReachedButton = false;
            isCameraMove = false;
            delay = 3;
        }

        private void Start()
        {
            // automatically set the Graph's StartNode 
            if (pathfinder != null && !pathfinder.SearchOnStart)
            {
                pathfinder.SetStartNode(transform.position);
            }
        }

        private void Update() {

            if(mainCamera.GetComponent<Camera>().orthographicSize > 4) {
                mainCamera.GetComponent<Camera>().orthographicSize = mainCamera.GetComponent<Camera>().orthographicSize - 0.75f * Time.deltaTime;
            }
            if(levelCamera.GetComponent<Camera>().orthographicSize > 4) {
                levelCamera.GetComponent<Camera>().orthographicSize = levelCamera.GetComponent<Camera>().orthographicSize - 0.75f * Time.deltaTime;
            }
            
            
            if(isReachedButton){
                button.transform.position = Vector3.MoveTowards(button.transform.position, new Vector3(9.75f, 7.5f, 0.65f), 0.5f * Time.deltaTime);
                star.transform.position = Vector3.MoveTowards(star.transform.position, new Vector3(11.3f, 7.55f, 0.65f), 0.3f * Time.deltaTime);
                this.gameObject.transform.Rotate(new Vector3(0, 150f * Time.deltaTime, 0));
                StartCoroutine(CameraRoutine(delay));
            }
        }

        private IEnumerator CameraRoutine(int delay)
        {
            yield return new WaitForSeconds(delay);
            mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, new Vector3(24f, 52f, -16f), 7.5f * Time.deltaTime);            
            levelCamera.transform.position = Vector3.MoveTowards(levelCamera.transform.position, new Vector3(24f, 52f, -16f), 7.5f * Time.deltaTime);            
            
            delay = 0;
        }

        public void OnStartMoving()
        {
            if (!isControlEnabled || pathfinder == null)
            {
                return;
            }

            StartCoroutine(FollowPathRoutine(path));

        }

        private IEnumerator FollowPathRoutine(List<Node> path)
        {
            // start moving
            isMoving = true;

            if (path == null || path.Count <= 1)
            {
                Debug.Log("PLAYERCONTROLLER FollowPathRoutine: invalid path");
            }
            else
            {
                UpdateAnimation();

                // loop through all Nodes
                for (int i = 0; i < path.Count; i++)
                {
                    // use the current Node as the next waypoint
                    nextNode = path[i];

                    // aim at the Node after that to minimize flipping
                    int nextAimIndex = Mathf.Clamp(i + 1, 0, path.Count - 1);
                    Node aimNode = path[nextAimIndex];
                    FaceNextPosition(transform.position, aimNode.transform.position);

                    // move to the next Node
                    yield return StartCoroutine(MoveToNodeRoutine(transform.position, nextNode));
                }
            }

            isMoving = false;
            UpdateAnimation();

        }

        //  lerp to another Node from current position
        private IEnumerator MoveToNodeRoutine(Vector3 startPosition, Node targetNode)
        {

            float elapsedTime = 0;

            // validate move time
            moveTime = Mathf.Clamp(moveTime, 0.1f, 5f);

            while (elapsedTime < moveTime && targetNode != null && !HasReachedNode(targetNode))
            {

                elapsedTime += Time.deltaTime;
                float lerpValue = Mathf.Clamp(elapsedTime / moveTime, 0f, 1f);

                Vector3 targetPos = targetNode.transform.position;
                transform.position = Vector3.Lerp(startPosition, targetPos, lerpValue);

                // if over halfway, change parent to next node
                if (lerpValue > 0.51f)
                {
                    transform.parent = targetNode.transform;
                    currentNode = targetNode;

                    // invoke UnityEvent associated with next Node
                    targetNode.gameEvent.Invoke();
                    //Debug.Log("invoked GameEvent from targetNode: " + targetNode.name);
                }

                // wait one frame
                yield return null;
            }
        }

        // turn face the next Node, always projected on a plane at the Player's feet
        public void FaceNextPosition(Vector3 startPosition, Vector3 nextPosition)
        {
            if (Camera.main == null)
            {
                return;
            }

            // convert next Node world space to screen space
            Vector3 nextPositionScreen = Camera.main.WorldToScreenPoint(nextPosition);

            // convert next Node screen point to Ray
            Ray rayToNextPosition = Camera.main.ScreenPointToRay(nextPositionScreen);

            // plane at player's feet
            Plane plane = new Plane(Vector3.up, startPosition);

            // distance from camera (used for projecting point onto plane)
            float cameraDistance = 0f;

            // project the nextNode onto the plane and face toward projected point
            if (plane.Raycast(rayToNextPosition, out cameraDistance))
            {
                Vector3 nextPositionOnPlane = rayToNextPosition.GetPoint(cameraDistance);
                Vector3 directionToNextNode = nextPositionOnPlane - startPosition;
                if (directionToNextNode != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToNextNode);
                }
            }
        }

        // toggle between Idle and Walk animations
        private void UpdateAnimation()
        {
            if (playerAnimation != null)
            {
                playerAnimation.ToggleAnimation(isMoving);
            }
        }

        // have we reached a specific Node?
        public bool HasReachedNode(Node node)
        {
            if (pathfinder == null || graph == null || node == null)
            {
                return false;
            }

            float distanceSqr = (node.transform.position - transform.position).sqrMagnitude;

            return (distanceSqr < 0.01f);
        }

        //  enable/disable controls
        public void EnableControls(bool state)
        {
            isControlEnabled = state;
        }

        public void ReachedButton(){
            if(isReachedButton == false) {
                buttonEvent.Invoke();
            }
            isReachedButton = true;
            
        }

    }
}