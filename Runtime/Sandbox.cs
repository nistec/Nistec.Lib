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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Security.Policy;

namespace Nistec.Runtime
{

    /// <summary>
    /// This method lets you hoist the compilation process outside of the sandboxed AppDomain, which is a plus
    ///For reference, This class created to facilitate the launching of script assemblies in a clean separate AppDomain that has limited permissions and can be easily unloaded when necessary.
    ///Note: if you use this method to supply security evidence for the new AppDomain, you need to sign your assembly to give it a strong name.
    ///Note:this works fine when run in process, but for a bullet-proof script environment, you need to isolate the script in a separate process to ensure that scripts that do malicious (or just stupid) things like stack overflows, fork bombs, and out of memory situations don't bring down the whole application process.
    /// </summary>
    public class Sandbox : MarshalByRefObject
    {
        const string BaseDirectory = "Untrusted";
        const string DomainName = "Sandbox";

        /// <summary>
        /// ctor
        /// </summary>
        public Sandbox()
        {
        }

        /// <summary>
        /// Create assembly as Sandbox.
        /// </summary>
        /// <returns></returns>
        public static Sandbox Create()
        {
            var setup = new AppDomainSetup()
            {
                ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BaseDirectory),
                ApplicationName = DomainName,
                DisallowBindingRedirects = true,
                DisallowCodeDownload = true,
                DisallowPublisherPolicy = true
            };

            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var domain = AppDomain.CreateDomain(DomainName, null, setup, permissions,
                typeof(Sandbox).Assembly.Evidence.GetHostEvidence<StrongName>());

            return (Sandbox)Activator.CreateInstanceFrom(domain, typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName, typeof(Sandbox).FullName).Unwrap();
        }

        /// <summary>
        /// Execute assembly.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="scriptType"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Execute(string assemblyPath, string scriptType, string method, params object[] parameters)
        {
            new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, assemblyPath).Assert();
            var assembly = Assembly.LoadFile(assemblyPath);
            CodeAccessPermission.RevertAssert();

            Type type = assembly.GetType(scriptType);
            if (type == null)
                return null;

            var instance = ActivatorUtil.CreateInstance(type);

            var methodInfo = type.GetMethod(method);
            if (methodInfo == null)
                return null;
            return methodInfo.Invoke(instance, parameters);
        }
    }
}
