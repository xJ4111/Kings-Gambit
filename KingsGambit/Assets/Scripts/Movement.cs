using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    CustomPiece Self;
    public Animator Anim;
    public Dictionary<string, float> ClipTimes = new Dictionary<string, float>();

    [Header("Targets")]
    public Tile TargetTile;
    public CustomPiece TargetPiece;

    [Header("State")]
    public bool Attacking;
    public bool Moving;
    public bool Reached;
    private Vector3 TargetPos;

    [Header("Parameters")]
    private float MovementSpeed = 5;
    private float TurnSpeed = 5;
    private float AttackDistance = 4f;

    [Header("Specific Info")]
    public Tile LastPos;
    public float ReviveTime = 0f;
    public GameObject Shield;


    private void Start()
    {
        Self = GetComponent<CustomPiece>();
        Anim = GetComponentInChildren<Animator>();
        TargetPos = transform.position;
        GetClipTimes();

        if(Shield)
        {
            Anim.SetBool("Shielded", Self.Ability == "Shielded");
            Shield.SetActive(Self.Ability == "Shielded");
        }

    }

    // Update is called once per frame
    void Update()
    {
        ReturnRotate();
    }

    void GetClipTimes()
    {
        AnimationClip[] Clips = Anim.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in Clips)
            if (!ClipTimes.ContainsKey(clip.name))
                ClipTimes.Add(clip.name, clip.length);
    }

    #region Movement
    public void Move(bool walk)
    {
        Debug.Log("move");

        if (!Moving && TargetTile)
        {
            Moving = true;

            if (walk)
                StartCoroutine(MoveTick());
            else
                StartCoroutine(WarpTick());
        }
    }

    IEnumerator MoveTick()
    {
        //Determine Target
        if (TargetPiece)
            TargetPos = TargetTile.transform.position - ((TargetTile.transform.position - transform.position).normalized * AttackDistance);
        else
            TargetPos = TargetTile.transform.position;

        //At the target?
        Reached = transform.position == TargetPos;

        //If not, keep moving
        if (!Reached)
        {
            Anim.SetBool("Moving", true);
            transform.position = Vector3.MoveTowards(transform.position, TargetPos, MovementSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPos - transform.position), TurnSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            StartCoroutine(MoveTick());
        }
        //If we are, attack?
        else if (Reached && TargetPiece)
        {
            Anim.SetBool("Moving", false);
            Anim.SetBool("Attacking", true);
            TargetPiece.GetComponent<Movement>().Attacked(ClipTimes["Attack"] * 0.9f);
            yield return new WaitForSeconds(ClipTimes["Attack"]);
            TargetPiece = null;
            Anim.SetBool("Attacking", false);
            StartCoroutine(MoveTick());
        }
        //No more targets? Next turn
        else if (Reached)
        {
            Anim.SetBool("Moving", false);
            TargetTile = null;
            Moving = false;
            Game.M.NextTurn();
        }
    }

    IEnumerator WarpTick()
    {
        //Determine Target
        TargetPos = TargetTile.transform.position;
        Reached = transform.position == TargetPos;

        //Facing Target?
        if (!Reached && !LookingTowards(TargetPos))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPos - transform.position), TurnSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            StartCoroutine(WarpTick());
        }
        else
        {
            //If not, keep moving
            if (!Reached)
            {
                Anim.SetBool("Moving", true);
                //Attack?
                if (TargetPiece)
                {
                    TargetPiece.GetComponent<Movement>().Attacked(ClipTimes["Walk"] * 0.75f);
                    TargetPiece = null;
                }
                yield return new WaitForSeconds(ClipTimes["Walk"]);
                transform.position = TargetPos;
                Anim.SetBool("Moving", false);
                StartCoroutine(WarpTick());
            }
            //No more targets? Next turn
            else if (Reached)
            {
                Anim.SetBool("Moving", false);

                if (Self.Ability == "Combat Medic" && Self.CanRevive())
                {
                    Anim.SetBool("Casting", true);
                    yield return new WaitForSeconds(ClipTimes["Cast"] * 0.5f);
                    Anim.SetBool("Casting", false);
                    CustomPiece revived = Self.Revive(LastPos);
                    revived.MC.Anim.SetBool("Dead", false);
                    revived.MC.Anim.SetBool("Revived", true);
                    yield return new WaitForSeconds(revived.MC.ClipTimes["Revive"]);
                    revived.MC.Anim.SetBool("Revived", false);

                }

                TargetTile = null;
                Moving = false;
                Game.M.NextTurn();
            }
        }
    }

    bool LookingTowards(Vector3 target)
    {
        return Mathf.Abs(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target - transform.position), TurnSpeed * Time.deltaTime).y - Quaternion.LookRotation(TargetPos - transform.position).y) < 0.01f;
    }

    public void Teleport(bool endTurn, Vector3 pos)
    {
        TargetPos = pos;
        StartCoroutine(TeleportTick(endTurn));
    }

    IEnumerator TeleportTick(bool endTurn)
    {
        Anim.SetBool("Teleporting", true);
        yield return new WaitForSeconds(ClipTimes["Crouch"]);
        Anim.SetBool("Teleporting", false);
        transform.position = TargetPos;
        if (endTurn)
            Game.M.NextTurn();
    }

    void ReturnRotate()
    {
        if (!TargetTile && !TargetPiece)
        {
            if (Self.Side == "White")
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, TurnSpeed * Time.deltaTime);
            
            if (Self.Side == "Black")
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 180, 0), TurnSpeed * Time.deltaTime);
        }

        Anim.SetBool("Injured", Self.Injured);
    }

    #endregion

    public void Attacked(float animTime)
    {
        StartCoroutine(Die(animTime));
    }

    private IEnumerator Die(float wait)
    {
        yield return new WaitForSeconds(wait / 2);
        Anim.SetBool("Dead", true);
        yield return new WaitForSeconds(2f);
        Game.M.Kill(Self);
    }

    public void Cast(string effect)
    {
        TargetPiece = Game.M.AbilityTarget;
        TargetTile = Game.M.AbilityPosition;
        TargetPos = TargetTile.transform.position;
        StartCoroutine(CastingTick(effect));
    }

    private IEnumerator CastingTick(string effect)
    {
        //Face Target
        if (Vector3.Distance(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPos - transform.position), TurnSpeed * Time.deltaTime).eulerAngles, Quaternion.LookRotation(TargetPos - transform.position).eulerAngles) > 2)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPos - transform.position), TurnSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            StartCoroutine(CastingTick(effect));
        }
        else
        {
            Anim.SetBool("Moving", false);
            Anim.SetBool("Casting", true);
            yield return new WaitForSeconds(ClipTimes["Cast"] * 0.8f);
            Anim.SetBool("Casting", false);

            switch (effect)
            {
                case "Injure":
                    TargetPiece.Injured = true;
                    break;
                case "Teleport":
                    if (TargetTile.Occupier)
                    {
                        TargetTile.Occupier.MC.Attacked(0);
                        Debug.Log("Dead");
                    }


                    TargetTile.Enter(TargetPiece);
                    TargetPiece.MC.Teleport(false, TargetTile.transform.position);

                    yield return new WaitForSeconds(TargetPiece.MC.ClipTimes["Crouch"] * 2);
                    break;
            }

            TargetPiece = null;

            Game.M.NextTurn();
        }
    }

    public void Promote()
    {
        StartCoroutine(PromoteTick());
    }

    private IEnumerator PromoteTick()
    {
        Anim.SetBool("PoweredUp", true);
        Self.AllyKing.MC.Anim.SetBool("PoweredUp", true);
        yield return new WaitForSeconds(ClipTimes["PoweredUp"]);
        Anim.SetBool("PoweredUp", false);
        Self.AllyKing.MC.Anim.SetBool("PoweredUp", false);
        Game.M.Ready = true;
    }
}
