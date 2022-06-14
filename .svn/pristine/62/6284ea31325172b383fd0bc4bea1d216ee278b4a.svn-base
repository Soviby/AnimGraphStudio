using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    public class AnimGraph
    {
        static readonly int kLRUOldCapacity = 3;
        static readonly int kLRUYoungCapacity = 5;
        static readonly int kLRUOldThreshold = 2;
        static Queue<AnimNode> sNodeQueue = new Queue<AnimNode>();

        Animator animator;
        PlayableGraph playableGraph;
        AnimRootNode root;
        float deltaTime;
        internal readonly Dictionary<int, AnimStateNode> stateNodes = new Dictionary<int, AnimStateNode>();
        AnimStateNode youngHead, youngTail, oldHead, oldTail;
        internal int youngCount, oldCount;

        public string EditorName { get; set; }
        public AnimRootNode Root => root;
        public float DeltaTime => deltaTime;
        public PlayableGraph PlayableGraph => playableGraph;
        public Animator Animator => animator;

        public bool IsValid => playableGraph.IsValid();

        public bool IsPlaying
        {
            get => playableGraph.IsPlaying();
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        public AnimGraph(Animator animator)
        {
            playableGraph = PlayableGraph.Create();

            root = new AnimRootNode(this);

            SetOutput(animator);

#if UNITY_EDITOR
            Internal.AnimGraphCollector.Add(this);
            if (animator)
                EditorName = animator.gameObject.name;
#endif
        }

        void SetOutput(Animator animator)
        {
            this.animator = animator;

            var output = playableGraph.GetOutput(0);
            if (output.IsOutputValid())
                playableGraph.DestroyOutput(output);

            if (animator)
            {
                AnimationPlayableUtilities.Play(animator, root.Playable, playableGraph);
            }
        }

        public AnimLayerNode GetLayer(int index = 0)
        {
            return root.GetLayers().GetLayer(index);
        }

        public AnimStateNode Play(AnimStateNode node)
        {
            var layer = GetLayer();
            return layer.Play(node);
        }

        public AnimStateNode CrossFade(AnimStateNode node, float duration = 0.25f)
        {
            var layer = GetLayer();
            return layer.CrossFade(node, duration);
        }

        public AnimStateNode Play(Object content)
        {
            var layer = GetLayer();
            return layer.Play(content);
        }

        public AnimStateNode CrossFade(Object content, float duration = 0.25f)
        {
            var layer = GetLayer();
            return layer.CrossFade(content, duration);
        }

        public void Stop()
        {
            if (playableGraph.IsPlaying())
            {
                playableGraph.Stop();
            }
        }

        public void Start()
        {
            if (!playableGraph.IsPlaying())
            {
                playableGraph.Play();

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    Evaluate(Time.maximumDeltaTime);
#endif
            }
        }

        public void Evaluate()
        {
            playableGraph.Evaluate();
        }

        public void Evaluate(float deltaTime)
        {
            playableGraph.Evaluate(deltaTime);
        }

        public void Destroy()
        {
            animator = null;
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
                stateNodes.Clear();
#if UNITY_EDITOR
                Internal.AnimGraphCollector.Remove(this);
#endif
            }
        }

        public AnimStateNode GetState(Object content)
        {
            if (content == null)
                throw new System.Exception("State content is null");

            if (stateNodes.TryGetValue(content.GetInstanceID(), out var ret))
                return ret;
            return null;
        }

        public AnimStateNode GetOrCreateState(Object content)
        {
            var state = GetState(content);
            if (state == null)
            {
                if (content is AnimationClip clip)
                    state = new AnimClipNode(clip);
                else if (content is AnimBlend2D blend2D)
                    state = new AnimBlend2DNode(blend2D);
                else if (content is AnimBlend1D blend1D)
                    state = new AnimBlend1DNode(blend1D);
                else if (content is RuntimeAnimatorController controller)
                    state = new AnimControllerNode(controller);
                else
                    throw new System.Exception($"Unsupported state content: {content}");
            }
            return state;
        }

        static void LinkState(AnimStateNode node, ref AnimStateNode head, ref AnimStateNode tail, ref int count)
        {
            if (head == null)
            {
                node.next = null;
                node.prev = null;
                head = tail = node;
            }
            else
            {
                head.prev = node;
                node.next = head;
                node.prev = null;
                head = node;
            }

            count++;
        }

        static void UnlinkState(AnimStateNode node, ref AnimStateNode head, ref AnimStateNode tail, ref int count)
        {
            if (node.prev != null)
                node.prev.next = node.next;
            if (node.next != null)
                node.next.prev = node.prev;

            if (head == node)
                head = node.next;
            if (tail == node)
                tail = node.prev;

            node.prev = node.next = null;
            count--;
        }

        internal void AttachState(AnimStateNode node)
        {
            var attachCount = node.attachCount;

            if (attachCount > kLRUOldThreshold)
            {
                if (oldHead != node)
                {
                    UnlinkState(node, ref oldHead, ref oldTail, ref oldCount);
                    LinkState(node, ref oldHead, ref oldTail, ref oldCount);
                }
            }
            else if (attachCount == 0)
            {
                LinkState(node, ref youngHead, ref youngTail, ref youngCount);
                if (youngCount > kLRUYoungCapacity)
                    youngTail.Destroy();
            }
            else if (attachCount < kLRUOldThreshold)
            {
                if (youngHead != node)
                {
                    UnlinkState(node, ref youngHead, ref youngTail, ref youngCount);
                    LinkState(node, ref youngHead, ref youngTail, ref youngCount);
                }
            }
            else //if (attachCount == kLRUOldThreshold)
            {
                UnlinkState(node, ref youngHead, ref youngTail, ref youngCount);
                LinkState(node, ref oldHead, ref oldTail, ref oldCount);
                if (oldCount > kLRUOldCapacity)
                    oldTail.Destroy();
            }
        }

        void DetachState(AnimStateNode node)
        {
            var attachCount = node.attachCount;
            node.attachCount = 0;

            if (attachCount < kLRUOldThreshold)
            {
                UnlinkState(node, ref youngHead, ref youngTail, ref youngCount);
            }
            else
            {
                UnlinkState(node, ref oldHead, ref oldTail, ref oldCount);
            }
        }

        internal void RegisterState(AnimStateNode node)
        {
            if (node != null && !node.IsConstrained)
            {
                var content = node.GetContent();
                if (content)
                {
                    stateNodes[content.GetInstanceID()] = node;
                    AttachState(node);
                }
            }
        }

        internal void UnregisterState(AnimStateNode node)
        {
            if (node != null && !node.IsConstrained)
            {
                var content = node.GetContent();
                if (content)
                {
                    stateNodes.Remove(content.GetInstanceID());
                    DetachState(node);
                }
            }
        }

        void UpdateNodes(AnimNode.UpdateFlags flag)
        {
            var queue = sNodeQueue;
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Graph != this)
                    continue;
                if ((node.updateFlags & flag) != 0)
                    node.Update(flag);
                var children = node.Children;
                if (children != null)
                {
                    foreach (var c in children)
                    {
                        queue.Enqueue(c);
                    }
                }
            }
            queue.Clear();
        }

        internal void HandlePreUpdate(ref FrameData frameData)
        {
            deltaTime = frameData.deltaTime;
            UpdateNodes(AnimNode.UpdateFlags.Fade);
            UpdateNodes(AnimNode.UpdateFlags.BlendParam);
            UpdateNodes(AnimNode.UpdateFlags.Weight);
            UpdateNodes(AnimNode.UpdateFlags.Sync);
        }

        internal void HandlePostUpdate(ref FrameData frameData)
        {
            deltaTime = frameData.deltaTime;
            UpdateNodes(AnimNode.UpdateFlags.Event);
        }
    }

#if UNITY_EDITOR
    namespace Internal
    {
        public struct AnimGraphDebugInfo
        {
            public int stateCount;
            public int youngCount;
            public int oldCount;

            public static AnimGraphDebugInfo FromGraph(AnimGraph graph)
            {
                var info = new AnimGraphDebugInfo();
                info.stateCount = graph.stateNodes.Count;
                info.youngCount = graph.youngCount;
                info.oldCount = graph.oldCount;
                return info;
            }
        }

        public static class AnimGraphCollector
        {
            public static readonly List<AnimGraph> graphs = new List<AnimGraph>();

            internal static void Add(AnimGraph graph)
            {
                if (graphs.Contains(graph))
                    return;
                graphs.Add(graph);
            }

            internal static void Remove(AnimGraph graph)
            {
                graphs.Remove(graph);
            }
        }
    }
#endif
}