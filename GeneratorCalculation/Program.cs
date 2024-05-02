using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		private static List<Generator> GetSelfCleaningRules()
		{
			var terminatorTrue = new ListType((ConcreteType)"T", PaperStar.Instance);
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);
			var falseG = new CoroutineInstanceType(new Dictionary<SequenceType, List<SequenceType>>
			{
				[new SequenceType((PaperVariable)"x")] = new List<SequenceType> { new SequenceType((PaperVariable)"y") }
			},
				new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"y")), ConcreteType.Void, null);
			var trueG = new CoroutineInstanceType(terminatorTrue, new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"x")), null);
			var rec = new CoroutineInstanceType(
				new Dictionary<SequenceType, List<SequenceType>>
				{
					//Use a "D"ummy type because (PaperInt)0 is not a PaperType.
					[new SequenceType(new ListType((ConcreteType)"D", (PaperVariable)"n"))] = new List<SequenceType> { new SequenceType(new ListType((ConcreteType)"D", (PaperInt)0)) }
				},
				receive: new SequenceType(new ListType(falseG, PaperStar.Instance), new ListType(trueG, PaperStar.Instance), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))),
				yield: new SequenceType(falseG, trueG, new TupleType((PaperVariable)"x", (PaperVariable)"y"), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))), null);

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("base", new CoroutineInstanceType(new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))), terminatorFalse, null)));
			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));

			return coroutines;
		}

		private static List<Generator> GetRules()
		{
			var terminatorTrue = new ListType((ConcreteType)"T", PaperStar.Instance);
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);

			var falseG = new CoroutineInstanceType(
				receive: new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"y")),
				yield: ConcreteType.Void, null);
			var trueG = new CoroutineInstanceType(terminatorTrue, new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"x")), null);
			var rec = new CoroutineInstanceType(
				receive: new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))),
				yield: new SequenceType(trueG, falseG, new TupleType((PaperVariable)"x", (PaperVariable)"y"), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))), null);

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));
			coroutines.Add(new Generator("base", new CoroutineInstanceType(terminatorFalse, new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))), null)));

			return coroutines;
		}

		static void Main(string[] args)
		{


			List<Generator> coroutines = GetSelfCleaningRules();
			coroutines.Add(new Generator("starter", new CoroutineInstanceType(ConcreteType.Void, new SequenceType(new TupleType((ConcreteType)"String", new ListType((ConcreteType)"String", (PaperInt)3))), null)));

			var result = new Solver().SolveWithBindings(coroutines);
			
			Console.WriteLine(result);
		}

	}


	/// <summary>
	/// labeled generator
	/// </summary>
	public class Generator
	{
		public string Name { get; }
		public bool IsInfinite { get; }

		public CoroutineInstanceType OriginalType { get; }
		public CoroutineInstanceType Type { get; set; }

		public Generator(string name, CoroutineInstanceType type) : this(name, false, type)
		{ }


		public Generator(string name, bool isInfinite, CoroutineInstanceType type)
		{
			Name = name;
			IsInfinite = isInfinite;
			Type = type;
			OriginalType = type.Clone();
		}

	}
}
