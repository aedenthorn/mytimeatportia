using Pathea.Spawn;
using System;
using UnityEngine;

namespace Invasion
{
    internal class BossActorAgent : ActorAgent
    {
        public BossActorAgent(Vector3 pos, Vector3 rot, int id, Action<ActorAgent> action) : base(pos,rot,id,action)
        {

        }

        public override bool Update()
        {
            if (!_actor.Alive)
                return false;
            bool result = base.Update();
            _actor.Buff.list.Clear();
            return result;
        }
    }
}