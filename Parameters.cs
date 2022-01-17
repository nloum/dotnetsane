// 
//  Parameters.cs
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
	/// Wraps the SANE_Parameters structure
	/// </summary>
	public class Parameters
	{
		public enum Frame{
			GRAY=0,
			RGB=1,
			RED=2,
			GREEN=3,
			BLUE=4
		}
		private API.SANE_Parameters _innerParameters;
		
		public int depth{
			get{
				return _innerParameters.depth;
			}
		}
		public Frame format{
			get{
				return (Frame)_innerParameters.format;
			}
		}
		public bool lastFrame{
			get{
				return (_innerParameters.last_frame==1);
			}
		}
		public int lines{
			get{
				return _innerParameters.lines;
			}
		}
		public int pixelsPerLine{
			get{
				return _innerParameters.pixels_per_line;
			}
		}
		public int bytesPerLine{
			get{
				return _innerParameters.bytes_per_line;
			}
		}
		public Parameters ()
		{
		}
		public static Parameters Wrap(API.SANE_Parameters p)
		{
			Parameters my_p = new Parameters();
			my_p._innerParameters=p;
			return my_p;
		}
	}
}

