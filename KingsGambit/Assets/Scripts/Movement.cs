using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    CustomPiece Self;

    public Tile Target;
    public CustomPiece Attack;
    public bool Attacking;

    private float MovementSpeed = 5;
    private float TurnSpeed = 5;
    private float AttackDistance = 2.5f;

    public bool Moving;
    public bool Reached;

    public Vector3 TargetPos;

    private void Start()
    {
        Self = GetComponent<CustomPiece>(); 
        TargetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ReturnRotate();
    }

    public void Move()
    {
        if(!Moving && Target)
        {
            Moving = true;
            StartCoroutine(MoveTick());
        }
    }

    IEnumerator MoveTick()
    {
        //Determine Target
        if (Attack)
            TargetPos = Target.transform.position - ((Target.transform.position - transform.position).normalized * AttackDistance);
        else
            TargetPos = Target.transform.position;

        //At the target?
        Reached = transform.position == TargetPos;

        //If not, keep moving
        if(!Reached)
        {
            transform.position = Vector3.MoveTowards(transform.position, TargetPos, MovementSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPos - transform.position), TurnSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            StartCoroutine(MoveTick());
        }
        //If we are, attack?
        else if(Reached && Attack)
        {
            Attacking = true;
            yield return new WaitForSeconds(1f);
            Game.M.Kill(Attack);
            Attack = null;
            Attacking = false;
            StartCoroutine(MoveTick());
        }
        //No more targets? Next turn
        else if (Reached)
        {
            Target = null;
            Moving = false;
            Game.M.NextTurn();
        }
    }

    void ReturnRotate()
    {
        if (!Target && !Attack)
        {
            if (Self.Side == "White")
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, TurnSpeed * Time.deltaTime);
            else if (Self.Side == "Black")
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 180, 0), TurnSpeed * Time.deltaTime);
            }
        }
    }
}
