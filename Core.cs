// 
//  Core.cs
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
	/// Core sane functionality
	/// </summary>
	public class Core
	{
		private const string TAG="Sane.Core";
		private IntPtr handle;
		private int _version;
		private Dictionary<string,Device> _devices;
		private List<string> _keys;
		private bool _devicesLoaded=false;
		private bool _initialized=false;
		private Status pStatus;
		public delegate void statusChangedDel(object sender,Status s);
		public delegate void enumDevicesDelegate(Device d);
		public event statusChangedDel statusChanged;
		
		public int version{
			get{
				return _version;
			}
		}
		
		public Core ()
		{
			pStatus = new Status(API.SANE_Status.SANE_STATUS_GOOD);
			
		}
		public Status init()
		{
			_devices=new Dictionary<string, Device>();
			_keys=new List<string>();
			Debugging.Debug.WriteLine(TAG,"Calling sane_init");
			Status s=Status.wrap(API.sane_init(out _version,IntPtr.Zero)); //TODO: Implement auth callback
			if(!s.isGood())
			{
				Debugging.Debug.Error(TAG,"Throwing invalid status for: {0}",s.description);
				throw new InvalidStatusException("Bad status encountered while attempting to call sane_init",s);
			}
			_initialized=s.isGood();
			return s;
		}
		
		public void exit()
		{
			foreach(Device d in _devices.Values)
			{
				Debugging.Debug.WriteLine(TAG,"Closing device: " + d.name);
				d.close();
			}
			API.sane_exit();
			Debugging.Debug.WriteLine(TAG,"Called sane_exit");
			_initialized=false;
		}
		public void enumDevices(enumDevicesDelegate del)
		{
			enumDevices(del,false);
		}
		public void enumDevices(enumDevicesDelegate del,bool force_reload)
		{
			_loadDevices(force_reload);
			foreach(string key in _devices.Keys)
			{
				del(_devices[key]);
			}
		}
		public Device getDeviceByIndex(int index)
		{
			_loadDevices();
			return getDevice(_keys[index]);
		}
		public Device getDevice(string name)
		{
			_loadDevices();
			enumDevices(new enumDevicesDelegate(delegate(Device d){
				Debugging.Debug.WriteLine("Found device: {0}",d.name);
			}));
			return _devices[name];
		}
		private void _loadDevices()
		{
			_loadDevices(false);
		}
		private void _loadDevices(bool force)
		{
			if(_devicesLoaded && !force)return;
			Debugging.Debug.WriteLine(TAG,"Calling Helpers.getDeviceList");
			API.SANE_Device[] list = Helpers.getDeviceList();
			Debugging.Debug.WriteLine(TAG,"Helpers.getDeviceList returned {0} devices",list.Length);
			foreach(API.SANE_Device d in list)
			{
				if(_devices.ContainsKey(d.name))continue;
				_devices.Add(d.name,Device.Wrap(d));
				_keys.Add(d.name);
			}
			_devicesLoaded=true;
		}
		private void _fireStatus(Status s)
		{
			if(s.Equals(pStatus))return;
			if(statusChanged!=null)
				statusChanged(this,s);
		}
		private void _fireStatus(API.SANE_Status s)
		{
			_fireStatus(Status.wrap(s));
		}
		
	}
}

