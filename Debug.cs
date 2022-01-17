// 
//  Debug.cs
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
namespace Sane.Debugging
{
	public static class Debug
	{
		public enum DebugLevel:uint{
			DEBUG=0,
			MESSAGE,
			NOTICE,
			WARNING,
			ERROR
		}
#if DEBUG
		private static bool _enabled=true;
#else
		private static bool _enabled=false;
#endif
#if DBG_LEVEL_WARN
		private static DebugLevel _level=DebugLevel.WARNING;
#elif DBG_LEVEL_MESSAGE
		private static DebugLevel _level=DebugLevel.MESSAGE;
#elif DBG_LEVEL_NOTICE
		private static DebugLevel _level=DebugLevel.NOTICE;
#elif DBG_LEVEL_ERROR
		private static DebugLevel _level=DebugLevel.ERROR;
#else
		private static DebugLevel _level=DebugLevel.DEBUG;
#endif
		public static void enable(DebugLevel level)
		{
			_enabled=true;
			_level=level;
		}
		public static void Write(string message,params object[] args)
		{
			Write("Anonymous",message,args);
		}
		public static void Write(string tag,string message,params object[] args)
		{
			Write(DebugLevel.DEBUG,tag,message,args);
		}
		public static void Write(DebugLevel level, string tag, string message, params object[] args)
		{
			if(!_enabled) return;
			if(level < level)return;
			message = string.Format(message,args);
			Console.Write("{0}: [{1}] -- {2}",level.ToString(),tag,message);
		}
		public static void WriteLine(string message,params object[] args)
		{
			WriteLine("Anonymous",message,args);
		}
		public static void WriteLine(string tag,string message,params object[] args)
		{
			WriteLine(DebugLevel.DEBUG,tag,message,args);
		}
		public static void WriteLine(DebugLevel level, string tag, string message, params object[] args)
		{
			if(!_enabled)return;
			if(_level < level)return;
			message = string.Format(message,args);
			Console.WriteLine("{0}: [{1}] -- {2}",level.ToString(),tag,message);
		}
		public static void WriteDebug(string message, params object[] args)
		{
			WriteDebug("Anonymous",message,args);
		}
		public static void WriteDebug(string tag, string message, params object[] args)
		{
			WriteLine(DebugLevel.DEBUG,tag,message,args);
		}
		public static void Message(string message, params object[] args)
		{
			Message("Anonymous",message,args);
		}
		public static void Message(string tag, string message, params object[] args)
		{
			WriteLine(DebugLevel.MESSAGE,tag,message,args);
		}
		public static void Notice(string message, params object[] args)
		{
			Notice("Anonymous",message,args);
		}
		public static void Notice(string tag, string message, params object[] args)
		{
			WriteLine(DebugLevel.NOTICE,tag,message,args);
		}
		public static void Warn(string message, params object[] args)
		{
			Warn("Anonymous",message,args);
		}
		public static void Warn(string tag, string message, params object[] args)
		{
			WriteLine(DebugLevel.WARNING,tag,message,args);
		}
		public static void Error(string message, params object[] args)
		{
			Error("Anonymous",message,args);
		}
		public static void Error(string tag, string message, params object[] args)
		{
			WriteLine(DebugLevel.ERROR,tag,message,args);
		}
	}
}

