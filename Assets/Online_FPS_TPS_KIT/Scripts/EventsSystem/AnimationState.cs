﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationState : StateMachineBehaviour, IEventCenterComponent, IEventsLists, ICurveEventsList
{
    public EventsCenter EventsCenter { get; set; }
    public List<EventModel> EnterEvents { get; set; } = new List<EventModel>();
    public List<EventModel> UpdateEvents { get; set; } = new List<EventModel>();
    public List<EventModel> ExitEvents { get; set; } = new List<EventModel>();
    public List<CurveEventModel> CurveEvents { get; set; } = new List<CurveEventModel>();
    //public List<CurveEventModel> AnimationCurves { get; set; }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (EventsCenter == null) return;

        foreach (var eventModel in EnterEvents)
        {
            EventsCenter.EventInvoke(eventModel.funcName, EventParameterConverter.ConvertParametersToObjectType(eventModel.parameters));
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (EventsCenter == null) return;

        foreach (var eventModel in UpdateEvents)
        {
            if (eventModel.done) continue;

            if (eventModel.time > stateInfo.normalizedTime && !eventModel.done)
                EventsCenter.EventInvoke(eventModel.funcName,
                        EventParameterConverter.ConvertParametersToObjectType(eventModel.parameters));
        }

        foreach (var animationCurveEvent in CurveEvents)
        {
            object[] floatValueParameter = new object[1] { animationCurveEvent.animationCurve.Evaluate(stateInfo.normalizedTime) };
            EventsCenter.EventInvoke(animationCurveEvent.eventName, floatValueParameter);
        }

        // foreach (var animationCurveModel in AnimationCurves)
        // {
        //     animator.SetFloat(animationCurveModel.eventName,
        //             animationCurveModel.animationCurve.Evaluate(stateInfo.normalizedTime));
        // }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (EventsCenter == null) return;

        foreach (var eventModel in ExitEvents)
        {
            EventsCenter.EventInvoke(eventModel.funcName,
                    EventParameterConverter.ConvertParametersToObjectType(eventModel.parameters));
        }
    }
}
