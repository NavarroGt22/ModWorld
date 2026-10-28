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
        private static bool warnedStats;

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
            // Confirmed in 0.51.2: this is the position used by Actor.updatePos/rendering.
            BaseSimObject sim=actorOrTile as BaseSimObject;
            if(sim!=null)
            {
                position=sim.cur_transform_position;
                position.z=0f;
                return !float.IsNaN(position.x) && !float.IsNaN(position.y);
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

        public static void SetBirthChanceIfAvailable(ActorTrait trait, float percent)
        {
            // 0.51.2: ActorTrait.birth no longer exists as a compile-time member.
            SetOptional(trait, "birth", percent);
        }

        private static void SetOptional(object target, string name, object value)
        {
            if (target == null) return;
            try
            {
                FieldInfo f = target.GetType().GetField(name, AnyInstance);
                if (f != null) { f.SetValue(target, Convert.ChangeType(value, f.FieldType)); return; }
                PropertyInfo p = target.GetType().GetProperty(name, AnyInstance);
                if (p != null && p.CanWrite) p.SetValue(target, Convert.ChangeType(value, p.PropertyType), null);
            }
            catch (Exception) { }
        }

        public static bool AddStat(ActorTrait trait, string statName, float bonus)
        {
            if (trait == null) return false;
            object stats = Read(trait, "base_stats");
            if (stats == null) return false;
            try
            {
                Type t = stats.GetType();
                foreach (PropertyInfo p in t.GetProperties(AnyInstance))
                {
                    ParameterInfo[] keys = p.GetIndexParameters();
                    if (p.Name == "Item" && p.CanWrite && keys.Length == 1 && keys[0].ParameterType == typeof(string))
                    {
                        p.SetValue(stats, Convert.ChangeType(bonus, p.PropertyType), new object[] { statName });
                        return true;
                    }
                }
                // Accommodate different game stat containers (set/add/stat setter).
                foreach (MethodInfo method in t.GetMethods(AnyInstance))
                {
                    if (method.Name != "set" && method.Name != "Set" &&
                        method.Name != "setStat" && method.Name != "add" && method.Name != "Add") continue;
                    ParameterInfo[] p = method.GetParameters();
                    if (p.Length != 2 || p[0].ParameterType != typeof(string)) continue;
                    object number = Convert.ChangeType(bonus, p[1].ParameterType);
                    method.Invoke(stats, new object[] { statName, number });
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AMA 0.3.3] Failed setting stat " + statName + ": " + e.Message);
            }
            if (!warnedStats)
            {
                warnedStats = true;
                Debug.LogWarning("[AMA 0.3.3] Could not write base_stats on WorldBox 0.51.2. Trait still works as a label and combat event hook.");
            }
            return false;
        }
    }
}
