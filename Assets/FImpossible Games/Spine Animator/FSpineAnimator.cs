using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FSpine
{
    /// <summary>
    /// FM: Main component for spine-chain-follow procedural animation
    /// </summary>
    public class FSpineAnimator : MonoBehaviour
    {
        [Tooltip("Spine bones chain, always go from parent to children.")]
        public List<Transform> SpineTransforms;

        #region MAIN CALCULATIONS DATA CONTINERS 

        /// <summary> List of invisible for editor points which represents ghost animation for spine </summary>
        private List<FSpine_Point> proceduralPoints;
        private List<FSpine_Point> ProceduralReferencePose;

        /// <summary> Helping points to support connecting spine animator's with unity animator and for debug view </summary>
        private List<FSpine_Point> helperProceduralPoints;

        /// <summary> Remember initial distances between tail transforms for right placement between tail segments during animation (will be also used in next versions to support object scalling) </summary>
        private List<float> initialBoneDistances;

        #endregion

        [Tooltip("Blend between procedural and animator's keyframed animation", order = 0)]
        [Range(0f, 1f)]
        public float BlendToOriginal = 0f;

        [Tooltip("If your spine lead bone is in beggining of your hierarchy chain, toggle it. Component's gizmos can help you out to define which bone should be leading (head gizmo when you switch this toggle).")]
        public bool LastBoneLeading = true;
        private bool reversedChangeFlag = false;

        private Vector3 lookUp = Vector3.up;
        [Tooltip("Not yet figured out how to detect when rotations for other skeleton axis orientations set is going up/down instead of down/up. If you encounter hevaiour like that, toggle this.")]
        public bool InversedVerticalRotation = false;

        [Tooltip("If you want tail animator motion to be connected with keyframed animation motion, don't use this when your object isn't animated")]
        public bool ConnectWithAnimator = true;
        [Tooltip("In most cases it work just right with this option disabled, but in some situations it can give you better results when you enable animating leading bone")]
        public bool AnimateLeadingBone = false;
        [Tooltip("If animated leading bone translation should be added after spine motion - to use in case model is using position frames animation's grounding")]
        public bool LeadingAnimateAfterMotion = false;

        [Tooltip("If you using animate physics on animator you should set this variable enabled")]
        public bool PhysicalUpdate = false;

        [Tooltip("When animation should be hardly precise to main transform motion, untoggle it when you using it only on part of chractesr body, not on main movement transform")]
        public bool AnchorToThis = true;
        [Tooltip("Connecting root translation to given transform, useful when you use limb, then you should put here parent of first bone and probably enable QueueToLastUpdate")]
        public Transform AnchorRoot = null;
        [Tooltip("If you find out that debug tail pose have wrong rotation")]
        public Vector3 CustomAnchorRotationOffset = Vector3.zero;

        [Tooltip("Useful when you use few spine animators and want to rely on animated position and rotation by other spine animator")]
        public bool QueueToLastUpdate = false;

        [Tooltip("When your keyframed animation don't have keys on position track (common case) or you using not animated object with spine animator")]
        public bool PositionsNotAnimated = true;
        public List<bool> SelectivePosNotAnimated;
        [Tooltip("When your keyframed animation don't have keys on rotation track (rare case) or you using not animated object with spine animator")]
        public bool RotationsNotAnimated = false;
        public List<bool> SelectiveRotNotAnimated;

        #region MAIN CALCULATIONS VARIABLES

        /// <summary> Depends of bones' hierarchy structure, sometimes you will need leading bone in reversed position than default, this variable will help handling it in code </summary>
        private int leadingBoneIndex;

        /// <summary> Helper variable to reverse some variables when using reversed spine feature </summary>
        private int reverser = 1;
        private int adjuster = 1;

        /// <summary> Initial or frame freeze coordinates for bones </summary>
        private List<FSpine_Point> staticCoordinates;

        /// <summary> Variables which are detected at Init() and allowing to configure tail follow rotations to look correctly independently of how bones rotations orientations are set </summary>
        private FSpine_FixingSet spineLookDirectionsSet;
        [Tooltip("Advanced option, when some of your spine bones going in opposite way that they should, you can adjust it here sometimes")]
        public List<Vector3> lookDirectionsAdditiveCorrection;

        #endregion

        [Tooltip("Component is made to work universally on many sets of skeletons, but there can exist small offsets which you can correct using this variables")]
        public List<Vector3> ManualRotationOffsets;
        [Tooltip("Component is made to work universally on many sets of skeletons, but there can exist small offsets which you can correct using this variables")]
        public List<Vector3> ManualPositionOffsets;

        [Tooltip("When start we doing precise calculations for fixing rotations, but in most cases rounding this values is doing job better")]
        public bool RoundCorrection = true;
        private bool wasRoundCorrection = false;
        [Tooltip("Using only one axis correction direction for all segments")]
        public bool UnifyCorrection = true;
        private bool wasUnified = true;

        [Tooltip("Often when you drop model to scene, it's initial pose is much different than animations, which causes problems, this toggle solves it at start")]
        public bool StartAfterTPose = true;


        #region Extra helper variables

        /// <summary> Initial rotation of base transform, important to calculating hard static referene poses </summary>
        private Vector3 previousScale;

        /// <summary> Generated transforms to help connecting motion with animator and support model scalling </summary>
        private Transform[] anchorHelpers;
        private Transform anchorsContainer;

        /// <summary> Flag to detect if we was animating object with full blend to source animation </summary>
        private bool wasSourceAnimation = false;

        /// <summary> Flag to define if component was initialized already for more controll </summary>
        private bool initialized = false;

        /// <summary> Variable to calculate difference in last frame position to current, needed for some straigtening calculations when bone is in move</summary>
        private Vector3 previousPos;

        #endregion

        #region Animation settings and limitations

        [Range(0f, 1f)]
        [Tooltip("If animation of changing segments position should be smoothed - creating a little gumy effect")]
        public float PosSmoother = 0f;
        [Range(0f, 1f)]
        [Tooltip("If animation of changing segments rotation should be smoothed - making it more soft, but don't overuse it")]
        public float RotSmoother = 0f;
        [Range(5f, 180f)]
        [Tooltip("Limiting rotation angle difference between each segment of spine but consider using StraightenSpeed variable for smoother effect")]
        public float AngleLimit = 90f;
        [Range(0f, 1f)]
        [Tooltip("Smoothing how fast limiting should make segments go back to marginal pose")]
        public float LimitSmoother = .35f;
        [Range(0f, 15f)]
        [Tooltip("How fast spine should be rotated to straight pose when it moves, higher angle limit - straigtening should be lower (behave different than GoBackSpeed)")]
        public float StraightenSpeed = 3.5f;
        public bool TurboStraighten = false;

        [Tooltip("Spine going back to straight position with choosed speed intensity")]
        [Range(0f, 1f)]
        public float GoBackSpeed = 0f;

        // V1.0.1
        [Tooltip("Sometimes offsetting model's pivot position gives better results using spine animator, offset forward axis so front legs are in centrum and see the difference (generating additional transform inside hierarchy)")]
        public Vector3 MainPivotOffset = new Vector3(0f, 0f, 0f);
        [HideInInspector] // As public because there was troubles with loosing reference when entering playmode and going back
        public Transform mainPivotOffsetTransform;

        [Tooltip("<! Most models can not need this !> Offset for bones rotations, thanks to that animation is able to rotate to segments in a correct way, like from center of mass")]
        public Vector3 SegmentsPivotOffset = new Vector3(0f, 0f, 0f);

        [Tooltip("Multiplies distance value between bones segments - can be useful for use with humanoid skeletons")]
        public float DistancesMultiplier = 1f;

        #endregion

        #region Incorrection detection
#if UNITY_EDITOR
        [HideInInspector]
        public bool wasIncorrectRemind = false;
        [HideInInspector]
        public bool incorrectionWarning = false;
        private int incorrectionCounter = 5;
        private float incorrectionSum = 0f;
        private float incorrectionSumPos = 0f;
        private Quaternion preIncorrect = Quaternion.identity;
#endif

        #endregion


        #region Initialization methods

        void Init()
        {
            if (initialized) return;

            // Getting bones transforms which will be animated by component
            ConfigureBonesTransforms();

            // Generating animation points for ghost translations
            PrepareSpinePoints();

            // Computing variables needed to hold motion
            ComputePredefinedVariables();

            // Flag for refresging variables if we do changes in playmode for tweaking
            reversedChangeFlag = LastBoneLeading;

            previousScale = transform.localScale;

            initialized = true;

            // Straightening spine pose to desired positions and rotations
            ReposeSpine();

            SpineMotion();
        }


        /// <summary>
        /// Precomputing spine look directions for more correct rotations of bones
        /// </summary>
        protected void ComputePredefinedVariables()
        {
            staticCoordinates = new List<FSpine_Point>();

            for (int i = 0; i < SpineTransforms.Count; i++)
            {
                staticCoordinates.Add(new FSpine_Point { Position = SpineTransforms[i].localPosition, Rotation = SpineTransforms[i].localRotation });

                FSpine_Point p = new FSpine_Point();
                p.Position = SpineTransforms[i].position - transform.position;
                p.Rotation = transform.rotation * Quaternion.Inverse(SpineTransforms[i].rotation);
            }

            RefreshDistances();

            spineLookDirectionsSet = new FSpine_FixingSet().Init();
            int c = SpineTransforms.Count - 1;

            for (int i = 0; i < SpineTransforms.Count; i++)
            {
                if (i != SpineTransforms.Count - 1)
                {
                    spineLookDirectionsSet.AddToAllNormal(
                        (SpineTransforms[i].InverseTransformPoint(SpineTransforms[i + 1].position) - SpineTransforms[i].InverseTransformPoint(SpineTransforms[i].position)).normalized);

                    spineLookDirectionsSet.Reversed.Add(
                        (SpineTransforms[i].InverseTransformPoint(SpineTransforms[i].position) - SpineTransforms[i].InverseTransformPoint(SpineTransforms[i + 1].position)).normalized);
                }
            }

            RefreshManualPosOffs();
            RefreshManualRotOffs();
            RefreshSelectivePosNotAnimated();
            RefreshSelectiveRotNotAnimated();

            spineLookDirectionsSet.AddToAllNormal(
                (SpineTransforms[c].InverseTransformPoint(SpineTransforms[c].position) - SpineTransforms[c].InverseTransformPoint(SpineTransforms[c - 1].position)).normalized);

            spineLookDirectionsSet.Reversed.Add(
                (SpineTransforms[c].InverseTransformPoint(SpineTransforms[c - 1].position) - SpineTransforms[c].InverseTransformPoint(SpineTransforms[c].position)).normalized);

            // Look directions defined
            for (int i = 0; i < spineLookDirectionsSet.Initial.Count; i++)
            {
                spineLookDirectionsSet.Rounded[i] = RoundToBiggestValue(spineLookDirectionsSet.Initial[i]);
                spineLookDirectionsSet.RoundedReversed[i] = RoundToBiggestValue(spineLookDirectionsSet.Reversed[i]);
            }

            lookDirectionsAdditiveCorrection = new List<Vector3>();

            for (int i = 0; i < SpineTransforms.Count; i++) lookDirectionsAdditiveCorrection.Add(Vector3.zero);
        }


        /// <summary>
        /// Generating ghost points for animating spine segments
        /// </summary>
        protected virtual void PrepareSpinePoints()
        {
            helperProceduralPoints = new List<FSpine_Point>();
            proceduralPoints = new List<FSpine_Point>();
            ProceduralReferencePose = new List<FSpine_Point>();

            for (int i = 0; i < SpineTransforms.Count; i++)
            {
                helperProceduralPoints.Add(new FSpine_Point
                {
                    Position = SpineTransforms[i].position,
                    Rotation = SpineTransforms[i].rotation
                });

                proceduralPoints.Add(new FSpine_Point
                {
                    Position = SpineTransforms[i].position,
                    Rotation = SpineTransforms[i].rotation
                });

                ProceduralReferencePose.Add(new FSpine_Point
                {
                    Position = proceduralPoints[i].Position,
                    Rotation = proceduralPoints[i].Rotation
                });
            }

            anchorsContainer = new GameObject(name + "-SpineAnimator-AnchorsContainer").transform;
            anchorsContainer.SetParent(transform, true);
            anchorsContainer.localPosition = Vector3.zero;
            anchorsContainer.localRotation = Quaternion.identity;
            anchorsContainer.localScale = Vector3.one;

            anchorHelpers = new Transform[SpineTransforms.Count];
            for (int i = 0; i < SpineTransforms.Count; i++)
            {
                Transform anchorHelper;
                anchorHelper = new GameObject(name + "-Spine Helper [" + i + "] - " + SpineTransforms[i].name).transform;
                anchorHelper.localScale = SpineTransforms[i].lossyScale;
                anchorHelper.SetParent(anchorsContainer, true);
                anchorHelper.position = SpineTransforms[i].position;
                anchorHelper.rotation = SpineTransforms[i].rotation;
                anchorHelpers[i] = anchorHelper;
            }
        }


        /// <summary>
        /// Auto collect spine transforms if they're not defined from inspector
        /// also this is place for override and configure more
        /// </summary>
        protected virtual void ConfigureBonesTransforms()
        {
            if (SpineTransforms == null) SpineTransforms = new List<Transform>();

            if (SpineTransforms.Count < 2)
            {
                Transform lastParent = transform;

                bool boneDefined = true;

                if (SpineTransforms.Count == 0)
                {
                    boneDefined = false;
                    lastParent = transform;
                }
                else lastParent = SpineTransforms[0];

                Transform rootTransform = lastParent;

                // 100 iterations because I am scared of while() loops :O so limit to 100 or 1000 if anyone would ever need
                for (int i = SpineTransforms.Count; i < 100; i++)
                {
                    if (boneDefined)
                        if (lastParent == rootTransform)
                        {
                            lastParent = lastParent.GetChild(0);
                            continue;
                        }

                    SpineTransforms.Add(lastParent);

                    if (lastParent.childCount > 0) lastParent = lastParent.GetChild(0); else break;
                }
            }
        }

        void OnEnable()
        {
            if (PhysicalUpdate)
                StartCoroutine("LateFixedUpdate");
        }

        void OnDisable()
        {
            StopCoroutine("LateFixedUpdate");
        }

        #endregion

        #region After initial helper methods

        /// <summary>
        /// Method to initialize component, to have more controll than waiting for Start() method, init can be executed before or after start, as programmer need it.
        /// </summary>
        protected void Start()
        {
            if (QueueToLastUpdate)
            {
                enabled = false;
                enabled = true;
            }

            if (!StartAfterTPose) Init(); else StartCoroutine(InitTPoseStartOffset());
        }

        /// <summary>
        /// Skipping few first frames to reference static poses not from TPose but from first played animation frame (in most cases important)
        /// </summary>
        private IEnumerator InitTPoseStartOffset()
        {
            int counter = 1;
            while (counter > -5)
            {
                if (!initialized)
                    if (StartAfterTPose)
                    {
                        counter--;
                        if (counter < -1) Init();
                        yield return null;
                    }
                    else
                    {
                        counter--;
                        if (counter == -3) ReposeSpine();
                    }

                yield return null;
            }
        }

        /// <summary>
        /// Updating pointers for reversed and basic spine lead direction
        /// </summary>
        private void UpdateReverseHelpVariables()
        {
            if (LastBoneLeading)
            {
                leadingBoneIndex = SpineTransforms.Count - 1;
                reverser = -1;
                adjuster = 1;
            }
            else
            {
                leadingBoneIndex = 0;
                reverser = 1;
                adjuster = 0;
            }
        }

        /// <summary>
        /// Restraightening spine motion pose
        /// </summary>
        private void ReposeSpine()
        {
            UpdateReverseHelpVariables();

            RefreshDifferenceReference();

            for (int i = 1 - adjuster; i < proceduralPoints.Count - adjuster; i++)
            {
                proceduralPoints[i].Position = ProceduralReferencePose[i].Position;
                proceduralPoints[i].Rotation = ProceduralReferencePose[i].Rotation;
            }

            proceduralPoints[leadingBoneIndex] = GetLeadingBoneCoordinates();
        }

        #endregion

        private void LateUpdate()
        {
            if (!PhysicalUpdate) SpineMotion();
        }

        IEnumerator LateFixedUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                SpineMotion();
            }
        }

        private void SpineMotion()
        {
#if UNITY_EDITOR
            if (DrawDebug) if (Input.GetKey(KeyCode.BackQuote)) return; // Turning off component motion for debug purposes using "~" key
#endif

            if (!initialized) return;

            if (SpineTransforms.Count == 0)
            {
                Debug.LogError("No spine bones defined in " + name + " !");
                return;
            }

            // Switching current correction list for calculations
            RefreshRefDirsOnReverse();

            #region Blending and animator definition

            Vector3 leadingOrigPos = SpineTransforms[leadingBoneIndex].position;
            Quaternion leadingOrigRot = SpineTransforms[leadingBoneIndex].rotation;

            if (ConnectWithAnimator)
            {
                if (PositionsNotAnimated)
                    for (int i = 0; i < SpineTransforms.Count; i++)
                        if (SelectivePosNotAnimated[i])
                            SpineTransforms[i].localPosition = staticCoordinates[i].Position;

                if (RotationsNotAnimated)
                    for (int i = 0; i < SpineTransforms.Count; i++)
                        if (SelectiveRotNotAnimated[i])
                            SpineTransforms[i].localRotation = staticCoordinates[i].Rotation;
            }
            else
            {
                for (int i = 0; i < SpineTransforms.Count; i++)
                {
                    SpineTransforms[i].localPosition = staticCoordinates[i].Position;
                    SpineTransforms[i].localRotation = staticCoordinates[i].Rotation;
                }
            }

            // No spine animator blend, just keyframed animation
            if (BlendToOriginal >= 1f)
            {
                for (int i = 0; i < SpineTransforms.Count; i++)
                {
                    helperProceduralPoints[i].Position = SpineTransforms[i].position;
                    helperProceduralPoints[i].Rotation = SpineTransforms[i].rotation;
                }

                wasSourceAnimation = true;

                // Returning so nothing more is animated
                return;
            }


            // Update on change for this variables
            if (reversedChangeFlag != LastBoneLeading) wasRoundCorrection = !RoundCorrection;
            if (wasUnified != UnifyCorrection) wasRoundCorrection = !RoundCorrection;

            // If we switched back from full blend to source animation we repose spine locomotion
            if (wasSourceAnimation)
            {
                ReposeSpine();
                wasSourceAnimation = false;
            }

            UpdateReverseHelpVariables();

            #endregion

            // Updating distance variables only when main object is rescaled
            if (previousScale != transform.localScale) RefreshDistances();

            // Leading bone positioning and rotation
            proceduralPoints[leadingBoneIndex] = GetLeadingBoneCoordinates();

            // Main calculations for spine animation
            CalculateMotion();


            // Calculating difference values in reference to static pose
            FSpine_Point[] diffs = new FSpine_Point[SpineTransforms.Count];
            Vector3[] eulDiffs = new Vector3[diffs.Length];

            RefreshDifferenceReference();
            for (int i = 0; i < SpineTransforms.Count; i++)
            {
                diffs[i] = new FSpine_Point();
                diffs[i].Position = proceduralPoints[i].Position - ProceduralReferencePose[i].Position;

                // Going to euler is solving here some problems with rotating in reference to base transorm's rotation
                diffs[i].Rotation = proceduralPoints[i].Rotation * Quaternion.Inverse(ProceduralReferencePose[i].Rotation);

                // Converting axis orientation to skeleton space rotation
                Quaternion fromTo;

                fromTo = Quaternion.FromToRotation((spineLookDirectionsSet.Current[i] + lookDirectionsAdditiveCorrection[i]) * (InversedVerticalRotation ? -1f : 1f), Vector3.forward);

                eulDiffs[i] = (proceduralPoints[i].Rotation * fromTo).eulerAngles - (ProceduralReferencePose[i].Rotation * fromTo).eulerAngles;

                diffs[i].Rotation = Quaternion.Euler(eulDiffs[i]);
            }

            // Applying variables to helper list
            for (int i = 1 - adjuster; i < proceduralPoints.Count - adjuster; i++)
            {
                helperProceduralPoints[i].Position = SpineTransforms[i].position + diffs[i].Position;
                helperProceduralPoints[i].Rotation = Quaternion.Euler(SpineTransforms[i].eulerAngles + eulDiffs[i]);
            }

            // Giving possibility to manual correction for bones rotations and positions in spine chain
            for (int i = 1 - adjuster; i < proceduralPoints.Count - adjuster; i++)
            {
                helperProceduralPoints[i].Position += helperProceduralPoints[i].TransformDirection(ManualPositionOffsets[i]);
                helperProceduralPoints[i].Position += helperProceduralPoints[i].TransformDirection(SegmentsPivotOffset * (initialBoneDistances[i] * DistancesMultiplier));
                helperProceduralPoints[i].Rotation *= Quaternion.Euler(ManualRotationOffsets[i]);
            }

            // Applying fixing parameters to leading bone
            if (!AnimateLeadingBone)
                helperProceduralPoints[leadingBoneIndex].Position = SpineTransforms[leadingBoneIndex].position + helperProceduralPoints[leadingBoneIndex].TransformDirection(ManualPositionOffsets[leadingBoneIndex]);
            else
                helperProceduralPoints[leadingBoneIndex].Position = proceduralPoints[leadingBoneIndex].Position + helperProceduralPoints[leadingBoneIndex].TransformDirection(ManualPositionOffsets[leadingBoneIndex]);

            helperProceduralPoints[leadingBoneIndex].Position += helperProceduralPoints[leadingBoneIndex].TransformDirection(SegmentsPivotOffset * (initialBoneDistances[leadingBoneIndex] * DistancesMultiplier));
            helperProceduralPoints[leadingBoneIndex].Rotation = SpineTransforms[leadingBoneIndex].rotation * Quaternion.Euler(ManualRotationOffsets[leadingBoneIndex]);

            SpineTransforms[leadingBoneIndex].position = Vector3.Lerp(helperProceduralPoints[leadingBoneIndex].Position, SpineTransforms[leadingBoneIndex].position, BlendToOriginal);
            SpineTransforms[leadingBoneIndex].rotation = Quaternion.Slerp(helperProceduralPoints[leadingBoneIndex].Rotation, SpineTransforms[leadingBoneIndex].rotation, BlendToOriginal);

            for (int i = 1 - adjuster; i < proceduralPoints.Count - adjuster; i++)
            {
                SpineTransforms[i].position = Vector3.Lerp(helperProceduralPoints[i].Position, SpineTransforms[i].position, BlendToOriginal);
                SpineTransforms[i].rotation = Quaternion.Slerp(helperProceduralPoints[i].Rotation, SpineTransforms[i].rotation, BlendToOriginal);
            }

            if (AnimateLeadingBone)
                if (LeadingAnimateAfterMotion)
                {
                    SpineTransforms[leadingBoneIndex].position = leadingOrigPos;
                    SpineTransforms[leadingBoneIndex].rotation = leadingOrigRot;
                }


#if UNITY_EDITOR
            // Detecting if something wrong is going on with animator in playmode -> one time for each added component if something wrong occurs
            if (!wasIncorrectRemind)
            {
                if (incorrectionCounter > 0)
                {
                    if (previousPos != RoundPosDiff(proceduralPoints[leadingBoneIndex].Position)) incorrectionCounter--;
                }
                else
                {
                    if (incorrectionCounter > -30)
                    {
                        int ind = Mathf.Max(0, SpineTransforms.Count / 4);
                        if (incorrectionCounter != 0)
                        {
                            incorrectionSum += Quaternion.Angle(preIncorrect, SpineTransforms[ind].localRotation);
                            for (int i = 0; i < SpineTransforms.Count; i++) incorrectionSumPos += Vector3.Distance(SpineTransforms[i].localPosition, staticCoordinates[i].Position);
                        }

                        preIncorrect = SpineTransforms[ind].localRotation;
                        incorrectionCounter--;
                    }
                    else
                    {
                        if (incorrectionCounter != -100)
                        {
                            if (incorrectionSum > 750 || incorrectionSumPos > (55 * (Mathf.Max(1f, SpineTransforms.Count / 4)) ) )
                            {
                                incorrectionWarning = true;
                                Debug.LogWarning("[Spine Animator] There is something wrong going on with your bones in " + name + ". Check now inspector window of SpineAnimator then exit playmode. (" + incorrectionSum + " : " + incorrectionSumPos + ")");
                            }

                            incorrectionCounter = -100;
                        }
                    }
                }
            }
#endif

            previousPos = RoundPosDiff(proceduralPoints[leadingBoneIndex].Position);
            previousScale = transform.localScale;

        }

        /// <summary>
        /// Refreshing position and rotation of leading bone according to settings
        /// </summary>
        private FSpine_Point GetLeadingBoneCoordinates()
        {
            FSpine_Point newPoint = new FSpine_Point();

            if (AnchorToThis)
            {
                if (!AnimateLeadingBone)
                {
                    newPoint.Position = anchorHelpers[leadingBoneIndex].position;
                    newPoint.Rotation = transform.rotation;
                }
                else
                {
                    if (!LeadingAnimateAfterMotion)
                    {
                        newPoint.Position = SpineTransforms[leadingBoneIndex].position;
                        newPoint.Rotation = SpineTransforms[leadingBoneIndex].rotation;
                        newPoint.Rotation = transform.rotation;
                    }
                    else
                    {
                        newPoint.Position = anchorHelpers[leadingBoneIndex].position;
                        newPoint.Rotation = transform.rotation;
                    }
                }
            }
            else
            {
                if (AnchorRoot)
                {
                    newPoint.Position = AnchorRoot.position + AnchorRoot.TransformDirection(staticCoordinates[leadingBoneIndex].Position);
                    newPoint.Rotation = AnchorRoot.rotation * Quaternion.FromToRotation(spineLookDirectionsSet.Current[leadingBoneIndex], Vector3.forward);
                    newPoint.Rotation *= Quaternion.Euler(CustomAnchorRotationOffset);
                }
                else
                {
                    Vector3 prePos = SpineTransforms[leadingBoneIndex].localPosition;
                    Quaternion preRot = SpineTransforms[leadingBoneIndex].localRotation;

                    SpineTransforms[leadingBoneIndex].localPosition = staticCoordinates[leadingBoneIndex].Position;
                    newPoint.Position = SpineTransforms[leadingBoneIndex].position;

                    SpineTransforms[leadingBoneIndex].localRotation = staticCoordinates[leadingBoneIndex].Rotation;
                    newPoint.Rotation = transform.rotation;
                    newPoint.Rotation *= Quaternion.Euler(CustomAnchorRotationOffset);

                    SpineTransforms[leadingBoneIndex].localPosition = prePos;
                    SpineTransforms[leadingBoneIndex].localRotation = preRot;
                }
            }

            return newPoint;
        }

        public Transform rotationRef;

        /// <summary>
        /// Calculating spine-like movement animation logic for given transforms list
        /// </summary>
        protected virtual void CalculateMotion()
        {
            if (LastBoneLeading)
            {
                for (int i = proceduralPoints.Count - 2; i >= 0; i--)
                {
                    CalculateSpineBehaviourRotation(i);
                    CalculateSpineBehaviourPosition(i);
                }
            }
            else
            {
                for (int i = 1; i < proceduralPoints.Count; i++)
                {
                    CalculateSpineBehaviourRotation(i);
                    CalculateSpineBehaviourPosition(i);
                }
            }
        }


        /// <summary>
        /// Calculating position for single segment
        /// </summary>
        private void CalculateSpineBehaviourPosition(int index)
        {
            Vector3 targetPosition = (proceduralPoints[index - reverser].Position) - (proceduralPoints[index].TransformDirection(Vector3.forward) * (initialBoneDistances[index] * DistancesMultiplier));

            if (PosSmoother == 0f)
                proceduralPoints[index].Position = targetPosition;
            else
                proceduralPoints[index].Position = Vector3.Lerp(proceduralPoints[index].Position, targetPosition, Time.smoothDeltaTime * SmootherValue(PosSmoother));
        }


        /// <summary>
        /// Calculating rotation with limitations and stuff for single segment
        /// </summary>
        private void CalculateSpineBehaviourRotation(int index)
        {
            FSpine_Point otherSpinePoint = proceduralPoints[index - reverser];
            FSpine_Point currentSpinePoint = proceduralPoints[index];

            Quaternion targetLookRotation = Quaternion.LookRotation(proceduralPoints[index - reverser].Position - proceduralPoints[index].Position, proceduralPoints[index - reverser].TransformDirection(lookUp));

            #region Calculations to limit rotations in order to make custom animation behaviour

            // Difference in rotation between spine segments
            float angleDiff = Quaternion.Angle(currentSpinePoint.Rotation, otherSpinePoint.Rotation);

            // Limiting rotation to correct state with elastic range
            if (angleDiff > AngleLimit * 0.75f)
            {
                Quaternion limitRange = Quaternion.Slerp(targetLookRotation, otherSpinePoint.Rotation, (angleDiff - AngleLimit) / (AngleLimit));

                float elasticPush = Mathf.Min(1f, angleDiff / (AngleLimit / 0.75f));
                elasticPush = Mathf.Sqrt(Mathf.Pow(elasticPush, 4)) * elasticPush; // sqrt and power will make this value increase slower but reaching 1f at the end

                if (LimitSmoother == 0f)
                    targetLookRotation = Quaternion.Slerp(targetLookRotation, limitRange, elasticPush);
                else
                    targetLookRotation = Quaternion.Slerp(targetLookRotation, limitRange, Time.smoothDeltaTime * SmootherValue(LimitSmoother) * elasticPush);
            }

            if (GoBackSpeed <= 0f)
            {
                // When position in previous frame was different, we straigtening a little rotation of spine
                if (StraightenSpeed > 0f)
                    if (previousPos != RoundPosDiff(proceduralPoints[leadingBoneIndex].Position))
                        targetLookRotation = Quaternion.Lerp(targetLookRotation, otherSpinePoint.Rotation, Time.smoothDeltaTime * StraightenSpeed * (TurboStraighten ? 6f : 1f));
            }
            else // When we set GoBackSpeed variable spine is going back to straight pose continously
            {
                // If we use straigtening at the same time when using GoBack variable
                float straightenVal = 0f;
                if (previousPos != RoundPosDiff(proceduralPoints[leadingBoneIndex].Position)) straightenVal = StraightenSpeed * (TurboStraighten ? 6f : 1f);

                targetLookRotation = Quaternion.Lerp(targetLookRotation, otherSpinePoint.Rotation, Time.smoothDeltaTime * (Mathf.Lerp(0f, 40f, GoBackSpeed) + straightenVal));
            }

            #endregion

            // If we want some smooth motion for follower
            if (RotSmoother == 0f)
                currentSpinePoint.Rotation = targetLookRotation;
            else
                currentSpinePoint.Rotation = Quaternion.Slerp(currentSpinePoint.Rotation, targetLookRotation, Time.smoothDeltaTime * SmootherValue(RotSmoother));
        }


        #region Helpers

        /// <summary>
        /// Assigning correction lists for calculations, only executed when change on flag occurs
        /// </summary>
        private void RefreshRefDirsOnReverse()
        {
            if (wasRoundCorrection != RoundCorrection)
            {
                //if (RolledBones) lookUp = Vector3.up * -1f; else lookUp = Vector3.up;

                wasUnified = UnifyCorrection;

                if (RoundCorrection)
                {
                    if (!LastBoneLeading)
                        spineLookDirectionsSet.Current = spineLookDirectionsSet.RoundedReversed;
                    else
                        spineLookDirectionsSet.Current = spineLookDirectionsSet.Rounded;
                }
                else
                {
                    if (!LastBoneLeading)
                        spineLookDirectionsSet.Current = spineLookDirectionsSet.Reversed;
                    else
                        spineLookDirectionsSet.Current = spineLookDirectionsSet.Initial;
                }

                if (UnifyCorrection)
                {
                    spineLookDirectionsSet.Current = spineLookDirectionsSet.SetUnified(spineLookDirectionsSet.Current);
                }

                wasRoundCorrection = RoundCorrection;
            }
        }

        /// <summary>
        /// Supporting scaling in update
        /// </summary>
        private void RefreshDistances()
        {
            initialBoneDistances = new List<float>();

            int c = SpineTransforms.Count - 1;

            for (int i = 0; i < SpineTransforms.Count - 1; i++)
            {
                initialBoneDistances.Add(Vector3.Distance(anchorHelpers[i].position, anchorHelpers[i + 1].position));
            }

            // Adding last variable in different way
            initialBoneDistances.Add(Vector3.Distance(anchorHelpers[c - 1].transform.position, anchorHelpers[c].transform.position));
        }

        /// <summary>
        /// Helper class to animate spine bones
        /// </summary>
        [System.Serializable]
        public class FSpine_Point
        {
            public Vector3 Position = Vector3.zero;
            public Quaternion Rotation = Quaternion.identity;

            public Vector3 TransformDirection(Vector3 dir)
            {
                return Rotation * dir;
            }
        }

        /// <summary>
        /// Making translations more smooth for more elastic effect
        /// </summary>
        private float SmootherValue(float val)
        {
            return Mathf.Lerp(60f, 0.1f, val);
        }

        /// <summary>
        /// Helper class to hold some calculation variables more learly in code
        /// </summary>
        [System.Serializable]
        public class FSpine_FixingSet
        {
            public List<Vector3> Current;
            public List<Vector3> Initial;
            public List<Vector3> Rounded;

            public List<Vector3> Reversed;
            public List<Vector3> RoundedReversed;

            public List<Vector3> Unified;

            public List<Vector3> SetUnified(List<Vector3> toUnify)
            {
                List<Vector3> unified = new List<Vector3>();
                for (int i = 0; i < toUnify.Count; i++) unified.Add(toUnify[toUnify.Count / 2]);
                return unified;
            }

            internal void AddToAllNormal(Vector3 v)
            {
                Current.Add(v);
                Initial.Add(v);
                Rounded.Add(v);
                RoundedReversed.Add(v);
            }

            internal FSpine_FixingSet Init()
            {
                Current = new List<Vector3>();
                Initial = new List<Vector3>();
                Rounded = new List<Vector3>();
                Reversed = new List<Vector3>();
                RoundedReversed = new List<Vector3>();

                return this;
            }
        }

        /// <summary>
        /// Rounding position used in calculating difference for straightening
        /// </summary>
        protected Vector3 RoundPosDiff(Vector3 pos, int digits = 1)
        {
            return new Vector3((float)System.Math.Round(pos.x, digits), (float)System.Math.Round(pos.y, digits), (float)System.Math.Round(pos.z, digits));
        }

        /// <summary>
        /// Rounding fix correction angles to nearest values, we calculate axes directions in precise way, but in most cases rounded are doing job much better
        /// </summary>
        private Vector3 RoundToBiggestValue(Vector3 vec)
        {
            int biggest = 0;
            if (Mathf.Abs(vec.y) > Mathf.Abs(vec.x))
            {
                biggest = 1;
                if (Mathf.Abs(vec.z) > Mathf.Abs(vec.y)) biggest = 2;
            }
            else
                if (Mathf.Abs(vec.z) > Mathf.Abs(vec.x)) biggest = 2;

            if (biggest == 0) vec = new Vector3(Mathf.Round(vec.x), 0f, 0f);
            else
            if (biggest == 1) vec = new Vector3(0f, Mathf.Round(vec.y), 0f);
            else
                vec = new Vector3(0f, 0f, Mathf.Round(vec.z));

            return vec;
        }

        /// <summary>
        /// Class for drawing rays to debug in more visible way
        /// </summary>
        private void DrawFatRay(Vector3 origin, Vector3 dir)
        {
            float off = 0.01f;
            Gizmos.DrawRay(origin + Vector3.forward * off, dir);
            Gizmos.DrawRay(origin - Vector3.forward * off, dir);
            Gizmos.DrawRay(origin - Vector3.right * off, dir);
            Gizmos.DrawRay(origin + Vector3.right * off, dir);
            Gizmos.DrawRay(origin + Vector3.up * off, dir);
            Gizmos.DrawRay(origin - Vector3.up * off, dir);
            Gizmos.DrawRay(origin, dir);
        }

        /// <summary>
        /// Class for drawing lines to debug in more visible way
        /// </summary>
        private void DrawBoneLine(Vector3 origin, Vector3 dir)
        {
            float off = 0.05f;
            Gizmos.DrawLine(origin + Vector3.forward * off, dir);
            Gizmos.DrawLine(origin - Vector3.forward * off, dir);
            Gizmos.DrawLine(origin - Vector3.right * off, dir);
            Gizmos.DrawLine(origin + Vector3.right * off, dir);
            Gizmos.DrawLine(origin + Vector3.up * off, dir);
            Gizmos.DrawLine(origin - Vector3.up * off, dir);
            Gizmos.DrawLine(origin, dir);
        }


        /// <summary>
        /// Refresh selective variables values
        /// </summary>
        public void RefreshSelectivePosNotAnimated()
        {
            if (SelectivePosNotAnimated == null || SelectivePosNotAnimated.Count != SpineTransforms.Count)
            {
                SelectivePosNotAnimated = new List<bool>();
                for (int i = 0; i < SpineTransforms.Count; i++) SelectivePosNotAnimated.Add(true);
            }
        }

        /// <summary>
        /// Refresh selective variables values
        /// </summary>
        public void RefreshSelectiveRotNotAnimated()
        {
            if (SelectiveRotNotAnimated == null || SelectiveRotNotAnimated.Count != SpineTransforms.Count)
            {
                SelectiveRotNotAnimated = new List<bool>();
                for (int i = 0; i < SpineTransforms.Count; i++) SelectiveRotNotAnimated.Add(true);
            }
        }

        /// <summary>
        /// Refresh manual offset variables values
        /// </summary>
        public void RefreshManualPosOffs()
        {
            if (ManualPositionOffsets == null || ManualPositionOffsets.Count != SpineTransforms.Count)
            {
                ManualPositionOffsets = new List<Vector3>();
                for (int i = 0; i < SpineTransforms.Count; i++) ManualPositionOffsets.Add(Vector3.zero);
            }
        }

        /// <summary>
        /// Refresh manual offset variables values
        /// </summary>
        public void RefreshManualRotOffs()
        {
            if (ManualRotationOffsets == null || ManualRotationOffsets.Count != SpineTransforms.Count)
            {
                ManualRotationOffsets = new List<Vector3>();
                for (int i = 0; i < SpineTransforms.Count; i++) ManualRotationOffsets.Add(Vector3.zero);
            }
        }

        /// <summary>
        /// Destroying objects generated by component
        /// </summary>
        public void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (anchorsContainer) Destroy(anchorsContainer.gameObject);
                if (anchorHelpers != null) for (int i = 0; i < anchorHelpers.Length; i++) if (anchorHelpers[i]) Destroy(anchorHelpers[i].gameObject);

                // V1.0.1
                if (mainPivotOffsetTransform)
                {
                    RestoreBasePivotChildren();
                    Destroy(mainPivotOffsetTransform.gameObject);
                }
            }
            else
            {
                if (mainPivotOffsetTransform)
                {
                    RestoreBasePivotChildren();
                    DestroyImmediate(mainPivotOffsetTransform.gameObject);
                }
            }
        }

        // V1.0.1
        /// <summary>
        /// Restoring generated pivot transform children to initial state
        /// </summary>
        private void RestoreBasePivotChildren()
        {
            for (int i = mainPivotOffsetTransform.childCount - 1; i >= 0; i--)
            {
                mainPivotOffsetTransform.GetChild(i).SetParent(mainPivotOffsetTransform.parent, true);
            }
        }

        // V1.0.1
        /// <summary>
        /// Supporting generating additional pivot transform
        /// </summary>
        private void OnValidate()
        {
            if (MainPivotOffset == Vector3.zero)
            {
                if (mainPivotOffsetTransform)
                {
                    if (mainPivotOffsetTransform.childCount > 0)
                    {
                        mainPivotOffsetTransform.localPosition = MainPivotOffset;
                        RestoreBasePivotChildren();
                    }

                    // We can't destroy objects in OnValidate so transform will be left here in case you will use it again
                    //if (Application.isPlaying) Destroy(mainPivotOffsetTransform.gameObject); else DestroyImmediate(mainPivotOffsetTransform.gameObject);
                }
            }
            else
            {
                // Generating pivot offset transform - quickest way without defining other stuff
                if (!mainPivotOffsetTransform)
                {
                    mainPivotOffsetTransform = new GameObject("Main Pivot Offset-Spine Animator-" + name).transform;
                    mainPivotOffsetTransform.SetParent(transform, false);
                    mainPivotOffsetTransform.localPosition = Vector3.zero;
                    mainPivotOffsetTransform.localRotation = Quaternion.identity;
                    mainPivotOffsetTransform.localScale = Vector3.one;
                }

                if (mainPivotOffsetTransform.childCount == 0)
                {
                    for (int i = transform.childCount - 1; i >= 0; i--)
                    {
                        if (transform.GetChild(i) == mainPivotOffsetTransform) continue;
                        transform.GetChild(i).SetParent(mainPivotOffsetTransform, true);
                    }
                }

                mainPivotOffsetTransform.localPosition = MainPivotOffset;
            }
        }

        /// <summary>
        /// Refreshing difference pose for spine animator motion
        /// </summary>
        private void RefreshDifferenceReference()
        {
            ProceduralReferencePose[leadingBoneIndex] = GetLeadingBoneCoordinates();

            if (LastBoneLeading)
            {
                // Setting spine straight forward in hierarchy space
                for (int i = ProceduralReferencePose.Count - 2; i >= 0; i--)
                {
                    ProceduralReferencePose[i].Rotation = ProceduralReferencePose[i - reverser].Rotation;
                    ProceduralReferencePose[i].Position = ProceduralReferencePose[i - reverser].Position - (ProceduralReferencePose[i].TransformDirection(Vector3.forward) * (initialBoneDistances[i] * DistancesMultiplier));
                }
            }
            else
            {
                for (int i = 1; i < ProceduralReferencePose.Count; i++)
                {
                    ProceduralReferencePose[i].Rotation = ProceduralReferencePose[i - reverser].Rotation;
                    ProceduralReferencePose[i].Position = ProceduralReferencePose[i - reverser].Position - (ProceduralReferencePose[i].TransformDirection(Vector3.forward) * (initialBoneDistances[i] * DistancesMultiplier));
                }
            }

        }

        /// <summary>
        /// Trying to predict some correction variables values
        /// </summary>
        public void TryAutoCorrect(Transform head = null)
        {
            if (!head)
            {
                if (SpineTransforms[0].parent == transform) LastBoneLeading = true;
                else
                {
                    Transform p = SpineTransforms[0].parent;
                    for (int i = 0; i < 100; i++)
                    {
                        if (p.parent == null) break;
                        if (p.childCount == 1)
                        {
                            p = p.parent;
                            if (p == transform)
                            {
                                LastBoneLeading = true;
                                break;
                            }
                        }
                        else break;
                    }
                }

                if (SpineTransforms[SpineTransforms.Count - 1].childCount == 0) LastBoneLeading = false;
            }
            else
            {
                Vector2 distances = Vector2.zero;
                distances.x = Vector3.Distance(SpineTransforms[0].position, head.position);
                distances.y = Vector3.Distance(SpineTransforms[1].position, head.position);
                if (distances.x > distances.y) LastBoneLeading = true; else LastBoneLeading = false;
            }


            List<Vector3> dirs = new List<Vector3>();

            for (int i = 0; i < SpineTransforms.Count; i++)
            {
                if (i != SpineTransforms.Count - 1)
                    dirs.Add((SpineTransforms[i].InverseTransformPoint(SpineTransforms[i + 1].position) - SpineTransforms[i].InverseTransformPoint(SpineTransforms[i].position)).normalized);
            }

            if (Mathf.Abs(RoundToBiggestValue(dirs[0]).x) >= 1f)
                if (Mathf.Abs(RoundToBiggestValue(dirs[1]).x) >= 1f)
                {
                    InversedVerticalRotation = true;
                }
        }

        public void DevLog()
        {
            Debug.Log("No need for now");
        }

