using UnityEngine;

[DefaultExecutionOrder(1000)] // esegue dopo l'animazione del leader
[ExecuteAlways]
public class FollowWithOffset : MonoBehaviour
{
    public Transform leader;

    public enum OffsetSpace { LeaderLocal, World, ParentLocal }
    [Header("Offset space (consigliato: LeaderLocal)")]
    public OffsetSpace offsetSpace = OffsetSpace.LeaderLocal;

    public enum PosMode { Disabled, AbsoluteToLeader, OffsetFromLeader }
    public enum RotMode { Disabled, AbsoluteToLeader, OffsetFromLeader }
    public enum SclMode { Disabled, MatchLeader, PreserveInitialWorld }

    [Header("What to follow")]
    public PosMode followPosition = PosMode.OffsetFromLeader;
    public RotMode followRotation = RotMode.OffsetFromLeader;
    public SclMode followScale = SclMode.Disabled; // << default: NO scala

    [Header("Axis locks (in Leader space)")]
    public bool lockX = false, lockY = false, lockZ = false; // assi del leader, non world

    [Header("Smoothing (0 = off)")]
    [Range(0f, 0.5f)] public float posDamp = 0.0f;
    [Range(0f, 0.5f)] public float rotDamp = 0.0f;
    [Range(0f, 0.5f)] public float sclDamp = 0.0f;

    [Tooltip("Forza un rebind dell'offset ogni frame (solo debug)")]
    public bool forceRecalcEachFrame = false;

    // cached offsets
    Vector3 _offsetPos_LeaderLocal;
    Quaternion _offsetRot_LeaderLocal = Quaternion.identity;
    Vector3 _initialWorldScale;      // world scale iniziale del follower
    Transform _cachedParent;

    // smooth state
    Vector3 _posVel; // per SmoothDamp
    Vector3 _sclVel;

    void OnEnable() => CacheOffset();
    void Start() => CacheOffset();

    void OnValidate()
    {
        if (!Application.isPlaying) CacheOffset();
    }

    [ContextMenu("Recalculate Offset Now")]
    public void RecalculateOffsetNow() => CacheOffset();

    void CacheOffset()
    {
        if (!leader) return;
        _cachedParent = transform.parent;

        // Calcola offset posizione/rotazione nello SPAZIO DEL LEADER
        _offsetPos_LeaderLocal = leader.InverseTransformPoint(transform.position);
        _offsetRot_LeaderLocal = Quaternion.Inverse(leader.rotation) * transform.rotation;

        // Salva la scala world iniziale per modalità PreserveInitialWorld
        _initialWorldScale = GetWorldScale(transform);
    }

    void Update()
    {
        if (forceRecalcEachFrame) CacheOffset();
        if (_cachedParent != transform.parent)
        {
            _cachedParent = transform.parent;
            CacheOffset();
        }
    }

