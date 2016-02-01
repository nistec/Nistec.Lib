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
using System.Runtime.InteropServices;
using System.Text;


namespace Nistec.Runtime
{
	#region COM
	/// <summary>
	/// COM HR checker: just to make code more compact;
	/// </summary>
	class COM
	{
		static public void CheckHR(int hr)
		{
			if (hr < 0)
				Marshal.ThrowExceptionForHR(hr);
		}
	}

	#endregion

	#region interfaces

	/// <summary>
	/// IAssemblyCache; COM import
	/// </summary>
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
	internal interface IAssemblyCache
	{
		//PreserveSig() Indicates that the HRESULT or retval signature transformation that takes place during COM interop calls should be suppressed
		[PreserveSig()]
		int UninstallAssembly(	int flags, 
			[MarshalAs(UnmanagedType.LPWStr)] 
			string assemblyName, 
			InstallReference refData, 
			out AssemblyCacheUninstallDisposition disposition);
        
		[PreserveSig()]
		int QueryAssemblyInfo(	int flags, 
			[MarshalAs(UnmanagedType.LPWStr)] 
			string assemblyName, 
			ref AssemblyInfo assemblyInfo);
		[PreserveSig()]
		int Reserved (			int flags, 
			IntPtr pvReserved, 
			out Object ppAsmItem, 
			[MarshalAs(UnmanagedType.LPWStr)] 
			string assemblyName);
		[PreserveSig()]
		int Reserved(			out Object ppAsmScavenger);
        
		[PreserveSig()]
		int InstallAssembly(	int flags, 
			[MarshalAs(UnmanagedType.LPWStr)] 
			string assemblyFilePath, 
			InstallReference refData);
	}
	/// <summary>
	/// IAssemblyName; COM import
	/// </summary>
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
	internal interface IAssemblyName
	{
		[PreserveSig()]
		int SetProperty(	int PropertyId, 
			IntPtr pvProperty, 
			int cbProperty);     
		[PreserveSig()]
		int GetProperty(	int PropertyId, 
			IntPtr pvProperty, 
			ref int pcbProperty);
		[PreserveSig()]
		int Finalize(); 
		[PreserveSig()]
		int GetDisplayName(	StringBuilder pDisplayName, 
			ref int pccDisplayName, 
			int displayFlags);
        
		[PreserveSig()]
		int Reserved(	ref Guid guid, 
			Object o1, 
			Object o2,
			string string1,
			Int64 llFlags,
			IntPtr pvReserved,
			int cbReserved,
			out IntPtr ppv);
		[PreserveSig()]
		int GetName(		ref int pccBuffer, 
			StringBuilder pwzName);
		[PreserveSig()]
		int GetVersion(		out int versionHi, 
			out int versionLow);
		[PreserveSig()]
		int IsEqual(		IAssemblyName pAsmName, 
			int cmpFlags);
		[PreserveSig()]
		int Clone(			out IAssemblyName pAsmName);
	}
	/// <summary>
	/// IAssemblyEnum; COM import
	/// </summary>
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
	internal interface IAssemblyEnum
	{
		[PreserveSig()]
		int GetNextAssembly(	IntPtr pvReserved, 
			out IAssemblyName ppName, 
			int flags);
		[PreserveSig()]
		int Reset();
		[PreserveSig()]
		int Clone(				out IAssemblyEnum ppEnum);
	}

	#endregion

	#region Enums

	/// <summary>
	/// AssemblyCommitFlags; Used by COM imported calls 
	/// </summary>
	enum AssemblyCommitFlags
	{
		Default,
		Force 
	}
	/// <summary>
	/// AssemblyCacheFlags; Used by COM imported calls
	/// </summary>
	[Flags]
	internal enum AssemblyCacheFlags
	{
		GAC = 2
	}
	/// <summary>
	/// AssemblyCacheUninstallDisposition; Used by COM imported calls
	/// </summary>
	enum AssemblyCacheUninstallDisposition
	{
		Unknown,                 
		Uninstalled,             
		StillInUse,              
		AlreadyUninstalled,      
		DeletePending,           
		HasInstallReference,     
		ReferenceNotFound,       
	}

	/// <summary>
	/// CreateAssemblyNameObjectFlags; Used by COM imported calls
	/// </summary>
	internal enum CreateAssemblyNameObjectFlags 
	{
		CANOF_DEFAULT,           
		CANOF_PARSE_DISPLAY_NAME,
		CANOF_SET_DEFAULT_VALUES
	}
	/// <summary>
	/// AssemblyNameDisplayFlags; Used by COM imported calls
	/// </summary>
	[Flags]
	internal enum AssemblyNameDisplayFlags
	{   
		VERSION					= 0x01,
		CULTURE					= 0x02,
		PUBLIC_KEY_TOKEN		= 0x04,
		PROCESSORARCHITECTURE	= 0x20,
		RETARGETABLE			= 0x80,
		ALL						= VERSION 
			| CULTURE 
			| PROCESSORARCHITECTURE
			| PUBLIC_KEY_TOKEN 
			| RETARGETABLE
	}

	#endregion

	#region InstallReference
	/// <summary>
	/// InstallReference + struct initialization; Used by COM imported calls
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	class InstallReference
	{
		int         cbSize;
		//int         flags;
		Guid        guidScheme;
		[MarshalAs(UnmanagedType.LPWStr)]
		string      identifier;
		[MarshalAs(UnmanagedType.LPWStr)]
		string      nonCannonicalData;

		public InstallReference(Guid guid, string id, string data)
		{
			cbSize				= (int)(2 * IntPtr.Size + 16 + (id.Length + data.Length) * 2);
			//flags				= 0;
			guidScheme			= guid;
			identifier			= id;
			nonCannonicalData	= data;
		}

		public Guid GuidScheme
		{
			get { return guidScheme;}
		}
	}

