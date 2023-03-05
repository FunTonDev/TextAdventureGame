using System;
using UnityEngine;
using UnityEngine.Events;

public class Waypoint
{
    private readonly string id;
    private readonly float topRightBound;
    private UnityEvent reachedEvent;
    private bool reached;
    private readonly float detectRange;
    private readonly Vector2 pos;

    public Waypoint(string par_id, float par_topRightBound, UnityAction par_event, float par_detectRange = 5.0f) {
        id = par_id;
        topRightBound = par_topRightBound;
        reachedEvent = new UnityEvent();
        reachedEvent.AddListener(par_event);
        reached = false;
        detectRange = par_detectRange;
        pos = new Vector2(getPosWithinBounds(), getPosWithinBounds());       
        Debug.Log(string.Format("Waypoint({0}): Constructed @({1},{2}).", id, pos.x, pos.y));
    }

    public string getId() { return id; }

    public Vector2 getPos() { return pos; }

    public bool wasReached() { return reached; }

    public void attemptReachedEvent(Vector2 targetPosition) {
        if (!reached) {
            reached = Vector2.Distance(targetPosition, pos) <= detectRange;
            if (reached) {
                reachedEvent.Invoke();
                Debug.Log(string.Format("Waypoint({0}): Invoked reached event.", id));
            }
        }
    }

    private float getPosWithinBounds() {
        return (float)Math.Round(UnityEngine.Random.Range(0.0f, topRightBound), 1);
    }
}
