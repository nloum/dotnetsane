// 
//  Main.cs
//  
//  Author:
//       Jonathan Taylor <jont@evrichart.com>
// 
//  Copyright (c) 2011 EvriChart Inc
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
namespace SaneLow
{
	/// <summary>
	/// Low level helpers for communicating with the backend sane library
	/// </summary>
	/// <remarks>
	/// The code here is messy, convoluted, and probably error prone... Beware!!!
	/// </remarks>
	class Helpers
	{
		/// <summary>
		/// Retrieve an array of known devices
		/// </summary>
		/// <returns>
		/// A <see cref="API.SANE_Device[]"/>
		/// </returns>
		public static API.SANE_Device[] getDeviceList()
		{
			IntPtr p_devices = IntPtr.Zero;
			API.SANE_Status s = API.sane_get_devices(ref p_devices,true);
			assert_status(s);
			API.SANE_Device[] devs = PtrToDeviceArray(p_devices);
			return devs;
		}
		/// <summary>
		/// Obtains the current scan parameters
		/// </summary>
		/// <remarks>
		/// See section 4.3.8 of the sane documentation
		/// </remarks>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <returns>
		/// A <see cref="API.SANE_Parameters"/>
		/// </returns>
		public static API.SANE_Parameters getParameters(IntPtr handle)
		{
			API.SANE_Parameters parms = new API.SANE_Parameters();
			GCHandle gc = GCHandle.Alloc(parms,GCHandleType.Pinned);
			API.sane_get_parameters3(handle,gc.AddrOfPinnedObject());//Is pinning right here?
			parms=(API.SANE_Parameters)Marshal.PtrToStructure(gc.AddrOfPinnedObject(),typeof(API.SANE_Parameters));
			gc.Free();
			return parms;
		}
		/// <summary>
		/// Throw a System.Exception if the given status is not SANE_STATUS_GOOD
		/// </summary>
		/// <param name="s">
		/// A <see cref="API.SANE_Status"/>
		/// </param>
		private static void assert_status(API.SANE_Status s)
		{
			if(!status_ok(s))
			{
				throw new Exception("BAD STATUS: " + getStatusStr(s));
			}
		}
		/// <summary>
		/// Check if the given status == SANE_STATUS_GOOD
		/// </summary>
		/// <param name="s">
		/// A <see cref="API.SANE_Status"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		private static bool status_ok(API.SANE_Status s)
		{
			return s == API.SANE_Status.SANE_STATUS_GOOD;
		}
		/// <summary>
		/// Check if the given SANE_Frame is a basic frame
		/// </summary>
		/// <param name="frame">
		/// A <see cref="API.SANE_Frame"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		private static bool is_basicframe(API.SANE_Frame frame)
		{
			return (frame == API.SANE_Frame.SANE_FRAME_GRAY||
			        frame == API.SANE_Frame.SANE_FRAME_RGB||
			        frame == API.SANE_Frame.SANE_FRAME_RED||
			        frame==API.SANE_Frame.SANE_FRAME_GREEN||
			        frame== API.SANE_Frame.SANE_FRAME_BLUE);
		}
		/// <summary>
		/// Set a SANE_String option on the device pointed to by handle
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="op">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="val">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool setOption(IntPtr handle, int op, string val)
		{
			int nAction=0;
			StringBuilder builder = new StringBuilder(val);
			API.SANE_Status status = API.sane_control_optionSTRB(handle,op,API.SANE_Action.SANE_ACTION_SET_VALUE,builder,ref nAction);
			assert_status(status);
			return true;
		}
		/// <summary>
		/// Set an integer value on the device pointed to by handle
		/// </summary>
		/// <remarks>
		/// This should also work for booleans and fixed values (After being "unfixed")
		/// </remarks>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="op">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="val">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool setOption(IntPtr handle, int op, int val)
		{
			int nAction=(int)API.SANE_Info.RELOAD_OPTIONS;
			API.SANE_Status status=API.sane_control_optionINT(handle,op,API.SANE_Action.SANE_ACTION_SET_VALUE,ref val,ref nAction);
			assert_status(status);
			return (status == API.SANE_Status.SANE_STATUS_GOOD);
		}
		/// <summary>
		/// Set a boolean value on the device pointed to by handle
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="op">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="val">
		/// A <see cref="System.Boolean"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool setOption(IntPtr handle, int op, bool val)
		{
			int nAction=0;
			API.SANE_Status status=API.sane_control_optionBOOL(handle,op,API.SANE_Action.SANE_ACTION_SET_VALUE,ref val,ref nAction);
			return (status == API.SANE_Status.SANE_STATUS_GOOD);
		}
		/// <summary>
		/// Set a "Fixed" option on the device pointed to by handle
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="op">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="val">
		/// A <see cref="System.Single"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool setOption(IntPtr handle, int op, float val) //fixed
		{
			//TODO: The provided float needs to be converted to int before hitting the API - See unfix 
			
			/*int nAction=0;
			
			API.SANE_Status status=API.sane_control_option(handle,op,API.SANE_Action.SANE_ACTION_SET_VALUE,ref val,ref nAction);
			return (status == API.SANE_Status.SANE_STATUS_GOOD);*/
			return false;
		}
		/// <summary>
		/// DON'T USE (It's not needed)
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="op">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool setDefault(IntPtr handle, int op)
		{
			int nAction=0;
			//int val=0;
			IntPtr val = IntPtr.Zero;
			API.SANE_Status status=API.sane_control_option(handle,op,API.SANE_Action.SANE_ACTION_SET_AUTO,out val,ref nAction);
			return (status == API.SANE_Status.SANE_STATUS_GOOD);
		}
		/// <summary>
		/// Prints a pipe delimited list of acceptable values for the given descriptor
		/// </summary>
		/// <param name="desc">
		/// A <see cref="API.SANE_Option_Descriptor"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string getValidOptionsStr(API.SANE_Option_Descriptor desc)
		{
			if(desc.constraint_type == API.SANE_Constraint_Type.SANE_CONSTRAINT_STRING_LIST)
			{
				IntPtr strList=desc.string_list;
				int count = 0;
				while(Marshal.ReadIntPtr(strList,count*IntPtr.Size) != IntPtr.Zero)
				{
					++count;
				}
				if(count < 0)
				throw new ArgumentOutOfRangeException("Count","< 0");
				if(strList == IntPtr.Zero)
					return "";
				string[] members = new string[count];
				for(int i = 0; i < count;++i)
				{
					IntPtr s = Marshal.ReadIntPtr(strList,i*IntPtr.Size);
					//members[i] = PtrToDevice(s);
					members[i]=Marshal.PtrToStringAuto(s);
				}
				return string.Join("|",members);
			}else if(desc.constraint_type == API.SANE_Constraint_Type.SANE_CONTRAINT_RANGE)
			{
				IntPtr rPtr = desc.string_list;
				API.SANE_Range range = (API.SANE_Range)Marshal.PtrToStructure(rPtr,typeof(API.SANE_Range));
				if(desc.type == API.SANE_Value_Type.FIXED)
				{
					return "Range: " + Math.Round(unfix(range.min),2).ToString() + "..." + Math.Round(unfix(range.max),2);
				}
				return "RANGE: " + range.min.ToString() + "..." + range.max.ToString();
				
			}else if(desc.constraint_type == API.SANE_Constraint_Type.SANE_CONTRAINT_WORD_LIST)
			{
				//return "*** WORD ***";
				//WORD is a list of integers
				IntPtr strList=desc.string_list;
				int count = 0;
				while(Marshal.ReadIntPtr(strList,count*sizeof(int)) != IntPtr.Zero)
				{
					++count;
				}
				if(count < 0)
				throw new ArgumentOutOfRangeException("Count","< 0");
				if(strList == IntPtr.Zero)
					return "";
				string[] members = new string[count-1];
				for(int i = 1; i < count;++i)
				{
					int myint = Marshal.ReadInt32(strList,i*sizeof(int));
					members[i-1] = myint.ToString();
				}
				return string.Join("|",members);
			}else{
				return "";
			}
			return "";
			
		}
		/// <summary>
		/// Retrieve an array of the string representations of the valid options for the given descriptor
		/// </summary>
		/// <param name="desc">
		/// A <see cref="API.SANE_Option_Descriptor"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String[]"/>
		/// </returns>
		public static string[] getValidOptionStringList(API.SANE_Option_Descriptor desc)
		{
			IntPtr strList=desc.string_list;
			int count = 0;
			while(Marshal.ReadIntPtr(strList,count*IntPtr.Size) != IntPtr.Zero)
			{
				++count;
			}
			if(count < 0)
			throw new ArgumentOutOfRangeException("Count","< 0");
			string[] members = new string[count];
			for(int i = 0; i < count;++i)
			{
				IntPtr s = Marshal.ReadIntPtr(strList,i*IntPtr.Size);
				//members[i] = PtrToDevice(s);
				members[i]=Marshal.PtrToStringAuto(s);
			}
			return members;
		}
		/// <summary>
		/// Retrieve the range of acceptable values for the given descriptor
		/// </summary>
		/// <param name="desc">
		/// A <see cref="API.SANE_Option_Descriptor"/>
		/// </param>
		/// <returns>
		/// A <see cref="API.SANE_Range"/>
		/// </returns>
		public static API.SANE_Range getValidOptionRange(API.SANE_Option_Descriptor desc)
		{
			IntPtr rPtr = desc.string_list;
			API.SANE_Range range = (API.SANE_Range)Marshal.PtrToStructure(rPtr,typeof(API.SANE_Range));
			
			return range;
		}
		/// <summary>
		/// Print all options in a format similar to that of scanimage
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="d">
		/// A <see cref="API.SANE_Device"/>
		/// </param>
		public static void printOptions(IntPtr handle,API.SANE_Device d)
		{
			int cnt = getOptionInt(handle,d,0);
			Console.WriteLine("{0} has {1} Options",d.name,cnt.ToString());
			for(int i = 1; i<cnt;++i)
			{
				API.SANE_Option_Descriptor desc = getOptionDescriptor(handle,i);
				//Set default values if capable
				string advstr="";
				if(hasCapability(desc,API.SANE_Capability.ADVANCED))
				{
					//Console.WriteLine("HAS AUTO CAP");
					advstr="(ADVANCED)";
				}
				string mval="";
				switch(desc.type)
				{
				case API.SANE_Value_Type.STRING:
					mval=getOptionStr(handle,i);
					break;
				case API.SANE_Value_Type.BOOL:
					mval=(getOptionBool(handle,i)?"TRUE":"FALSE");
					break;
				case API.SANE_Value_Type.INT:
					mval=getOptionInt(handle,d,i).ToString();
					break;
				case API.SANE_Value_Type.FIXED:
					//mval=getOptionFixed(handle,d,i).ToString();
					mval=string.Format("{0}",System.Math.Round(getOptionFixed(handle,d,i),2));
					//mval=getOptionStr(handle,d,i);
					break;
				case API.SANE_Value_Type.GROUP:
					continue;
				case API.SANE_Value_Type.BUTTON:
					mval="[BUTTON]";
					break;
				default:
					mval="[UNKNOWN TYPE]";
					break;
				}
				if(hasCapability(desc,API.SANE_Capability.INACTIVE))continue;
				setDefault(handle,i);
				Console.WriteLine("OP: {0} {1}",desc.title,advstr);
				Console.WriteLine("\tName: {0}",desc.name);
				Console.WriteLine("\tDesc: {0}",desc.desc);
				Console.WriteLine("\tSize: {0}",desc.size.ToString());
				Console.WriteLine("\ttype: {0}",desc.type.ToString());
				Console.WriteLine("\tuint: {0}",desc.unit.ToString());
				Console.WriteLine("\tValue: {0}",mval);
				if(desc.constraint_type != API.SANE_Constraint_Type.SANE_CONSTRAINT_NONE)
				{
					Console.WriteLine("\tPossible: {0}",getValidOptionsStr(desc));
				}
				Console.WriteLine("--------------------------------------");
				
			}
			
		}
		/// <summary>
		/// Check whether the given option descriptor supports the given capability
		/// </summary>
		/// <param name="des">
		/// A <see cref="API.SANE_Option_Descriptor"/>
		/// </param>
		/// <param name="cap">
		/// A <see cref="API.SANE_Capability"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool hasCapability(API.SANE_Option_Descriptor des,API.SANE_Capability cap)
		{
			return (des.cap & cap) != API.SANE_Capability.NONE;
			
		}
		/// <summary>
		/// Get the number of devices returned by <see cref="API.sane_get_devices"/>
		/// </summary>
		/// <param name="array">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		private static int countDevices(IntPtr array)
		{
			int count = 0;
			while(Marshal.ReadIntPtr(array,count*IntPtr.Size) != IntPtr.Zero)
			{
				++count;
			}
			return count;
		}
		#region ll_options
		/// <summary>
		/// Retrieve the option descriptor for the option at index N from the device pointed to by handle
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="n">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="API.SANE_Option_Descriptor"/>
		/// </returns>
		public static API.SANE_Option_Descriptor getOptionDescriptor(IntPtr handle,int n)
		{
			return getOptionDescriptor(API.sane_get_option_descriptor(handle,n));
		}
		/// <summary>
		/// Marshal a <see cref="API.SANE_Option_Descriptor"/> from the given pointer
		/// </summary>
		/// <param name="ptr">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <returns>
		/// A <see cref="API.SANE_Option_Descriptor"/>
		/// </returns>
		private static API.SANE_Option_Descriptor getOptionDescriptor(IntPtr ptr)
		{
			if(ptr == IntPtr.Zero)
				return new API.SANE_Option_Descriptor();
			return (API.SANE_Option_Descriptor)Marshal.PtrToStructure(ptr,typeof(API.SANE_Option_Descriptor));
		}
		/// <summary>
		/// Converts a SANE_Int to a double
		/// </summary>
		/// <param name="ival">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public static double unfix(int ival)
		{
			return ((double)ival / (1 << 16)); //the 16 here represents the SANE_FIXED_SCALE_SHIFT define and may vary
		}
		/// <summary>
		/// Convers a SANE_Fixed to a SANE_Word
		/// </summary>
		/// <param name="dval">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public static int fix(double dval)
		{
			return (int)(dval * (1 << 16)); //the 16 here represents the SANE_FIXED_SCALE_SHIFT define and may vary
		}
		/// <summary>
		/// Retrieve a pointer to the value of the option at the given index
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="num">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="IntPtr"/>
		/// </returns>
		public static IntPtr getOption(IntPtr handle, int num)
		{
			API.SANE_Option_Descriptor desc = getOptionDescriptor(API.sane_get_option_descriptor(handle,num));
			if(desc.size == 0)return IntPtr.Zero;
			IntPtr vPtr = IntPtr.Zero;
			int nAction=0;
			API.SANE_Status status=API.sane_control_option(handle,num,0,out vPtr,ref nAction);
			
			if(vPtr.Equals(IntPtr.Zero))
			{
				return IntPtr.Zero;
			}
			return vPtr;
		}
		/// <summary>
		/// Retrieve the string representation of the value of the option at the given index
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="num">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string getOptionStr(IntPtr handle, int num)
		{
			StringBuilder val = new StringBuilder("");
			int action=0;
			API.sane_control_option(handle,num,0,val,ref action);
			
			return val.ToString();
		}
		/// <summary>
		/// Retrieve the value of the option at the given index as a double
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="num">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public static double getOptionFixed(IntPtr handle, int num)
		{
			IntPtr ptr = getOption(handle,num);
			if(ptr.Equals(IntPtr.Zero))return 0d;
			int l = ptr.ToInt32();//Marshal.ReadInt32(getOption(handle,d,num));
			return unfix(l);
		}
		/// <summary>
		/// Retrieve the value of the option at the given index as a boolean
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="num">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool getOptionBool(IntPtr handle, int num)
		{
			IntPtr ptr = getOption(handle,num);
			if(ptr == IntPtr.Zero)return false;
			return (ptr.ToInt32() == 1);
		}
		/// <summary>
		/// Retrieve the value of the option at the given index as an integer
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="num">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public static int getOptionInt(IntPtr handle, int num)
		{
			IntPtr ptr = getOption(handle,num);
			if(ptr == IntPtr.Zero)return 0;
			return ptr.ToInt32();
		}
		/// <summary>
		/// Retrieve a pointer to the value of the option at the given index for the given device
		/// </summary>
		/// <param name="handle">
		/// A <see cref="IntPtr"/>
		/// </param>
		/// <param name="d">
		/// A <see cref="API.SANE_Device"/>
		/// </param>
		/// <param name="num">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="IntPtr"/>
		/// </returns>
		public static IntPtr getOption(IntPtr handle,API.SANE_Device d, int num)
		{
			API.SANE_Option_Descriptor desc = getOptionDescriptor(API.sane_get_option_descriptor(handle,num));
			if(desc.size == 0)return IntPtr.Zero;
			IntPtr vPtr = IntPtr.Zero;
			int nAction=0;
			API.SANE_Status status=API.sane_control_option(handle,num,0,out vPtr,ref nAction);
			assert_status(status);
			if(vPtr.Equals(IntPtr.Zero))
			{
				return IntPtr.Zero;
			}
			return vPtr;
		}
		public static string getOptionStr(IntPtr handle,API.SANE_Device d, int num)
		{
			StringBuilder val = new StringBuilder("");
			int action=0;
			API.sane_control_option(handle,num,0,val,ref action);
			
			return val.ToString();
		}
		
