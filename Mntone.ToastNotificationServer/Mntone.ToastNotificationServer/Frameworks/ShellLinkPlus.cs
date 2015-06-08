using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Mntone.ToastNotificationServer.Frameworks
{
	public sealed class ShellLink : IDisposable
	{
		#region Win32 and COM

		// IShellLink Interface
		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214F9-0000-0000-C000-000000000046")]
		private interface IShellLinkW
		{
			void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, ref WIN32_FIND_DATAW pfd, uint fFlags);
			IntPtr GetIDList();
			void SetIDList(IntPtr pidl);
			void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName,
								 int cchMaxName);
			void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
			void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
									  int cchMaxPath);
			void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
			void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
							   int cchMaxPath);
			void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
			void GetHotKey(out ushort pwHotkey);
			void SetHotKey(ushort wHotKey);
			void GetShowCmd(out int piShowCmd);
			void SetShowCmd(int iShowCmd);
			void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
								  int cchIconPath, out int piIcon);
			void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
			void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
								  uint dwReserved);
			void Resolve(IntPtr hwnd, uint fFlags);
			void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
		}

		// ShellLink CoClass (ShellLink object)
		[ComImport]
		[ClassInterface(ClassInterfaceType.None)]
		[Guid("00021401-0000-0000-C000-000000000046")]
		private class CShellLink { }

		// WIN32_FIND_DATAW Structure
		[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
		private struct WIN32_FIND_DATAW
		{
			public uint dwFileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}

		// IPropertyStore Interface
		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
		private interface IPropertyStore
		{
			uint GetCount();
			PropertyKey GetAt([In] uint iProp);
			void GetValue([In] ref PropertyKey key, [Out] PropVariant pv);
			void SetValue([In] ref PropertyKey key, [In] PropVariant pv);
			void Commit();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct PropertyKey
		{
			public readonly Guid formatId;
			public readonly int propertyId;

			public PropertyKey(Guid formatId, int propertyId)
			{
				this.formatId = formatId;
				this.propertyId = propertyId;
			}

			public PropertyKey(string formatId, int propertyId)
			{
				this.formatId = new Guid(formatId);
				this.propertyId = propertyId;
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		private sealed class PropVariant : IDisposable
		{
			[FieldOffset(0)]
			private ushort valueType;
			[FieldOffset(8)]
			private IntPtr ptr;

			public PropVariant()
			{ }

			// Construct with string value
			public PropVariant(string value)
			{
				if (value == null)
				{
					throw new ArgumentException("Failed to set value.");
				}

				valueType = (ushort)VarEnum.VT_LPWSTR;
				ptr = Marshal.StringToCoTaskMemUni(value);
			}

			~PropVariant()
			{
				Dispose();
			}

			public void Dispose()
			{
				PropVariantClear(this);
				GC.SuppressFinalize(this);
			}

			public VarEnum VarType
			{
				get { return (VarEnum)valueType; }
				set { valueType = (ushort)value; }
			}

			public bool IsNullOrEmpty
			{
				get { return valueType == (ushort)VarEnum.VT_EMPTY || valueType == (ushort)VarEnum.VT_NULL; }
			}

			public string Value
			{
				get { return Marshal.PtrToStringUni(ptr); }
			}
		}

		[DllImport("Ole32.dll")]
		private extern static void PropVariantClear([In, Out] PropVariant pvar);

		#endregion

		private const int MAX_PATH = 260;
		private const int INFOTIPSIZE = 1024;

		private const int STGM_READ = 0x00000000;     // STGM constants
		private const uint SLGP_UNCPRIORITY = 0x0002; // SLGP flags

		private readonly PropertyKey AppUserModelIDKey = new PropertyKey("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}", 5);

		private IShellLinkW _shellLinkW = null;



		public ShellLink() : this(null)
		{ }

		public ShellLink(string file)
		{
			try
			{
				this._shellLinkW = (IShellLinkW)new CShellLink();
			}
			catch
			{
				throw new COMException("Failed to create ShellLink object.");
			}

			if (file != null)
			{
				this.Load(file);
			}
		}

		~ShellLink()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this._shellLinkW != null)
			{
				// Release all references.
				Marshal.FinalReleaseComObject(_shellLinkW);
				this._shellLinkW = null;
			}
		}

		private IPersistFile PersistFile
		{
			get
			{
				var persistFile = _shellLinkW as IPersistFile;
				if (persistFile == null)
				{
					throw new COMException("Failed to create IPersistFile.");
				}

				return persistFile;
			}
		}

		private IPropertyStore PropertyStore
		{
			get
			{
				var propertyStore = _shellLinkW as IPropertyStore;
				if (propertyStore == null)
				{
					throw new COMException("Failed to create IPropertyStore.");
				}

				return propertyStore;
			}
		}

		public string ShortcutFile
		{
			get
			{
				string ret;
				this.PersistFile.GetCurFile(out ret);
				return ret;
			}
		}

		public string TargetPath
		{
			get
			{
				var targetPath = new StringBuilder(MAX_PATH);
				var data = new WIN32_FIND_DATAW();
				this._shellLinkW.GetPath(targetPath, targetPath.Capacity, ref data, SLGP_UNCPRIORITY);
				return targetPath.ToString();
			}
			set
			{
				this._shellLinkW.SetPath(value);
			}
		}


		public string Arguments
		{
			get
			{
				var arguments = new StringBuilder(INFOTIPSIZE);
				_shellLinkW.GetArguments(arguments, arguments.Capacity);
				return arguments.ToString();
			}
			set { _shellLinkW.SetArguments(value); }
		}

		public string AppUserModelID
		{
			get
			{
				using (var pv = new PropVariant())
				{
					this.PropertyStore.GetValue(AppUserModelIDKey, pv);
					return pv.Value == null ? "Null" : pv.Value;
				}
			}
			set
			{
				using (PropVariant pv = new PropVariant(value))
				{
					this.PropertyStore.SetValue(AppUserModelIDKey, pv);
					this.PropertyStore.Commit();
				}
			}
		}

		public void Load(string file)
		{
			if (!File.Exists(file))
			{
				throw new FileNotFoundException("File is not found.", file);
			}
			else
			{
				PersistFile.Load(file, STGM_READ);
			}
		}

		public void Save()
		{
			var file = this.ShortcutFile;
			if (file == null)
			{
				throw new InvalidOperationException("File name is not given.");
			}

			this.Save(file);
		}

		public void Save(string file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("File name is required.");
			}

			this.PersistFile.Save(file, true);
		}

		// Verify if operation succeeded.
		public static void VerifySucceeded(uint hresult)
		{
			if (hresult > 1)
			{
				throw new InvalidOperationException("Failed with HRESULT: " + hresult.ToString("X"));
			}
		}
	}

}
