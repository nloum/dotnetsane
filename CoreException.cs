// 
//  CoreException.cs
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
namespace SaneLow
{
	public class CoreException: Exception
	{
		API.SANE_Status _innerStatus;
		public API.SANE_Status status{
			get{
				return _innerStatus;
			}
		}
		public CoreException(Exception _innerException):base("No message given",_innerException)
		{
		}
		public CoreException(string message, Exception _innerException):base(message,_innerException)
		{
		}
		public CoreException(API.SANE_Status _status):base("No message given")
		{
			_innerStatus=_status;
		}
		public CoreException(string message,API.SANE_Status _status): base(message)
		{
			_innerStatus=_status;
		}
		public CoreException (string message): base(message)
		{
		}
	}
}

