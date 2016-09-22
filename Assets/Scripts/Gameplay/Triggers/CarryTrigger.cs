using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarryTrigger : MonoBehaviour, ITrigger, ITriggerAll {

    private Plateforme _plateform;

    void Start()
    {
        _plateform = GetComponentInParent<Plateforme>();
    }

    #region ITrigger

    public void OnPlayerEnter(PlayerControllerFus player)
    {
        OnActorEnter(player._physics);
    }
    public void OnPlayerExit(PlayerControllerFus player)
    {
        OnActorExit(player._physics);
    }

    public void OnActorEnter(ActorPhysics actor)
    {
        if (_plateform.carriedObjects.IndexOf(actor.transform) == -1)
        {
            _plateform.carriedObjects.Add(actor.transform);
        }
    }
    public void OnActorExit(ActorPhysics actor)
    {
        if (_plateform.carriedObjects.IndexOf(actor.transform) >= 0)
        {
            _plateform.carriedObjects.Remove(actor.transform);
        }
    }

    #endregion
}