    void LateUpdate()
    {
        if (!leader) return;

        // --- TARGETS ---
        // Posizione
        Vector3 targetWorldPos = transform.position;
        if (followPosition != PosMode.Disabled)
        {
            if (followPosition == PosMode.AbsoluteToLeader)
                targetWorldPos = leader.position;
            else // OffsetFromLeader
                targetWorldPos = leader.TransformPoint(SpaceOffset(offsetSpace));
        }

        // Axis locks nello SPAZIO DEL LEADER
        if (lockX || lockY || lockZ)
        {
            // porta pos attuale e target nello spazio leader, applica lock, poi torna in world
            Vector3 curL = leader.InverseTransformPoint(transform.position);
            Vector3 tgtL = leader.InverseTransformPoint(targetWorldPos);
            if (lockX) tgtL.x = curL.x;
            if (lockY) tgtL.y = curL.y;
            if (lockZ) tgtL.z = curL.z;
            targetWorldPos = leader.TransformPoint(tgtL);
        }

        // Rotazione
        Quaternion targetWorldRot = transform.rotation;
        if (followRotation != RotMode.Disabled)
        {
            if (followRotation == RotMode.AbsoluteToLeader)
                targetWorldRot = leader.rotation;
            else // OffsetFromLeader
                targetWorldRot = leader.rotation * _offsetRot_LeaderLocal;
        }

        // Scala
        Vector3 targetLocalScale = transform.localScale;
        if (followScale != SclMode.Disabled)
        {
            if (followScale == SclMode.MatchLeader)
            {
                // copia la world scale del leader (attenzione: non è quasi mai desiderabile per vasi!)
                Vector3 worldS = GetWorldScale(leader);
                targetLocalScale = ToLocalScale(transform, worldS);
            }
            else if (followScale == SclMode.PreserveInitialWorld)
            {
                // mantieni la world scale iniziale del follower
                targetLocalScale = ToLocalScale(transform, _initialWorldScale);
            }
        }

        // --- APPLY with damping ---
        if (followPosition != PosMode.Disabled)
        {
            if (posDamp > 0f && Application.isPlaying)
                transform.position = Vector3.SmoothDamp(transform.position, targetWorldPos, ref _posVel, posDamp);
            else
                transform.position = targetWorldPos;
        }

        if (followRotation != RotMode.Disabled)
        {
            if (rotDamp > 0f && Application.isPlaying)
            {
                float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(1e-4f, rotDamp));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRot, t);
            }
            else
                transform.rotation = targetWorldRot;
        }

        if (followScale != SclMode.Disabled)
        {
            if (sclDamp > 0f && Application.isPlaying)
            {
                Vector3 curWS = GetWorldScale(transform);
                Vector3 tgtWS = GetWorldScaleForLocal(transform, targetLocalScale);
                Vector3 smWS = new Vector3(
                    Mathf.SmoothDamp(curWS.x, tgtWS.x, ref _sclVel.x, sclDamp),
                    Mathf.SmoothDamp(curWS.y, tgtWS.y, ref _sclVel.y, sclDamp),
                    Mathf.SmoothDamp(curWS.z, tgtWS.z, ref _sclVel.z, sclDamp)
                );
                transform.localScale = ToLocalScale(transform, smWS);
            }
            else
            {
                transform.localScale = targetLocalScale;
            }
        }
    }

    // ------ helpers ------
    Vector3 SpaceOffset(OffsetSpace space)
    {
        switch (space)
        {
            case OffsetSpace.World:
                return transform.position - leader.position; // world delta (sconsigliato per rotazioni)
            case OffsetSpace.ParentLocal:
                if (transform.parent)
                {
                    Vector3 world = transform.position;
                    Vector3 parentLocal = transform.parent.InverseTransformPoint(world);
                    // portiamo questo parentLocal nello spazio leader
                    Vector3 leaderLocalFromParentLocal = leader.InverseTransformPoint(transform.parent.TransformPoint(parentLocal));
                    return leaderLocalFromParentLocal;
                }
                return leader.InverseTransformPoint(transform.position);
            case OffsetSpace.LeaderLocal:
            default:
                return _offsetPos_LeaderLocal;
        }
    }

    static Vector3 GetWorldScale(Transform t)
    {
        var lossy = t.lossyScale; // Unity la calcola già correttamente
        return new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));
    }

    static Vector3 GetWorldScaleForLocal(Transform t, Vector3 localScale)
    {
        if (t.parent == null) return localScale;
        Vector3 ps = t.parent.lossyScale;
        return new Vector3(localScale.x * ps.x, localScale.y * ps.y, localScale.z * ps.z);
    }

    static Vector3 ToLocalScale(Transform t, Vector3 desiredWorldScale)
    {
        if (t.parent == null) return desiredWorldScale;
        Vector3 ps = t.parent.lossyScale;
        return new Vector3(
            ps.x == 0 ? 1 : desiredWorldScale.x / ps.x,
            ps.y == 0 ? 1 : desiredWorldScale.y / ps.y,
            ps.z == 0 ? 1 : desiredWorldScale.z / ps.z
        );
    }
}