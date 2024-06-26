﻿using System;
using System.Collections.Generic;
using Xunit;
using GeneratorCalculation;

namespace GeneratorCalculationTests
{
	public class SolverTests
	{
		[Fact]
		public void ContinueYieldAfterReceive()
		{
			var coroutines = new List<Generator>
			{
				new Generator("",new CoroutineInstanceType(ConcreteType.Void, new SequenceType((ConcreteType)"A",(ConcreteType)"B"))),
				new Generator("",new CoroutineInstanceType((ConcreteType)"A", (ConcreteType)"C"))
			};

			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Equal(new SequenceType((ConcreteType)"C", (ConcreteType)"B"), result.Yield);
		}

		[Fact]
		public void ReceiveStartPosition()
		{
			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("a", new CoroutineInstanceType((ConcreteType)"S", (ConcreteType)"T")));
			coroutines.Add(new Generator("b", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"S")));
			coroutines.Add(new Generator("c", new CoroutineInstanceType((ConcreteType)"S", (ConcreteType)"U")));

			var result = new Solver().SolveWithBindings(coroutines);
		}

		[Fact]
		public void RunInfiniteLoop()
		{
			//Console.WriteLine("hi");
			var back = Console.Out;
			Console.SetOut(System.IO.TextWriter.Null);

			//Console.WriteLine("hello");
			try
			{

				List<Generator> list = new List<Generator>();

				list.Add(new Generator("a", true, new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"X")));
				list.Add(new Generator("b", true, new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Y")));
				Assert.Throws<StepLimitExceededException>(() => new Solver().SolveWithBindings(list, steps: 100));
			}
			finally
			{
				Console.SetOut(back);
			}
		}

		[Fact]
		public void SolveSingle()
		{
			var list = new List<Generator>();
			list.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Y")));
			var g = new Solver().SolveWithBindings(list);

			Assert.Equal(ConcreteType.Void, g.Receive);
			Assert.Contains("Y", g.Yield.ToString());
		}

		[Fact]
		public void SolveDeadlock()
		{
			var list = new List<Generator>();
			var g1 = new CoroutineInstanceType((ConcreteType)"B", (ConcreteType)"A");
			list.Add(new Generator("g1", g1));

			var g2 = new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"C");
			list.Add(new Generator("g2", g2));

			var g3 = new CoroutineInstanceType((ConcreteType)"E", (ConcreteType)"D");
			list.Add(new Generator("g3", g3));

			Assert.Throws<DeadLockException>(() => new Solver().SolveWithBindings(list));
		}

		[Fact]
		public void SingleRemainingNoLock()
		{
			var list = new List<Generator>();
			var g1 = new CoroutineInstanceType((ConcreteType)"B", (ConcreteType)"A");
			list.Add(new Generator("g1", g1));

			var g2 = new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"C");
			list.Add(new Generator("g2", g2));


			var result = new CoroutineInstanceType((ConcreteType)"B", new SequenceType((ConcreteType)"C", (ConcreteType)"A"));
			Assert.Equal(result, new Solver().SolveWithBindings(list));
		}

		[Fact]
		public void Interleave()
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("oc1", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("oc2", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("fr1", new CoroutineInstanceType((ConcreteType)"Y", new ListType((ConcreteType)"S", PaperStar.Instance))));
			coroutines.Add(new Generator("fr2", new CoroutineInstanceType((ConcreteType)"Y", new ListType((ConcreteType)"S", PaperStar.Instance))));


			CoroutineInstanceType interleave = new CoroutineInstanceType(new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")), new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")));
			coroutines.Add(new Generator("interleave", interleave));

			var result = new Solver().SolveWithBindings(coroutines);

			Console.WriteLine("Final result:");
			Console.WriteLine(result);
		}

		[Fact]
		public void UseVariable()
		{

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("a", new CoroutineInstanceType(ConcreteType.Void , (ConcreteType)"Y")));
			coroutines.Add(new Generator("b", new CoroutineInstanceType((PaperVariable)"a", (PaperVariable)"a")));


			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Contains("Y", result.Yield.ToString());
			Assert.Equal(ConcreteType.Void, result.Receive);
		}


		[Fact]
		public void PopReceive()
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("a", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"A")));
			coroutines.Add(new Generator("b", new CoroutineInstanceType((ConcreteType)"A", new SequenceType((ConcreteType)"B", (ConcreteType)"C"))));

			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Equal(new SequenceType((ConcreteType)"B", (ConcreteType)"C"), result.Yield);
			Assert.Equal(ConcreteType.Void, result.Receive);
		}

		[Fact]
		public void ReceiveCoroutine()
		{
			var coroutines = new List<Generator>();
			var g = new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"A");
			coroutines.Add(new Generator("a", g));
			coroutines.Add(new Generator("b", new CoroutineInstanceType(new SequenceType(new ListType(g.Clone(), (PaperInt)1)), (ConcreteType)"B")));

			var result = new Solver().SolveWithBindings(coroutines);

		}

		/// <summary>
		/// r1 and r2 are in a tuple and they are composed first. They dead lock.
		/// Nevertheless, l should activate r1, and return a simple coroutine.
		/// </summary>
		[Fact]
		public void ComposeTupleOfCoroutines()
		{
			var bindings = new Dictionary<PaperVariable, PaperWord>();
			bindings.Add("r1", new CoroutineInstanceType((ConcreteType)"A", (ConcreteType)"B"));
			bindings.Add("r2", new CoroutineInstanceType((ConcreteType)"B", (ConcreteType)"D"));

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType((PaperVariable)"r1", (PaperVariable)"r2"))));
			coroutines.Add(new Generator("l", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"A")));


			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Assert.Equal(ConcreteType.Void, result.Receive);
			Assert.Equal((ConcreteType)"D", result.Yield);

		}

	}
}
