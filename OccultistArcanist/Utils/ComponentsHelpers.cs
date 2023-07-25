using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using TabletopTweaks.Core.ModLogic;
using TabletopTweaks.Core.Utilities;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace OccultistArcanist {
    public static class ComponentsHelpers
    {
        internal class ReferenceEqualityComparer : EqualityComparer<object>
        {
            public override bool Equals(object x, object y)
            {
                return x == y;
            }

            public override int GetHashCode(object obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                WeakResourceLink weakResourceLink = obj as WeakResourceLink;
                if ((object)weakResourceLink != null)
                {
                    if (weakResourceLink.AssetId == null)
                    {
                        return "WeakResourceLink".GetHashCode();
                    }

                    return weakResourceLink.GetHashCode();
                }

                return obj.GetHashCode();
            }
        }
        internal class ArrayTraverse
        {
            public int[] Position;

            private int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; i++)
                {
                    maxLengths[i] = array.GetLength(i) - 1;
                }

                Position = new int[array.Rank];
            }

            internal bool Step()
            {
                for (int i = 0; i < Position.Length; i++)
                {
                    if (Position[i] < maxLengths[i])
                    {
                        Position[i]++;
                        for (int j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null)
            {
                return null;
            }

            Type type = originalObject.GetType();
            if (IsPrimitive(type))
            {
                return originalObject;
            }

            if (originalObject is BlueprintReferenceBase)
            {
                return originalObject;
            }

            if (visited.ContainsKey(originalObject))
            {
                return visited[originalObject];
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return null;
            }

            object obj = CloneMethod.Invoke(originalObject, null);
            if (type.IsArray && !IsPrimitive(type.GetElementType()))
            {
                Array clonedArray = (Array)obj;
                ForEach(clonedArray, delegate (Array array, int[] indices)
                {
                    array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices);
                });
            }

            visited.Add(originalObject, obj);
            CopyFields(originalObject, visited, obj, type);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, obj, type);
            return obj;
            static void ForEach(Array array, Action<Array, int[]> action)
            {
                if (array.LongLength != 0L)
                {
                    ArrayTraverse arrayTraverse = new ArrayTraverse(array);
                    do
                    {
                        action(array, arrayTraverse.Position);
                    }
                    while (arrayTraverse.Step());
                }
            }
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, (FieldInfo info) => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            FieldInfo[] fields = typeToReflect.GetFields(bindingFlags);
            foreach (FieldInfo fieldInfo in fields)
            {
                if ((filter == null || filter(fieldInfo)) && !IsPrimitive(fieldInfo.FieldType))
                {
                    object value = InternalCopy(fieldInfo.GetValue(originalObject), visited);
                    fieldInfo.SetValue(cloneObject, value);
                }
            }
        }

        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static bool IsPrimitive(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return type.IsValueType & type.IsPrimitive;
        }

        public static object Copy(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        public static T CopyA<T>(this T original)
        {
            return (T)((object)original.Copy());
        }

        public static T CreateCopy<T>(this T original, Action<T> action = null)
        {
            T clone = original.CopyA<T>();
            bool flag = action != null;
            if (flag)
            {
                action(clone);
            }
            return clone;
        }

        public static void ReplaceComponent<T>(this BlueprintScriptableObject obj, BlueprintComponent replacement) where T : BlueprintComponent
        {
            ReplaceComponent(obj, obj.GetComponent<T>(), replacement);
        }


        public static void ReplaceComponent<T>(this BlueprintScriptableObject obj, Action<T> action) where T : BlueprintComponent
        {
            var replacement = obj.GetComponent<T>().CreateCopy();
            action(replacement);
            ReplaceComponent(obj, obj.GetComponent<T>(), replacement);
        }


        public static void MaybeReplaceComponent<T>(this BlueprintScriptableObject obj, Action<T> action) where T : BlueprintComponent
        {
            var replacement = obj.GetComponent<T>()?.CreateCopy();
            if (replacement == null)
            {
                return;
            }
            action(replacement);
            ReplaceComponent(obj, obj.GetComponent<T>(), replacement);
        }


        public static void ReplaceComponent(this BlueprintScriptableObject obj, BlueprintComponent original, BlueprintComponent replacement)
        {
            var components = obj.ComponentsArray;
            var newComponents = new BlueprintComponent[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                var c = components[i];
                newComponents[i] = c == original ? replacement : c;
            }
            obj.SetComponents(newComponents);
        }

    }
}