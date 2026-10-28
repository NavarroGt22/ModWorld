using System;
using System.Reflection;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    // WorldBox 0.51.2 replaced a number of public members used by old examples.
    // Keep unstable game-member lookups here, instead of letting C# compilation fail.
    public static class GameCompat
    {
        private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static bool warnedPosition;

        private static object Read(object target, string name)
        {
            if (target == null) return null;
            Type t = target.GetType();
            try
            {
                PropertyInfo p = t.GetProperty(name, AnyInstance);
                if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(target, null);
                FieldInfo f = t.GetField(name, AnyInstance);
                if (f != null) return f.GetValue(target);
            }
            catch (Exception) { }
            return null;
        }

        private static object Call(object target, string name)
        {
            if (target == null) return null;
            try
            {
                foreach (MethodInfo m in target.GetType().GetMethods(AnyInstance))
                    if (m.Name == name && m.GetParameters().Length == 0)
                        return m.Invoke(target, null);
            }
            catch (Exception) { }
            return null;
        }

        public static string DisplayName(Actor actor)
        {
            if (actor == null) return "(nenhum)";
            object data = Read(actor, "data") ?? Read(actor, "base_data");
            object name = Read(data, "name") ?? Read(actor, "name") ?? Call(actor, "getName");
            return name == null ? "Habitante" : name.ToString();
        }

        private static bool ToVector(object value, out Vector3 result)
        {
            result = Vector3.zero;
            if (value == null) return false;
            if (value is Vector3) { result = (Vector3)value; return true; }
            if (value is Vector2)
            {
                Vector2 p = (Vector2)value;
                result = new Vector3(p.x, p.y, 0f);
                return true;
            }
            object x = Read(value, "x");
            object y = Read(value, "y");
            if (x == null || y == null) return false;
            try
            {
                result = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), 0f);
                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool GetPosition(object actorOrTile, out Vector3 position)
        {
            position = Vector3.zero;
            if (actorOrTile == null) return false;
            // Logical map coordinates remain valid at every zoom and for hidden actors.
            // They share the coordinate space used by PlayerControl.getMousePos.
            BaseSimObject sim=actorOrTile as BaseSimObject;
            if(sim!=null)
            {
                position=new Vector3(sim.current_position.x,sim.current_position.y,0f);
                return !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsInfinity(position.x) && !float.IsInfinity(position.y);
            }
            // Try direct coordinates / Unity-type Vector3 first.
            if (ToVector(actorOrTile, out position)) return true;
            string[] direct = { "currentPosition", "current_position", "position", "pos", "position_vec3", "position_real" };
            foreach (string candidate in direct)
                if (ToVector(Read(actorOrTile, candidate), out position)) return true;
            string[] methods = { "getPosition", "getPos", "getCurrentPosition" };
            foreach (string name in methods)
                if (ToVector(Call(actorOrTile, name), out position)) return true;
            object transform = Read(actorOrTile, "transform") ?? Read(actorOrTile, "_transform");
            if (ToVector(Read(transform, "position"), out position)) return true;
            // Actor positions in game builds commonly come via their occupied tile.
            string[] tiles = { "currentTile", "current_tile", "tile", "main_tile", "currentTileMain" };
            foreach (string candidate in tiles)
                if (ToVector(Read(actorOrTile, candidate), out position)) return true;
            string[] tileMethods = { "getTile", "getCurrentTile", "getTileMain", "getCurrentTileMain" };
            foreach (string name in tileMethods)
                if (ToVector(Call(actorOrTile, name), out position)) return true;
            // May be a BaseSimObject that has a .a Actor reference.
            object nested = Read(actorOrTile, "a");
            if (nested != null && !ReferenceEquals(nested, actorOrTile))
                return GetPosition(nested, out position);
            if (!warnedPosition)
            {
                warnedPosition = true;
                Debug.LogWarning("[AMA 0.3.3] Actor position not found. Please send Player.log. Type=" + actorOrTile.GetType().FullName);
            }
            return false;
        }

        public static Camera MapCamera
        {
            get { return World.world!=null && World.world.camera!=null?World.world.camera:Camera.main; }
        }

        public static Vector3 PositionOrCursor(object target)
        {
            Vector3 pos;
            if (GetPosition(target, out pos)) return pos;
            Camera cam = Camera.main;
            if (cam == null && Camera.allCamerasCount > 0) cam = Camera.allCameras[0];
            if (cam == null) return Vector3.zero;
            Vector3 cursor = Input.mousePosition;
            cursor.z = Mathf.Abs(cam.transform.position.z);
            Vector3 world = cam.ScreenToWorldPoint(cursor);
            world.z = 0f;
            return world;
        }

        public static void ConfigureProgressionTrait(ActorTrait trait)
        {
            // Native fields verified in 0.51.2. Formation/equipment owns these traits;
            // native random births cannot create a class without its actual progression.
            trait.rate_birth=0;
            trait.rate_acquire_grow_up=0;
            trait.rate_inherit=0;
            trait.spawn_random_trait_allowed=false;
        }

        public static bool AddStat(ActorTrait trait, string statName, float bonus)
        {
            if(trait==null || string.IsNullOrEmpty(statName) || float.IsNaN(bonus) || float.IsInfinity(bonus))return false;
            try
            {
                // 0.51.2: inherited BaseUnlockableAsset.base_stats starts null.
                // Verified in the local assembly, including BaseStats' string indexer.
                if(AssetManager.base_stats_library.get(statName)==null)
                    throw new InvalidOperationException("Atributo nativo desconhecido: "+statName);
                if(trait.base_stats==null)trait.base_stats=new BaseStats();
                trait.base_stats[statName]=bonus;
                return Mathf.Approximately(trait.base_stats[statName],bonus);
            }
            catch (Exception e)
            {
                Debug.LogError("[AMA Stats] Falha em "+trait.id+" / "+statName+": "+e);
            }
            return false;
        }
    }
}
