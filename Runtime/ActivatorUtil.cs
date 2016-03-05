//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Nistec.Runtime
{

    public class ActivatorUtil
    {
        private static readonly ActivatorUtil instance = new ActivatorUtil();

        // Explicit static constructor to tell C# compiler
        // not to mark type as before field init
        static ActivatorUtil()
        {
        }
        private ActivatorUtil()
        {
        }
        public static ActivatorUtil Get { get { return instance; } }

        internal delegate object GenericSetterDeligate(object target, object value);
        internal delegate object GenericGetterDeligate(object obj);
        private delegate object CreateObjectDeligate();

        private ConcurrentDictionary<Type, CreateObjectDeligate> _ctorCache = new ConcurrentDictionary<Type, CreateObjectDeligate>();
        private ConcurrentDictionary<string, Dictionary<string, FieldInfo>> _fieldCache = new ConcurrentDictionary<string, Dictionary<string, FieldInfo>>();
        private ConcurrentDictionary<string, Dictionary<string, PropertyInfo>> _propertyCache = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>();

        public static object CreateInstance(Type objtype)
        {
            return ActivatorUtil.Get.CreateInstanceInternal(objtype);
        }
        public static T CreateInstance<T>()
        {
            return (T)ActivatorUtil.Get.CreateInstanceInternal(typeof(T));
        }
        public object CreateInstanceInternal(Type objtype)
        {
            try
            {
                CreateObjectDeligate oDeligate = null;
                if (_ctorCache.TryGetValue(objtype, out oDeligate))
                {
                    return oDeligate();
                }
                else
                {
                    if (objtype.IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", objtype, null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        oDeligate = (CreateObjectDeligate)dynMethod.CreateDelegate(typeof(CreateObjectDeligate));
                        _ctorCache.TryAdd(objtype, oDeligate);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", typeof(object), null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        var lv = ilGen.DeclareLocal(objtype);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, objtype);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, objtype);
                        ilGen.Emit(OpCodes.Ret);
                        oDeligate = (CreateObjectDeligate)dynMethod.CreateDelegate(typeof(CreateObjectDeligate));
                        _ctorCache.TryAdd(objtype, oDeligate);
                    }
                    return oDeligate();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        public PropertyInfo[] GetProperties(Type type, string typename, bool ignoreCase, bool customType)
        {
            Dictionary<string, PropertyInfo> td = null;
            if (_propertyCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] pr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo p in pr)
                {
                    if (ignoreCase)
                        td.Add(p.Name.ToLower(), p);
                    else
                        td.Add(p.Name, p);
                }
                _propertyCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        public FieldInfo[] GetFields(Type type, string typename, bool ignoreCase, bool customType)
        {
            Dictionary<string, FieldInfo> td = null;
            if (_fieldCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, FieldInfo>();
                FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo f in fi)
                {
                    if (ignoreCase)
                        td.Add(f.Name.ToLower(), f);
                    else
                        td.Add(f.Name, f);
                }

                _fieldCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        internal static GenericSetterDeligate CreateSetField(Type type, FieldInfo fieldInfo)
        {
            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod dynamicSet = new DynamicMethod("_", typeof(object), arguments, type);

            ILGenerator il = dynamicSet.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsClass)
                    il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
                else
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                il.Emit(OpCodes.Stfld, fieldInfo);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                il.Emit(OpCodes.Stfld, fieldInfo);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);
            }
            return (GenericSetterDeligate)dynamicSet.CreateDelegate(typeof(GenericSetterDeligate));
        }

        internal static GenericSetterDeligate CreateSetMethod(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;

            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod fieldsSetter = new DynamicMethod("_", typeof(object), arguments);
            ILGenerator il = fieldsSetter.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsClass)
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                else
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                il.EmitCall(OpCodes.Call, setMethod, null);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsClass)
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                else
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                il.EmitCall(OpCodes.Callvirt, setMethod, null);
                il.Emit(OpCodes.Ldarg_0);
            }

            il.Emit(OpCodes.Ret);

            return (GenericSetterDeligate)fieldsSetter.CreateDelegate(typeof(GenericSetterDeligate));
        }

        internal static GenericGetterDeligate CreateGetField(Type type, FieldInfo fieldInfo)
        {
            DynamicMethod dynamicGet = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type);

            ILGenerator il = dynamicGet.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Ret);

            return (GenericGetterDeligate)dynamicGet.CreateDelegate(typeof(GenericGetterDeligate));
        }

        internal static GenericGetterDeligate CreateGetMethod(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;

            DynamicMethod fieldsGetter = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type);

            ILGenerator il = fieldsGetter.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.EmitCall(OpCodes.Call, getMethod, null);
                if (propertyInfo.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                il.EmitCall(OpCodes.Callvirt, getMethod, null);
                if (propertyInfo.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            il.Emit(OpCodes.Ret);

            return (GenericGetterDeligate)fieldsGetter.CreateDelegate(typeof(GenericGetterDeligate));
        }

        public void ClearCache()
        {
            _ctorCache = new ConcurrentDictionary<Type, CreateObjectDeligate>();
            _fieldCache = new ConcurrentDictionary<string, Dictionary<string, FieldInfo>>();
            _propertyCache = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>();

        }
    }
}