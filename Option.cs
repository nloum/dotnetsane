// 
//  testclass.cs
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
using SaneLow;
using System.Collections.Generic;
namespace Sane
{
	/// <summary>
	/// Wraps a SANE_Option_Descriptor
	/// </summary>
	/// <remarks>
	public class Option
	{
		public enum OptionType{
			BOOL=0,
			INT=1,
			FIXED=2,
			STRING=3,
			BUTTON=4,
			GROUP=5
		}
		public enum Capabilities{
			NONE=0,
			SOFT_SELECT=1,
			HARD_SELECT=2,
			DETECT=4,
			EMULATED=8,
			AUTOMATIC=16,
			INACTIVE=32,
			ADVANCED=64
		}
		public enum Constraint{
			NONE=0,
			RANGE=1,
			WORD_LIST=2,
			STRING_LIST=3
		}
		private const string TAG="Sane.Option";
		private List<string> _validOptions;
		private string _innerDesc;
		private string _innerName;
		private string _innerTitle;
		private Constraint _constraint;
		private IntPtr _handle;
		private API.SANE_Device _innerDevice;
		private API.SANE_Option_Descriptor _innerDescriptor;
		private int opIndex=0;
		private Capabilities _innerCap;
		public OptionType type;
		private int _innerSize;
		private string _validOpString;
		public string name{
			get{
				return _innerName;
			}
		}
		public string description{
			get{
				return _innerDesc;
			}
		}
		public string title{
			get{
				return _innerTitle;
			}
		}
		public int size{
			get{
				return _innerSize;
			}
		}
		public string validOptionsString{
			get{
				return _validOpString;
			}
		}
		public Constraint constraint{
			get{
				return _constraint;
			}
		}
		public Capabilities capability{
			get{
				return _innerCap;
			}
		}
		public Option (IntPtr handle)
		{
			_validOptions=new List<string>();
			_handle=handle;
		}
		public bool hasCapability(Capabilities _test)
		{
			//return false;
			return (_test & _innerCap) != Capabilities.NONE;
		}
		public static Option wrap(IntPtr handle,API.SANE_Option_Descriptor desc,int index)
		{
			Option o = new Option(handle);
			o._innerDesc=desc.desc;
			o._innerName=desc.name;
			o._innerSize=desc.size;
			o._innerTitle=desc.title;
			o._innerCap = (Capabilities)desc.cap;
			o._constraint = (Constraint)desc.constraint_type;
			o._validOpString = Helpers.getValidOptionsStr(desc);
			o._innerDescriptor = desc;
			o.opIndex=index;
			o.type = (OptionType)desc.type;
			return o;
		}
		public string getValue(IntPtr handle)
		{
			switch(type)
			{
			case OptionType.BOOL:
				return Helpers.getOptionBool(handle,opIndex).ToString();
			case OptionType.FIXED:
				return Helpers.getOptionFixed(handle,opIndex).ToString();
			case OptionType.INT:
				return Helpers.getOptionInt(handle,opIndex).ToString();
			case OptionType.STRING:
				return Helpers.getOptionStr(handle,opIndex).ToString();
			}
			return "";
		}
		public List<string> getValidOptionsList() //Only for Constraint.STRING_LIST or Constraint.WORD_LIST
		{
			if(_constraint == Option.Constraint.RANGE)
			{
				throw new InvalidOperationException("Use getValidRange to get the valid range for an option");
			}
			List<string> valid = new List<string>();
			if(_constraint == Option.Constraint.NONE)return valid;
			foreach(string str in Helpers.getValidOptionStringList(_innerDescriptor))
			{
				valid.Add(str);
			}
			return valid;
		}
		public Range getValidRange()
		{
			if(_constraint != Option.Constraint.RANGE)
			{
				throw new InvalidOperationException("This option is not a range");
			}
			API.SANE_Range rng = Helpers.getValidOptionRange(_innerDescriptor);
			Range r = Range.wrap(rng);
			return r;
		}
		public string[] getValidOptionsArray()
		{
			return Helpers.getValidOptionStringList(_innerDescriptor);
		}
		public List<int> getValidOptionsInt()
		{
			//throw new NotImplementedException("Retrieving a SANE_WORD_LIST is not currently supported");
			string[] parts = Helpers.getValidOptionsStr(_innerDescriptor).Split('|'); //Lazy hack
			List<int> ret = new List<int>();
			foreach(string part in parts)
			{
				ret.Add(int.Parse(part)); //FIXME: Type safety
			}
			return ret;
		}
		public bool setValue(int val)
		{
			Debugging.Debug.WriteLine(TAG,"Try set value of {0} to {1}",name,val);
			if(_constraint == Option.Constraint.RANGE)
			{
				API.SANE_Range range = Helpers.getValidOptionRange(_innerDescriptor);
				if(val < range.min || val > range.max)
				{
					throw new System.ArgumentOutOfRangeException("Value",val,"Range.min=" + range.min.ToString() + " Range Max: " + range.max.ToString());
				}
			}else if(_constraint == Option.Constraint.WORD_LIST)
			{
				
				//List of numbers
			}
			return Helpers.setOption(_handle,opIndex,val);
		}
		public bool setValue(string val)
		{
			try{
				Debugging.Debug.WriteLine(TAG,"Try set value of {0} to {1}",name,val);
				if(type != Option.OptionType.STRING)
				{
					switch(type)
					{
					case OptionType.INT:
						int intval;
						if(int.TryParse(val,out intval))
						{
							return setValue(intval);
						}else{
							throw new InvalidCastException("Unable to cast from {0} to integer in setValue(STRING)");
						}
					case OptionType.BOOL:
						int bint;
						if(int.TryParse(val,out bint) && ((bint == 0)||(bint==1)))
						{
							return setValue(bint);
						}else{
							bool op = ((val.ToLower()=="true")||(val.ToLower()=="yes"));
							return setValue((op?1:0));
						}
					case OptionType.FIXED:
						return setValue(SaneLow.Helpers.fix(double.Parse(val)));
					default:
						throw new NotImplementedException("Setting a value type other than string/integer is not currently supported");
					}
				}
				if(_constraint == Option.Constraint.STRING_LIST)
				{
					bool found=false;
					string[] list = Helpers.getValidOptionStringList(_innerDescriptor);
					for(int i=0;i<list.Length;i++)
					{
						if(list[i]==null)break; //Somethings wrong
						if(list[i].ToLower() == val.ToLower())
						{
							found=true;
							break;
						}
					}
					if(!found)
					{
						throw new Exception("Invalid string specified. valid options are: " + string.Join(" | ",list));
					}
				}
				return Helpers.setOption(_handle,opIndex,val);
			}catch(Exception ex)
			{
				Debugging.Debug.Error(TAG,"Caught exception: {0} while setting value of {1} to {2}",ex.Message,name,val);
				return false;
			}
		}
	}
}

