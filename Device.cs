// 
//  Device.cs
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
using SaneLow; //For low sane_init, sane_exit
using System.Collections.Generic; //for Dictionary and List
namespace Sane
{
	/// <summary>
	/// Represents a SANE_Device
	/// </summary>
	/// <remarks>
	/// Need to figure out a way to either open/close the device handle properly or hold it for an extended period of time without GC killing the pointer
	/// </remarks>
	public class Device : SaneObject
	{
		private const string TAG="Sane.Device";
		private string _innerName;
		private string _innerMake;
		private string _innerModel;
		private string _innerType;
		//private API.SANE_Device _innerDevice;
		private bool _open = false;
		private Dictionary<string,List<Option>>_options;
		
		public delegate void enumOptionsDelegate(Device sender,string section,Option o);
		public delegate void enumOptionGroupDelegate(Device sender,string section);
		private IntPtr handle;
		public string name{
			get{
				return _innerName;
			}
		}
		public string make{
			get{
				return _innerMake;
			}
		}
		public string model{
			get{
				return _innerModel;
			}
		}
		public string type{
			get{
				return _innerType;
			}
		}
		private Device ()
		{
			_options=new Dictionary<string,List<Option>>();
		}
		public IntPtr open()
		{
			if(!_open){
				//Console.WriteLine("Open device");
				Debugging.Debug.WriteLine(TAG,"Open Device");
				Status s = Status.wrap(API.sane_open(this.name,out handle));
				if(!s.isGood())
				{
					throw new Exception("Unable to get device handle");
				}
				_open=true;
			}
			return handle;
		}
		public void destroy()
		{
			/*if(_innerDevice != null)
			{
			
			}*/
		}
		public void close()
		{
			if(_open)
			{
				Debugging.Debug.WriteLine(TAG,"Device ({0}) Closing",name);
				API.sane_close(handle);
				Debugging.Debug.WriteLine(TAG,"Closed");
				_open=false;
			}
		}
		public Option getOption(string name)
		{
			foreach(string key in _options.Keys)
			{
				foreach(Option o in _options[key])
				{
					if(o.name == name)
					{
						return o;
					}
				}
			}
			return null;
		}
		public IntPtr getHandle()
		{
			if(!_open)
				return IntPtr.Zero;
			return handle;
		}
		public void enumOptions(enumOptionsDelegate del)
		{
			//this.open();
			//foreach(Option op in _options)
			foreach(string key in _options.Keys)
			{
				//del(this,key,_options);
				foreach(Option op in _options[key])
				{
					del(this,key,op);
				}
			}
			//this.close();
		}
		public void enumOptions(string section,enumOptionsDelegate del)
		{
			//TODO: Throw exception if !_options.hasKey(section)
			foreach(Option op in _options[section])
			{
				del(this,section,op);
			}
		}
		public void enumOptionGroups(enumOptionGroupDelegate del)
		{
			foreach(string key in _options.Keys)
			{
				del(this,key);
			}
		}
		public Dictionary<string,List<Option>> getOptions()
		{
			return _options;
		}
		public List<Option> getOptionsInSection(string section)
		{
			return _options[section];
		}
		public Status start()
		{
			return Status.wrap(API.sane_start(handle));
		}
		public Status read(int max, ref byte[] buffer,out int read)
		{
			read=0;
			
			Status s = Status.wrap(API.sane_read2(handle,buffer,max,ref read));
			return s;
		}
		public void cancel()
		{
			Debugging.Debug.WriteLine(TAG,"Attempting to cancel active job");
			SaneLow.API.sane_cancel(getHandle());
		}
		public Parameters getParameters()
		{
			API.SANE_Parameters p = Helpers.getParameters(handle);
			return Parameters.Wrap(p);
		}
		
		public static Device Wrap(API.SANE_Device d)
		{
			Device r = new Device();
			r._innerMake = d.vendor;
			r._innerModel=d.model;
			r._innerName=d.name;
			r._innerType=d.type;
			//r._innerDevice = d;
			IntPtr handle = r.open();
			string curGroup="global";
			
			int count = Helpers.getOptionInt(handle,d,0);
			Debugging.Debug.WriteLine(TAG,r.name + " Has " + count.ToString() + " Options");
			for(int i =1;i<count;i++)
			{
				Option o = Option.wrap(handle,Helpers.getOptionDescriptor(handle,i),i);
				Debugging.Debug.WriteLine(TAG,"Load option {0} (TYPE:{1}, CAP:{2})",o.name,o.type,o.capability);
				if(o.type!=Option.OptionType.GROUP)
				{
					if(!o.hasCapability(Option.Capabilities.INACTIVE))
					{
						if(!r._options.ContainsKey(curGroup))
						{
							r._options.Add(curGroup,new List<Option>());
						}
						if(r._options[curGroup] == null)
						{
							r._options[curGroup] = new List<Option>();
						}
						r._options[curGroup].Add(o);
					}
				}else{
					curGroup = o.title;
					r._options.Add(curGroup,new List<Option>());
					
				}
			}
			//API.sane_close(handle);
			return r;
		}
	}
}

