// 
//  IWriter.cs
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
namespace Sane
{
	/// <summary>
	/// Interface for various image writers
	/// </summary>
	/// <remarks>
	/// The api here probably won't be sufficient for much of anything besides PNM images
	/// </remarks>
	public interface IWriter
	{
		void writeHeader(Parameters.Frame format, int width, int height, int depth);
		void write(byte[] data,int offset, int count);
		byte[] getBytes();
		void close();
	}
}

