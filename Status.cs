// 
//  Status.cs
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
	/// Wrapper for the SANE_Status enum
	/// </summary>
	public class Status
	{
		private static string TAG="Sane.Status";
		private int _innerCode;
		private string _innerDescription;
		private static Dictionary<API.SANE_Status,Status> _cache;
		public int code
		{
			get{
				return _innerCode;
			}
		}
		public string description{
			get{
				return _innerDescription;
			}
		}
		public bool isGood()
		{
			return _innerCode==(int)API.SANE_Status.SANE_STATUS_GOOD;
		}
		public bool isEOF()
		{
			return _innerCode==(int)API.SANE_Status.SANE_STATUS_DEVICE_EOF;
		}
		public Status (API.SANE_Status s)
		{
			_innerCode=(int)s;
			_innerDescription=Helpers.getStatusStr(s);
		}
		public Status()
		{
			API.SANE_Status s = API.SANE_Status.SANE_STATUS_GOOD;
			_innerCode=(int)s;
			_innerDescription = Helpers.getStatusStr(s);
		}
		public static Status wrap(API.SANE_Status s)
		{
			if(_cache == null)
			{
				_cache = new Dictionary<API.SANE_Status, Status>();
			}
			if(_cache.Keys.Count > 5)
			{
				Debugging.Debug.WriteLine(TAG,"Clear Status Cache");
				_cache.Clear();
			}
			if(!_cache.ContainsKey(s))
			{
				Debugging.Debug.WriteLine(TAG,"Status Cache -- Grow");
				_cache.Add(s,new Status(s));
			}
			return _cache[s];
		}
	}
}

