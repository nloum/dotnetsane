// 
//  Range.cs
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
	/// Wraps the SANE_Range structure
	/// </summary>
	public class Range
	{
		private API.SANE_Range _innerRange;
		public int min{
			get{
				return _innerRange.min;
			}
		}
		public int quant{
			get{
				return _innerRange.max;
			}
		}
		public int max{
			get{
				return _innerRange.max;
			}
		}
		public int step{
			get{
				return quant;
			}
		}
		private Range (API.SANE_Range range)
		{
			_innerRange=range;
		}
		public static Range wrap(API.SANE_Range range)
		{
			return new Range(range);
		}
		public bool Validate(int val)
		{
			return (val >= min && val <= max);
		}
		public bool Validate(double val)
		{
			double _max = Helpers.unfix(_innerRange.max);
			double _min = Helpers.unfix(_innerRange.min);
			//TODO: Make sure step is right
			return (val >= min && val <= max);
		}
		public double getFixedMin()
		{
			return Helpers.unfix(_innerRange.min);
		}
		public double getFixedMax()
		{
			return Helpers.unfix(_innerRange.max);
		}
		public double getFixedStep()
		{
			return Helpers.unfix(_innerRange.quant);
		}
	}
}

