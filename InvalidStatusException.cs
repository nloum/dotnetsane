// 
//  InvalidStatusException.cs
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
namespace Sane
{
	public class InvalidStatusException : Exception
	{
		private Status _innerStatus;
		public Status status{
			get{
				return _innerStatus;
			}
		}
		public InvalidStatusException(Sane.Status innerStatus): base(innerStatus.description)
		{
			_innerStatus=innerStatus;
		}
		public InvalidStatusException(API.SANE_Status innerStatus): base(Sane.Status.wrap(innerStatus).description)
		{
			_innerStatus=Sane.Status.wrap(innerStatus);
		}
		public InvalidStatusException (string message,API.SANE_Status innerStatus): base(message)
		{
			this._innerStatus=Sane.Status.wrap(innerStatus);
		}
		public InvalidStatusException(string message,Sane.Status innerStatus):base(message)
		{
			_innerStatus = innerStatus;
		}
	}
}

