﻿using Xunit;
using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation.Tests
{
	public class GeneratorTypeTests
	{
		[Fact()]
		public void CheckTest()
		{
			CoroutineInstanceType g = new CoroutineInstanceType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"z"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));


			Assert.Throws<FormatException>(() => g.Check());
		}

		[Fact]
		public void TestForbiddenBindings()
		{
			var forbiddenBindings = new Dictionary<SequenceType, List<SequenceType>>();
			forbiddenBindings[new SequenceType((PaperVariable)"b")] = new List<SequenceType> { new SequenceType((ConcreteType)"B") };
			CoroutineInstanceType g = new CoroutineInstanceType(forbiddenBindings, new SequenceType((PaperVariable)"a", (PaperVariable)"b"), (ConcreteType)"X");

			CoroutineInstanceType ng;
			var conditions = g.RunReceive((ConcreteType)"A", out ng);
			Assert.True(conditions != null, "The coroutine should have no problem in receiving A.");

			Assert.Equal(forbiddenBindings, ng.ForbiddenBindings);
		}

		[Fact]
		public void TestConstructor()
		{
			var g = new CoroutineInstanceType(condition: new InheritanceCondition(),
				receive: (ConcreteType)"A",
				yield: (ConcreteType)"B");

			Assert.Equal((ConcreteType)"A", g.Receive);
			Assert.Equal((ConcreteType)"B", g.Yield);
		}
	}

}