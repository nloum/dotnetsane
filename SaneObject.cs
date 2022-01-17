// 
//  SaneObject.cs
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
	/// <summary>
	/// Allows extending objects to signal status changes
	/// </summary>
	public class SaneObject
	{
		public delegate void statusChangeDelegate(object sender,Status s);
		public event statusChangeDelegate statusChanged;
		protected Status pStatus;
		public SaneObject ()
		{
		}
		protected void setStatus(Status s)
		{
			_fireStatusChanged(s);
		}
		/*public void setStatus(API.SANE_Status s)
		{
			setStatus(Status.wrap(s));
		}*/
		private void _fireStatusChanged(Status s)
		{
			if(s.Equals(pStatus))return;
			pStatus=s;
			if(statusChanged!=null)
				statusChanged(this,s);
		}
	}
}

