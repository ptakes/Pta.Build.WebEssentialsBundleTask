using System;
using System.Collections.Generic;

namespace Pta.Build.WebEssentialsBundleTask
{
	public class BundleMap : Dictionary<string, Bundle>
	{
		public BundleMap(int capacity)
			: base(capacity, StringComparer.InvariantCultureIgnoreCase)
		{
		}
	}
}