#if UNITY_EDITOR

        public bool DrawDebug = false;
        [Range(0f, 1f)] public float DebugAlpha = 1f;

        // Set it to false if you don't want any gizmo
        public static bool drawMainGizmo = true;
        public bool drawGizmos = true;

        protected virtual void OnDrawGizmos()
        {
            if (drawMainGizmo)
            {
                Gizmos.DrawIcon(transform.position, "FIMSpace/FSpine/SPR_SpineFollowerGizmo.png", true);

                if (drawGizmos)
                {
                    if (SpineTransforms != null)
                    {
                        if (SpineTransforms.Count != 0)
                        {
                            if (SpineTransforms.Count > 0) if (!LastBoneLeading) Gizmos.DrawIcon(SpineTransforms[0].position, "FIMSpace/FSpine/SPR_SpineFollowerGizmoHead.png"); else Gizmos.DrawIcon(SpineTransforms[0].position, "FIMSpace/FSpine/SPR_SpineFollowerGizmoSegment.png");

                            for (int i = 0; i < SpineTransforms.Count - 1; i++)
                            {
                                if (i == 0) if (SpineTransforms[0] == transform) continue;
                                Gizmos.DrawIcon(SpineTransforms[i].position, "FIMSpace/FSpine/SPR_SpineFollowerGizmoSegment.png");
                            }

                            if (LastBoneLeading) Gizmos.DrawIcon(SpineTransforms[SpineTransforms.Count - 1].position, "FIMSpace/FSpine/SPR_SpineFollowerGizmoHead.png");
                            else
                                Gizmos.DrawIcon(SpineTransforms[SpineTransforms.Count - 1].position, "FIMSpace/FSpine/SPR_SpineFollowerGizmoSegment.png");
                        }
                    }
                }
            }

            if (!DrawDebug || !enabled) return;

            if (!initialized)
            {
                if (LastBoneLeading)
                {
                    for (int i = SpineTransforms.Count - 2; i >= 0; i--)
                    {
                        Gizmos.color = Color.HSVToRGB((float)i / (float)(SpineTransforms.Count), 0.5f, DebugAlpha);
                        DrawBoneLine(SpineTransforms[i].position, SpineTransforms[i + 1].position);
                    }

                }
                else
                    for (int i = 1; i < SpineTransforms.Count; i++)
                    {
                        Gizmos.color = Color.HSVToRGB((float)i / (float)(SpineTransforms.Count), 0.5f, DebugAlpha);
                        DrawBoneLine(SpineTransforms[i].position, SpineTransforms[i - 1].position);
                    }

                return;
            }

            Gizmos.color = new Color(0.5f, 1f, 0.4f, DebugAlpha);
            Gizmos.DrawSphere(anchorHelpers[leadingBoneIndex].position, 0.25f);

            for (int i = 0; i < proceduralPoints.Count; i++)
            {
                Gizmos.color = FColorMethods.ChangeColorAlpha(Color.HSVToRGB((float)i / (float)(proceduralPoints.Count), 0.7f, 0.9f), DebugAlpha);
                DrawFatRay(proceduralPoints[i].Position, proceduralPoints[i].Rotation * Vector3.right);
            }

            if (LastBoneLeading)
            {
                for (int i = proceduralPoints.Count - 2; i >= 0; i--)
                {
                    Gizmos.color = Color.HSVToRGB((float)i / (float)(proceduralPoints.Count), 0.75f, DebugAlpha);
                    DrawBoneLine(proceduralPoints[i].Position, proceduralPoints[i + 1].Position);
                }
            }
            else
            {
                for (int i = 1; i < proceduralPoints.Count; i++)
                {
                    Gizmos.color = Color.HSVToRGB((float)i / (float)(proceduralPoints.Count), 0.75f, DebugAlpha);
                    DrawBoneLine(proceduralPoints[i].Position, proceduralPoints[i - 1].Position);
                }
            }
        }
#endif



        #endregion

    }
}