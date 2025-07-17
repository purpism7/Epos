using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity;

using Creature.Action;
using UnityEngine.AI;

namespace Creature
{
    public interface ISubject
    {
        
        int Id { get; }
        Vector3 HeadPos { get; }

        bool IsActivate { get; }
        void Activate();
        void Deactivate();
        
        Transform Transform { get; }
        // Rigidbody2D Rigidbody2D { get; }
        NavMeshAgent NavMeshAgent { get; }
    }
}

