using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blamite.Blam.Util
{
	public class NewPointerConverter
	{
		public static uint ConvertToPointer(uint addr)
		{
			long address = (long) addr * 4;
			uint eighthBit = ((uint)(address >> 32)) << 28;
			uint newPointer = (uint)address + eighthBit;

			return newPointer;
		}
	}
}
