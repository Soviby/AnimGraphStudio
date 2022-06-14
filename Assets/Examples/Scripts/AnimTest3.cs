using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest3 : MonoBehaviour
    {
        public AnimComponent animComponent;

        public AnimationClip wakeUpClip;

        public AnimBlend2D moveBlend;

        public Rigidbody body;

        public float moveSpeed = 1.5f;

        bool wasMoving;
        Vector3 direction;

        void Awake()
        {
            var graph = animComponent.GetGraph();
            graph.Stop();
            graph.Play(wakeUpClip);
            graph.Evaluate();
        }

        void Update()
        {
            var graph = animComponent.GetGraph();

            direction = GetMovementDirection();

            var isMoving = direction != Vector3.zero;
            if (isMoving)
            {
                if (!wasMoving)
                {
                    wasMoving = true;

                    graph.Start();
                    var state = graph.CrossFade(wakeUpClip);
                    state.Speed = 1;
                    state.OnEnd = () => graph.CrossFade(moveBlend);
                }
            }
            else
            {
                if (wasMoving)
                {
                    wasMoving = false;

                    var state = graph.GetState(wakeUpClip);
                    var wasPlaying = state.IsPlaying;
                    state = graph.CrossFade(wakeUpClip);
                    state.Speed = -1;
                    state.OnEnd = () => graph.Stop();
                    if (!wasPlaying)
                        state.NormalizedTime = 1;
                }
            }

            var moveState = graph.GetState(moveBlend) as AnimBlend2DNode;
            if (isMoving && moveState != null && moveState.IsPlaying)
            {
                var eulerAngles = transform.eulerAngles; 
                var targetEulerY = Camera.main.transform.eulerAngles.y;
                eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetEulerY, 90 * Time.deltaTime);
                transform.eulerAngles = eulerAngles;

                moveState.Parameter = new Vector2(
                    Vector3.Dot(transform.right, direction), Vector3.Dot(transform.forward, direction));
                
                body.velocity = direction * moveSpeed;
            }
            else
            {
                if (moveState != null)
                    moveState.Parameter = Vector2.zero;
                body.velocity = Vector3.zero;
            }
        }

        Vector3 GetMovementDirection()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var raycastHit))
                return Vector3.zero;

            var dir = raycastHit.point - transform.position;
            dir.y = 0;
            var dist = dir.magnitude;
            if (dist < 0.05f)
                return Vector3.zero;
            
            return dir / dist;
        }

    }
}