		public static double getOptionFixed(IntPtr handle,API.SANE_Device d, int num)
		{
			IntPtr ptr = getOption(handle,num);
			if(ptr.Equals(IntPtr.Zero))return 0d;
			int l = ptr.ToInt32();//Marshal.ReadInt32(getOption(handle,d,num));
			return unfix(l);
		}
		
		public static bool getOptionBool(IntPtr handle,API.SANE_Device d, int num)
		{
			IntPtr ptr = getOption(handle,num);
			if(ptr == IntPtr.Zero)return false;
			return (ptr.ToInt32() == 1);
		}
		public static int getOptionInt(IntPtr handle,API.SANE_Device d, int num)
		{
			IntPtr ptr = getOption(handle,num);
			if(ptr == IntPtr.Zero)return 0;
			return ptr.ToInt32();
		}
		#endregion ll_options
		#region ll_device
		private static API.SANE_Device[] PtrToDeviceArray(IntPtr array)
		{
			if(array == IntPtr.Zero)
				return new API.SANE_Device[]{};
			int cnt = countDevices(array);
			return PtrToDeviceArray(array,cnt);
		}
		private static API.SANE_Device[] PtrToDeviceArray(IntPtr array,int count)
		{
			if(count < 0)
				throw new ArgumentOutOfRangeException("Count","< 0");
			if(array == IntPtr.Zero)
				return new API.SANE_Device[count];
			API.SANE_Device[] members = new API.SANE_Device[count];
			for(int i = 0; i < count;++i)
			{
				IntPtr s = Marshal.ReadIntPtr(array,i*IntPtr.Size);
				members[i] = PtrToDevice(s);
				//Marshal.FreeCoTaskMem(s);
			}
			return members;
		}
		private static API.SANE_Device PtrToDevice(IntPtr ptr)
		{
			if(ptr == IntPtr.Zero)
				return new API.SANE_Device();
			API.SANE_Device device = (API.SANE_Device)Marshal.PtrToStructure(ptr,typeof(API.SANE_Device));
			return device;
		}
		#endregion ll_device
		public static string getStatusStr(API.SANE_Status status)
		{
			IntPtr ptr = API.sane_strstatus(status);
			return Marshal.PtrToStringAnsi(ptr);
		}
		
	}
	public class API
	{
		public enum SANE_Status:uint
		{
			SANE_STATUS_GOOD=0,
			SANE_STATUS_UNSUPPORTED=1,
			SANE_STATUS_CANCELLED=2,
			SANE_STATUS_DEVICE_BUSY=3,
			SANE_STATUS_DEVICE_INVAL=4,
			SANE_STATUS_DEVICE_EOF=5,
			SANE_STATUS_DEVICE_JAMMED=6,
			SANE_STATUS_NO_DOCS=7,
			SANE_STATUS_COVER_OPEN=8,
			SANE_STATUS_IO_ERROR=9,
			SANE_STATUS_NO_MEM=10,
			SANE_STATUS_ACCESS_DENIED=11
		}
		public enum SANE_Value_Type:uint
		{
			BOOL=0,
			INT=1,
			FIXED=2,
			STRING=3,
			BUTTON=4,
			GROUP=5
		}
		public enum SANE_Unit:uint
		{
			SANE_UNIT_NONE=0,
			SANE_UNIT_PIXEL=1,
			SANE_UNIT_BIT=2,
			SANE_UNIT_MM=3,
			SANE_UNIT_DPI=4,
			SANE_UNIT_PERCENT=5,
			SANE_UNIT_MICROSECOND=6
		}
		[Flags]
		public enum SANE_Capability:uint
		{
			NONE=0,
			SOFT_SELECT=1,
			HARD_SELECT=2,
			DETECT=4,
			EMULATED=8,
			AUTOMATIC=16,
			INACTIVE=32,
			ADVANCED=64
		}
		public enum SANE_Info:uint{
			INEXACT=1,
			RELOAD_OPTIONS=2,
			RELOAD_PARAMS=4
		}
		public enum SANE_Frame:int{
			SANE_FRAME_GRAY=0,
			SANE_FRAME_RGB=1,
			SANE_FRAME_RED=2,
			SANE_FRAME_GREEN=3,
			SANE_FRAME_BLUE=4
		}
			
