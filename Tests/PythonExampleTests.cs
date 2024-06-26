﻿using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;

namespace GeneratorCalculation.Tests
{
	public class PythonExampleTests
	{
		[Fact]
		public void Run()
		{

			List<Generator> coroutines = new List<Generator>();
			coroutines.Add(new Generator("oc1", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("oc2", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("fr1", new CoroutineInstanceType((ConcreteType)"Y", new ListType((ConcreteType)"S", PaperStar.Instance))));
			coroutines.Add(new Generator("fr2", new CoroutineInstanceType((ConcreteType)"Y", new ListType((ConcreteType)"S", PaperStar.Instance))));
			coroutines.Add(new Generator("zip", new CoroutineInstanceType(new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"m"), new ListType((PaperVariable)"y", (PaperVariable)"n")), new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"m", (PaperVariable)"n")))));

			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Equal(ConcreteType.Void, result.Receive);
			Assert.Contains("S, S", result.Yield.ToString());
			Assert.Contains("min(a, b)", result.Yield.ToString());
		}
	}
}
