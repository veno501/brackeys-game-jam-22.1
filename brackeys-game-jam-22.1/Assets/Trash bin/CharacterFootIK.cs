using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFootIK : MonoBehaviour
{
    Animator anim;
    CapsuleCollider col;
    private Vector3 leftFootIkPosition, rightFootIkPosition;
    private Quaternion leftFootIkRotation, rightFootIkRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;
    [SerializeField] float defaultPelvisOffset = 0.9f;

    [Header("Foot Grounding IK")]
    [HideInInspector] public bool enableFeetIk = true;
    [HideInInspector] public float slopeAngle = 1.0f;
    [SerializeField] LayerMask m_RaycastLayers = Physics.AllLayers;
    [SerializeField] private float pelvisOffset = 0.0f;
    [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [SerializeField] private float feetToIkPositionSpeed = 1.0f;

    // public string leftFootAnimVariableName = "LeftFootIkRotation";
    // public string rightFootAnimVariableName = "RightFootIkRotation";

    // public bool useRotationIkCurves = false;
    public bool useFootRotation = false;


    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        // lastPelvisPositionY = anim.bodyPosition.y;
    }

    private void FixedUpdate()
    {
        if(!enableFeetIk) return;
        
        // find and raycast to the ground to find positions
        FeetPositionSolver(HumanBodyBones.RightFoot, ref rightFootIkPosition, ref rightFootIkRotation);
        FeetPositionSolver(HumanBodyBones.LeftFoot, ref leftFootIkPosition, ref leftFootIkRotation);

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(!enableFeetIk)
        {
            ResetPelvisHeight();
            return;
        }
        
        MovePelvisHeight();

        if(useFootRotation)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
        }

        //right foot ik position and rotation
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        // if(useRotationIkCurves)
        //     anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);

        //left foot ik position and rotation
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        // if (useRotationIkCurves)
        //     anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
    }


#region FeetGroundingMethods

    // Moves the feet to ik point
    void MoveFeetToIkPoint (AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 newIkPosition = anim.GetIKPosition(foot);

        if(positionIkHolder != Vector3.zero)
        {
            newIkPosition = transform.InverseTransformPoint(newIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float newIkPositionY = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, Time.deltaTime * 60f * feetToIkPositionSpeed * slopeAngle);
            newIkPosition.y += newIkPositionY;

            lastFootPositionY = newIkPositionY;

            newIkPosition = transform.TransformPoint(newIkPosition);

            anim.SetIKRotation(foot, rotationIkHolder);
        }
        // doesn't really help, sets feet to default if no ground instead of setting to same as last frame
        // else
        // {
        //     newIkPosition.y = transform.position.y;
        // }

        anim.SetIKPosition(foot, newIkPosition);
    }

    // Moves the pelvis down or up to match the feet offset
    private void MovePelvisHeight()
    {

        if(rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero)
        {
            lastPelvisPositionY = anim.bodyPosition.y;
            return;
        }

        float lOffset = leftFootIkPosition.y - transform.position.y;
        float rOffset = rightFootIkPosition.y - transform.position.y;

#if UNITY_EDITOR
        Debug.DrawRay(anim.bodyPosition - transform.forward, Vector3.up * Mathf.Min(lOffset, rOffset), Color.red);
#endif

        float newPelvisOffset = Mathf.Min(lOffset, rOffset);
        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * newPelvisOffset;

        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, Time.deltaTime * 60f * pelvisUpAndDownSpeed);

        anim.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = anim.bodyPosition.y;
    }

    // Moves hips to default height when not using IK
    private void ResetPelvisHeight()
    {
        Vector3 newPelvisPosition = anim.bodyPosition;
        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, transform.position.y + defaultPelvisOffset, Time.deltaTime * 60f * pelvisUpAndDownSpeed * 2f);

        anim.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = anim.bodyPosition.y;
    }

    // Locates the feet position via a raycast and then solving
    private void FeetPositionSolver(HumanBodyBones foot, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        float raycaseHighPoint = col.height - 0.5f;
        float raycastLowPoint = col.height * 0.9f;

        Vector3 raycastStartPosition = anim.GetBoneTransform(foot).position;
        raycastStartPosition.y = transform.position.y + raycaseHighPoint;

        RaycastHit feetOutHit;

#if UNITY_EDITOR
            // Debug.DrawRay(raycastStartPosition, Vector3.down * (raycastLowPoint + raycaseHighPoint), Color.yellow);
#endif

        if (Physics.Raycast(raycastStartPosition, Vector3.down, out feetOutHit, raycastLowPoint + raycaseHighPoint, 
            m_RaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // gets foot IK position from the in-air position
            feetIkPositions = raycastStartPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            //feetIkPositions.y = Mathf.Clamp(feetIkPositions.y - transform.position.y, -0.2f, 0.4f) + transform.position.y;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
        }
        else
            feetIkPositions = Vector3.zero; // raycast didn't hit

    }

#endregion
}
