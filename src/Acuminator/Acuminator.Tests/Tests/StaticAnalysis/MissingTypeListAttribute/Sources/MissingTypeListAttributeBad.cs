﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class BadDac : IBqlTable
	{
		public abstract class someField { }
		[PXStringList(new[] { "O", "N" }, new[] { "Open", "New" })]
		public string SomeField { get; set; }
	}
}