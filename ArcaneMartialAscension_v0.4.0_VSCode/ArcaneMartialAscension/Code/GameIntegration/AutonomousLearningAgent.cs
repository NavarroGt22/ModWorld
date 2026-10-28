using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    public static class AutonomousLearningAgent
    {
        private static MapStats activeMap;
        private static bool wasEnabled=true;
        private static float nextUpdate,nextWarning;
        private static int offset;
        private static double lastWorldTime;

        public static void Update(bool enabled)
        {
            if(World.world==null || World.world.map_stats==null || World.world.units==null)return;
            double now=World.world.getCurWorldTime();
            if(double.IsNaN(now) || double.IsInfinity(now))return;
            if(!ReferenceEquals(activeMap,World.world.map_stats) || now<lastWorldTime)
            {
                if(activeMap!=null)ActorAbilities.ResetRuntimeForWorld();
                activeMap=World.world.map_stats;offset=0;nextUpdate=0;
            }
            lastWorldTime=now;
            if(enabled!=wasEnabled)
            {
                ActorAbilities.ResetLearningObservation();
                wasEnabled=enabled;nextUpdate=0;
            }
            if(!enabled || World.world.isPaused() || Time.unscaledTime<nextUpdate)return;
            nextUpdate=Time.unscaledTime+.5f;
            // Reuse the native live-actor list: no complete world scan per character.
            List<Actor> actors=World.world.units.units_only_alive;
            if(actors==null || actors.Count==0){offset=0;return;}
            int budget=Mathf.Min(24,actors.Count);
            for(int i=0;i<budget;i++)
            {
                if(offset>=actors.Count)offset=0;
                Actor actor=actors[offset++];
                try {ActorAbilities.ObserveLearning(actor,now);}
                catch(Exception error)
                {
                    if(Time.unscaledTime>=nextWarning)
                    {nextWarning=Time.unscaledTime+10f;Debug.LogWarning("[AMA Learning] Falha ao observar habitante: "+error);}
                }
            }
        }
    }
}
