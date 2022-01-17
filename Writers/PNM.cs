//
//  PNM.cs
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
using System.IO;
using Sane;
namespace Sane.Writers
{
	public class PNM : IWriter
	{
		private Stream _stream;
		private int _length;
		public PNM (Stream stream)
		{
			_stream = stream;
		}
		public void writeHeader(Parameters.Frame format, int width, int height, int depth)
		{
			switch(format)
			{
			case Parameters.Frame.RED:
			case Parameters.Frame.GREEN:
			case Parameters.Frame.BLUE:
			case Parameters.Frame.RGB:
				write("P6\n# SANE data follows\n{0} {1}\n{2}\n",width,height,
				      (depth <= 8) ? 255:65535);
				break;
			default:
				if(depth == 1)
				{
					write("P4\n# SANE data follows\n{0} {1}\n",width,height);
				}else{
					write("P5\n# SANE data follows\n{0} {1}\n{2}\n",width,height,
					      (depth <= 8) ? 255:65535);
				}
			break;	                                                                  
			}	
		}
		public byte[] getBytes()
		{
			byte[] bytes=new byte[_length];
			long oldpos=_stream.Position;
			_stream.Seek(0L,SeekOrigin.Begin);
			_stream.Read(bytes,0,_length);
			_stream.Seek(oldpos,SeekOrigin.Begin);
			return bytes;
		}
		public void close()
		{
			_stream.Close();
			_stream.Dispose();
		}
		private void write(string data,params object[] parameters)
		{
			string formatted=string.Format(data,parameters);
			//Console.Out.WriteLine("WRITE HEADER: {0}",formatted);
			byte[] bytes=System.Text.ASCIIEncoding.ASCII.GetBytes(formatted);
			//_stream.Write(bytes,0,bytes.Length);
			write(bytes,0,bytes.Length);
		}
		public void write(byte[] data,int offset, int count)
		{
			_stream.Write(data,offset,count);
			_length+=count;
		}
	}
}

