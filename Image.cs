// 
//  Image.cs
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
using Gdk;
using System.IO;
using SaneLow;
using System.Diagnostics;
namespace Sane
{
	/// <summary>
	/// Basically the scanning portion.
	/// </summary>
	/// <remarks>
	/// I Haven't quite worked out how to fit this into the overall api yet
	/// </remarks>
	public class Image : SaneObject, IDisposable
	{
		private const string TAG="Sane.Image";
		private IWriter writer;
		private int _width=0;
		private int _height=0;
		private Device _owner;
		private Pixbuf _pixbuf;
		private Sane.Status _lastStatus{
			get{
				return _lastStatus;
			}set{
				setStatus(value);
			}
		}
		public Sane.Status lastStatus{
			get{
				return pStatus;
			}
		}
		public Device device{
			get{
				return _owner;
			}
		}
		public delegate void progressTickDelegate(Image i, double progress);
		public static event progressTickDelegate onProgressTick;
		public delegate void readyDelegate(Image sender);
		public static event readyDelegate onReady;
		public Image (Device owner,IWriter writer)
		{
			this.writer = writer;
			_owner = owner;
			setStatus(new Status());
		}
		public void destroy()
		{
			writer.close();
			if(_pixbuf != null)_pixbuf.Dispose();
		}
		public static Image acquire(Device d)
		{
			Writers.PNM writer=new Writers.PNM(new System.IO.MemoryStream());
			return acquire(d,writer);
		}
		public static Image acquire(Device d, IWriter writer)
		{
			Image img = new Image(d,writer);
			img._owner=d;
			//img._lastStatus=img.read(d);
			img.setStatus(img.read(d));
			/*try{
				img._pixbuf = new Pixbuf(writer.getBytes());
			}catch(NotSupportedException e)
			{
				Console.WriteLine("Reading from open stream not supported by writter backend");
			}*/
			//writer.close();
			return img;
		}
		public void Dispose ()
		{
			Debugging.Debug.WriteLine(TAG,"Image -- Disposing");
			writer.close();
			
			//throw new NotImplementedException ();
		}
		public Pixbuf getPixbuf()
		{
			if(_pixbuf==null)
			{
				Debugging.Debug.WriteLine(TAG,"Generating new pixbuf!!!");
				return new Pixbuf(writer.getBytes());
			}
			return _pixbuf;
		}
		public Pixbuf getPixbuf(int width, int height)
		{
			return getPixbuf(width,height,true);
		}
		public Pixbuf getPixbuf(int width, int height, bool proportional)
		{
			return _pixbuf.ScaleSimple(width,height,InterpType.Bilinear);
		}
		/// <summary>
		/// Read an image from the given device
		/// </summary>
		/// <remarks>
		/// Big portions of this are not implemented yet (buffering for example).  As this is a rip of the scanning portion of scanimage,
		/// filling in the blanks shouldn't be too hard.  Works fine if buffering isn't needed though.
		/// </remarks>
		/// <param name="device">
		/// A <see cref="Device"/>
		/// </param>
		/// <returns>
		/// A <see cref="Status"/>
		/// </returns>
		private Sane.Status read(Device device)
		{
			setStatus(new Status(API.SANE_Status.SANE_STATUS_GOOD));
			int len, offset =0, hundred_percent;
			bool must_buffer=false,first_frame=true;
			int buffer_size=(1024*1024);
			Parameters parm;
			byte[] buffer = new byte[buffer_size];
			int total_bytes=0;
			setStatus(device.start());
			parm=device.getParameters();
			
			if(first_frame)
			{
				switch(parm.format)
				{
					case Parameters.Frame.RED:
					case Parameters.Frame.GREEN:
					case Parameters.Frame.BLUE:
						must_buffer=true;
						offset=parm.format - Parameters.Frame.RED;
						break;
					case Parameters.Frame.RGB:
					case Parameters.Frame.GRAY:
						Debug.Assert((parm.depth == 1) || (parm.depth == 8) || (parm.depth == 16));
						if(parm.lines < 0)
						{
							must_buffer = true;
							offset=0;
						}else{
							_width=parm.pixelsPerLine;
							_height=parm.lines;
							writer.writeHeader(parm.format,parm.pixelsPerLine,parm.lines,parm.depth);
						}
					break;
					default:
						break;
				}
				if(must_buffer)
				{
					throw new NotImplementedException("Buffering is not yet implemented");
				}else{
					Debug.Assert(parm.format >= Parameters.Frame.RED
					             && parm.format <= Parameters.Frame.BLUE);
					offset=parm.format - Parameters.Frame.RED;
					//image.x = image.y = 0;
					if(offset<0)offset=0;
				}
				hundred_percent = parm.bytesPerLine * parm.lines
					* ((parm.format == Parameters.Frame.RGB || parm.format == Parameters.Frame.GRAY) ? 1:3);
				
				while(true)
				{
					double progress;
					setStatus(device.read(buffer_size,ref buffer,out len));
					total_bytes+=len;
					progress = (double)((total_bytes*100) / (double)hundred_percent);
					if(progress > 100)
					{
						progress=100;
					}
					_fireProgres(progress);
					//Console.WriteLine("PROGRESS: {0}%",progress);
					//Progress_tick
					if(!pStatus.isGood())
					{
						if(pStatus.isEOF())
						{
							_fireReady();
						}
						return pStatus;
					}
					if(must_buffer)
					{
						throw new NotImplementedException("Buffering is not yet implemented");
					}
					if(parm.depth != 16)
					{
						//Write to stream
						writer.write(buffer,offset,len);
					}else{
						throw new NotImplementedException("16 bit images are not supported yet");
					}
				}
				first_frame=false;
			}while(!parm.lastFrame);
			if(must_buffer)
			{
				throw new NotImplementedException("Buffering is not supported yet");
			}
			return pStatus;
		}
		private void _fireReady()
		{
			if(onReady!= null)
			{
				onReady(this);
			}
		}
		private void _fireProgres(double progress)
		{
			if(onProgressTick != null)
				onProgressTick(this,progress);
		}
	}
}