		public enum SANE_Constraint_Type:uint
		{
			SANE_CONSTRAINT_NONE=0,
			SANE_CONTRAINT_RANGE=1,
			SANE_CONTRAINT_WORD_LIST=2,
			SANE_CONSTRAINT_STRING_LIST=3
		}
		public enum SANE_Action:uint
		{
			SANE_ACTION_GET_VALUE=0,
			SANE_ACTION_SET_VALUE=1,
			SANE_ACTION_SET_AUTO=2
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct SANE_Range
		{
			public int min;
			public int max;
			public int quant;
		}
		[StructLayout(LayoutKind.Explicit)]
		public struct SANE_Parameters
		{
			[MarshalAs(UnmanagedType.SysInt),FieldOffset(0)]
			public SANE_Frame format;
			[FieldOffset(4)]
			public int last_frame;
			[FieldOffset(8)]
			public int bytes_per_line;
			[FieldOffset(12)]
			public int pixels_per_line;
			[FieldOffset(16)]
			public int lines;
			[FieldOffset(20)]
			public int depth;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct SANE_Device
		{
			public string name;
			public string vendor;
			public string model;
			public string type;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct SANE_Option_Descriptor
		{
			public string name;
			public string title;
			public string desc;
			public SANE_Value_Type type;
			public SANE_Unit unit;
			public int size;
			public SANE_Capability cap;
			public SANE_Constraint_Type constraint_type;
			public IntPtr string_list; //Union removed.  use string_list for all your inner lists
		};
		
		[DllImport("libsane")]
		public extern static SANE_Status sane_init(out int version,IntPtr cb);
		[DllImport("libsane")]
		public extern static void sane_exit();
		[DllImport("libsane")]
		public extern static IntPtr sane_strstatus(SANE_Status s);
		[DllImport("libsane")]
		public extern static SANE_Status sane_get_devices(ref IntPtr device_list, bool local_only);
		[DllImport("libsane")]
		public extern static SANE_Status sane_open(string name, out IntPtr handle);
		[DllImport("libsane")]
		public extern static void sane_close(IntPtr handle);
		[DllImport("libsane")]
		public extern static SANE_Status sane_control_option(IntPtr handle, int n, SANE_Action a, out IntPtr v, ref int i);
		[DllImport("libsane",EntryPoint="sane_control_option")]
		public extern static SANE_Status sane_control_option2(IntPtr handle, int n, SANE_Action a, ref IntPtr v, ref int i);
		[DllImport("libsane",EntryPoint="sane_control_option")]
		public extern static SANE_Status sane_control_optionINT(IntPtr handle, int n, SANE_Action a, ref int v, ref int i);
		[DllImport("libsane",EntryPoint="sane_control_option")]
		public extern static SANE_Status sane_control_optionSTR(IntPtr handle, int n, SANE_Action a, ref string v, ref int i);
		[DllImport("libsane",EntryPoint="sane_control_option")]
		public extern static SANE_Status sane_control_optionSTRB(IntPtr handle, int n, SANE_Action a, StringBuilder v, ref int i);
		[DllImport("libsane",EntryPoint="sane_control_option")]
		public extern static SANE_Status sane_control_optionFIXED(IntPtr handle, int n, SANE_Action a, ref int v, ref int i);
		[DllImport("libsane",EntryPoint="sane_control_option")]
		public extern static SANE_Status sane_control_optionBOOL(IntPtr handle, int n, SANE_Action a, ref bool v, ref int i);
		[DllImport("libsane")]
		public extern static SANE_Status sane_control_option(IntPtr handle, int n, SANE_Action a, ref IntPtr v, ref IntPtr i);
		[DllImport("libsane")]
		public extern static SANE_Status sane_control_option(IntPtr handle, int n, SANE_Action a, ref float v, ref int i);
		[DllImport("libsane")]
		public extern static SANE_Status sane_control_option(IntPtr handle, int n, SANE_Action a, StringBuilder v, ref int i);
		[DllImport("libsane")]
		public extern static IntPtr sane_get_option_descriptor(IntPtr handle, int n);
		[DllImport("libsane")]
		public extern static SANE_Status sane_get_parameters(IntPtr handle, ref IntPtr p);
		[DllImport("libsane",EntryPoint="sane_get_parameters")]
		public extern static SANE_Status sane_get_parameters2(IntPtr handle, [In,Out,MarshalAs(UnmanagedType.LPStruct)] SANE_Parameters p);
		[DllImport("libsane",EntryPoint="sane_get_parameters")]
		public extern static SANE_Status sane_get_parameters3(IntPtr handle, IntPtr p);
		//Scanning ops
		[DllImport("libsane")]
		public extern static SANE_Status sane_start(IntPtr handle);
		[DllImport("libsane")]
		public extern static SANE_Status sane_read(IntPtr handle,out IntPtr buf, int maxlen, ref int len);
		[DllImport("libsane",EntryPoint="sane_read")]
		public extern static SANE_Status sane_read2(IntPtr handle, [Out,MarshalAs(UnmanagedType.LPArray,SizeConst=32768)] byte[] buf, int maxlen, ref int len);
		//public extern static SANE_Status sane_read(IntPtr handle,out IntPtr buf, int maxlen, out int len);
		[DllImport("libsane")]
		public extern static void sane_cancel(IntPtr handle);
		[DllImport("libsane")]
		public extern static SANE_Status sane_set_io_mode(IntPtr handle, bool async);
		/*
		 * IntPtr ptr = CreateFile(filename,access, share,0, mode,0, IntPtr.Zero);
		/* Is bad handle? INVALID_HANDLE_VALUE *
		 *if (ptr.ToInt32() == -1)
		 *{
		 *   /* ask the framework to marshall the win32 error code to an exception *
		 *   Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		 *}
		 *else
		 *{
		 *    return new FileStream(ptr,access);
		 *}*/
		[DllImport("libsane")]
		public extern static SANE_Status sane_get_select_fd(IntPtr handle, out IntPtr fd); //TODO: Implement this
	}
 }