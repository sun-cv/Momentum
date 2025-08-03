

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Momentum
{


    public interface IStrategy
    {
        Node.Status Process();
        void Reset();
    }



    public class PatrolStrategy : IStrategy
    {
        readonly Transform          entity;
        readonly NavMeshAgent       agent;
        readonly List<Transform>    patrolPoints;
        readonly float              patrolSpeed;

        int currentIndex;
        bool isPathCalulcated;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f)
        {
            this.entity         = entity;
            this.agent          = agent;
            this.patrolPoints   = patrolPoints;
            this.patrolSpeed    = patrolSpeed;
        }

        public Node.Status Process() 
        {
            if (currentIndex == patrolPoints.Count)
            {
                return Node.Status.Success;
            }

            var target = patrolPoints[currentIndex];
            
            agent.SetDestination(target.position);
            entity.LookAt(target);

            if (isPathCalulcated && agent.remainingDistance < 0.1f)
            {
                currentIndex++;
                isPathCalulcated = false;
            }

            if (agent.pathPending)
            {
                isPathCalulcated = true;
            }

            return Node.Status.Running;
        }

        public void Reset()
        {
            currentIndex = 0;
        }

    }


}