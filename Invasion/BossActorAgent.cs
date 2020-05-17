using Pathea.ActorNs;
using Pathea.AttrNs;
using Pathea.BuffNs;
using Pathea.Spawn;
using System;
using System.Reflection;
using UnityEngine;

namespace Invasion
{
    internal class BossActorAgent : ActorAgent
    {
        public BossActorAgent(Vector3 pos, Vector3 rot, int id, Action<ActorAgent> action) : base(pos,rot,id,action)
        {

        }
    }
}