	#endregion

	#region Structs

	/// <summary>
	/// AssemblyInfo; Used by COM imported calls
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct AssemblyInfo
	{
		public int      cbAssemblyInfo; 
		public int      assemblyFlags;
		public long     assemblySizeInKB;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string   currentAssemblyPath;
		public int      cchBuf; 
	}

	#endregion

	#region Class InstallReferenceGuid
	/// <summary>
	/// InstallReferenceGuid; Used by COM imported calls
	/// </summary>
	[ComVisible(false)]
	class InstallReferenceGuid
	{
		public static bool IsValidGuidScheme(Guid guid)
		{
			return (guid.Equals(UninstallSubkeyGuid)	||
				guid.Equals(FilePathGuid)           ||
				guid.Equals(OpaqueStringGuid)       ||
				guid.Equals(Guid.Empty));
		}
   
		public readonly static Guid UninstallSubkeyGuid  = new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
		public readonly static Guid FilePathGuid         = new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
		public readonly static Guid OpaqueStringGuid     = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
	}
	#endregion

	#region Class AssemblyCache
	/// <summary>
	///  Helper calss for IAssemblyCache
	/// </summary>
	[ComVisible(false)]
	class AssemblyCache
	{
		// If you use this, fusion will do the streaming & commit
		public static void InstallAssembly(string assemblyPath, InstallReference reference, AssemblyCommitFlags flags)
		{
			if (reference != null) 
			{
				if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
					throw new ArgumentException("Invalid argument( reference guid).");
			}

			IAssemblyCache asmCache = null;

			COM.CheckHR(CreateAssemblyCache(out asmCache, 0));
			COM.CheckHR(asmCache.InstallAssembly((int)flags, assemblyPath, reference));
		}

		public static void UninstallAssembly(string assemblyName, InstallReference reference, out AssemblyCacheUninstallDisposition disp)
		{
			AssemblyCacheUninstallDisposition dispResult = AssemblyCacheUninstallDisposition.Uninstalled;
			if (reference != null) 
			{
				if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
					throw new ArgumentException("Invalid argument (reference guid).");
			}
            
			IAssemblyCache asmCache = null;   
      
			COM.CheckHR(CreateAssemblyCache(out asmCache, 0));
			COM.CheckHR(asmCache.UninstallAssembly(0, assemblyName, reference, out dispResult));

			disp = dispResult;
		}

		public static string QueryAssemblyInfo(string assemblyName)
		{
			if (assemblyName == null) 
			{
				throw new ArgumentException("Invalid argument (assemblyName)");
			}

			AssemblyInfo aInfo = new AssemblyInfo();
			aInfo.cchBuf = 1024;
			aInfo.currentAssemblyPath = "Path".PadLeft(aInfo.cchBuf) ;

			IAssemblyCache ac = null;
			COM.CheckHR(CreateAssemblyCache(out ac, 0));
			COM.CheckHR(ac.QueryAssemblyInfo(0, assemblyName, ref aInfo));
			
			return aInfo.currentAssemblyPath;
		}
        
		[DllImport("fusion.dll")]
		internal static extern int CreateAssemblyCache( out IAssemblyCache ppAsmCache, int reserved);
	}

	#endregion 

	#region Class AssemblyEnum
	/// <summary>
	/// Helper calss for IAssemblyEnum
	/// </summary>
	[ComVisible(false)]
	class AssemblyEnum
	{
		public AssemblyEnum(string sAsmName) 
		{
			IAssemblyName asmName = null;
			if (sAsmName != null)	//if no name specified all ssemblies will be returned
			{	
				COM.CheckHR(CreateAssemblyNameObject( out asmName, sAsmName, CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero));
			}
			COM.CheckHR(CreateAssemblyEnum( out m_assemblyEnum, IntPtr.Zero, asmName, AssemblyCacheFlags.GAC, IntPtr.Zero));
		}

		public string GetNextAssembly()
		{
			string retval = null;
			if (!m_done) 
			{
				IAssemblyName asmName = null;
				COM.CheckHR(m_assemblyEnum.GetNextAssembly((IntPtr) 0, out asmName, 0));

				if (asmName != null) 
					retval = GetFullName(asmName);

				m_done = (retval != null);
			}			
			return retval;
		}
    
		private string GetFullName(IAssemblyName asmName)
		{
			StringBuilder fullName = new StringBuilder(1024); 
			int iLen = fullName.Capacity;
			COM.CheckHR(asmName.GetDisplayName(fullName, ref iLen, (int)AssemblyNameDisplayFlags.ALL));

			return fullName.ToString();
		}

		[DllImport("fusion.dll")]
		internal static extern int CreateAssemblyEnum(	out IAssemblyEnum ppEnum, 
			IntPtr pUnkReserved, 
			IAssemblyName pName, 
			AssemblyCacheFlags flags, 
			IntPtr pvReserved);

		[DllImport("fusion.dll")]
		internal static extern int CreateAssemblyNameObject(	out IAssemblyName ppAssemblyNameObj, 
			[MarshalAs(UnmanagedType.LPWStr)]
			string szAssemblyName, 
			CreateAssemblyNameObjectFlags flags, 
			IntPtr pvReserved);

		private bool m_done;
		private IAssemblyEnum m_assemblyEnum = null;
	}
	#endregion}
}