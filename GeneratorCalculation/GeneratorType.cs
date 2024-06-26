﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneratorCalculation
{

	/// <summary>
	/// This is an instance of a coroutine definition.
	/// A definition turns into an instance by starting the definition.
	/// </summary>
	public class CoroutineInstanceType : PaperType
	{
		public Condition Condition { get; }

		public PaperType Yield { get; }

		public PaperType Receive { get; }

		public Dictionary<SequenceType, List<SequenceType>> ForbiddenBindings { get; } = new Dictionary<SequenceType, List<SequenceType>>();

		public PaperVariable Source { get; }

		public bool CanRestore { get; }

		public void Check()
		{
			//Receive is like input, yield is like output.
			//Yield cannot have variables that are unbound from Receive.

			List<string> constants = new List<string>();
			var inputVariables = Receive.GetVariables().Select(v => v.Name).ToList();
			var outputVariables = Yield.GetVariables().Select(v => v.Name).ToList();

			if (outputVariables.Any(v => inputVariables.Contains(v) == false))
			{
				var culprits = outputVariables.Where(v => inputVariables.Contains(v) == false).ToList();
				throw new FormatException($"{string.Join(", ", culprits)} are not bound by receive.");
			}


			//Console.WriteLine("Yield variables: " + string.Join(", ", ));

			//Console.WriteLine("Receive variables: " + string.Join(", ", ));
		}

		/// <summary>
		/// If it can yield, return the new type. Otherwise return null.
		/// </summary>
		/// <param name="constants"></param>
		/// <param name="g"></param>
		/// <param name="yieldedType"></param>
		/// <returns></returns>
		public virtual CoroutineInstanceType RunYield(Dictionary<PaperVariable, PaperWord> bindings, ref PaperType yieldedType)
		{
			if (Receive != ConcreteType.Void)
				return null;


			if (Yield.GetVariables().Except(bindings.Keys.ToList()).Any() == false)
			{
				PaperType remaining = null;
				if (Yield.Pop(ref yieldedType, ref remaining))
				{
					yieldedType = (PaperType)yieldedType.ApplyEquation(bindings.ToList());
					//Forbidden bindings are not needed when the coroutine starts to yield
					//because all variables have been bound.
					return new CoroutineInstanceType(Receive, remaining, this.Source);
				}
			}
			else
				Console.WriteLine("Unable to yield due to unbound variables");


			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="receive"></param>
		/// <param name="yield"></param>
		/// <param name="source">This parameter is for information purpose. Only when canRestore is true, the solver then looks up the source in the bindings.</param>
		/// <param name="canRestore"></param>
		public CoroutineInstanceType(PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false)
		{
			Receive = receive;
			Yield = yield;
			Source = source;
			CanRestore = canRestore;
		}

		public CoroutineInstanceType(Dictionary<SequenceType, List<SequenceType>> forbiddenBindings, PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false)
		{
			Receive = receive;
			Yield = yield;
			ForbiddenBindings = forbiddenBindings;
			Source = source;
			CanRestore = canRestore;
		}

		public CoroutineInstanceType(Condition condition, PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false)
		{
			Receive = receive;
			Yield = yield;
			Source = source;
			CanRestore = canRestore;

			if (condition is InheritanceCondition c1 && !receive.GetVariables().Contains(c1.Subclass))
				Condition = null;
			else
				Condition = condition;


		}

		public CoroutineInstanceType(Condition condition, Dictionary<SequenceType, List<SequenceType>> forbiddenBindings, PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false)
		{
			Receive = receive;
			Yield = yield;
			ForbiddenBindings = forbiddenBindings;
			Source = source;
			CanRestore = canRestore;

			if (condition is InheritanceCondition c1 && !receive.GetVariables().Contains(c1.Subclass))
				Condition = null;
			else
				Condition = condition;

		}
		/// <summary>
		/// Check whether this generator can receive the given type.
		/// 
		/// If it can receive, return 'condition'. If it cannot receive, return null.
		/// </summary>
		/// <param name="providedType"></param>
		/// <param name="newGenerator"></param>
		/// <returns></returns>
		public virtual Dictionary<PaperVariable, PaperWord> RunReceive(PaperType providedType, out CoroutineInstanceType newGenerator)
		{
			newGenerator = null;
			Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();
			if (Receive == ConcreteType.Void)
				return null;

			PaperType head = null;
			PaperType remaining = null;
			if (Receive.Pop(ref head, ref remaining))
			{
				var c = head.IsCompatibleTo(providedType);
				if (c == null)
					return null;

				conditions = Solver.JoinConditions(conditions, c);
				if (HasForbiddenBindings(conditions))
					return null;

				newGenerator = new CoroutineInstanceType(ForbiddenBindings, remaining, Yield, Source);
				return conditions;
			}

			Debug.Assert(Receive == ConcreteType.Void);
			return null;
		}

		protected bool HasForbiddenBindings(Dictionary<PaperVariable, PaperWord> valueMappings)
		{
			foreach (SequenceType key in ForbiddenBindings.Keys)
			{
				var valuedKey = key.ApplyEquation(valueMappings);
				var forbiddenSet = ForbiddenBindings[key];

				if (forbiddenSet.Select(s => s.ApplyEquation(valueMappings)).Any(s => s.Equals(valuedKey)))
					return true;
			}

			return false;
		}


		public override string ToString()
		{
			if (Condition != null)
				return $"[{Receive}; {Yield}] where {Condition}";
			else if (ForbiddenBindings.Count == 0)
				return $"[{Receive}; {Yield}]";
			else
			{
				string constrain = string.Join(", ", ForbiddenBindings.Select(p => p.Key + " not in {" + string.Join(", ", p.Value) + "}"));
				return $"[{Receive}; {Yield}] where {constrain}";
			}
		}



		public List<PaperVariable> GetVariables()
		{
			var inputVariables = Receive.GetVariables().ToList();
			var outputVariables = Yield.GetVariables().ToList();
			return inputVariables.Concat(outputVariables).ToList();
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is CoroutineInstanceType another)
				// TODO: should use full match. IsCompatibleTo only checks the head element.
				return Solver.JoinConditions(Yield.IsCompatibleTo(another.Yield), Receive.IsCompatibleTo(another.Receive));

			return null;

		}


		PaperWord PaperWord.ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return ApplyEquation(equations);
		}

		/// <summary>
		/// Never returns null
		/// </summary>
		/// <param name="equations"></param>
		/// <returns></returns>
		public CoroutineInstanceType ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			var newYield = Yield.ApplyEquation(equations);
			var newReceive = Receive.ApplyEquation(equations);
			if (newYield is PaperType newYieldType && newReceive is PaperType newReceiveType)
			{
				var copy = new Dictionary<SequenceType, List<SequenceType>>();
				foreach (SequenceType key in ForbiddenBindings.Keys)
				{
					SequenceType valuedKey = (SequenceType)key.ApplyEquation(equations);
					var valuedSet = ForbiddenBindings[key].Select(s => (SequenceType)s.ApplyEquation(equations)).ToList();
					if (valuedSet.Any(s => s.Equals(valuedKey)))
						return new CoroutineInstanceType(ConcreteType.Void, ConcreteType.Void, this.Source); // This identity element will be nuked.

					var c = new List<string>();
					if (valuedKey.GetVariables().Count == 0 && valuedSet.Sum(s => s.GetVariables().Count) == 0)
						continue; //Since both sides have no variables, we don't have to add them to ForbiddenBindings.

					if (copy.ContainsKey(valuedKey))
						copy[valuedKey].AddRange(valuedSet);
					else
						copy[valuedKey] = valuedSet;
				}

				return new CoroutineInstanceType(copy, newReceiveType, newYieldType, this.Source);
			}
			else
				return this;
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			Yield.ReplaceWithConstant(availableConstants, usedConstants);
			Receive.ReplaceWithConstant(availableConstants, usedConstants);
		}

		public virtual PaperType Normalize()
		{
			CoroutineInstanceType g;
			if (Condition != null)
				g = new CoroutineInstanceType(Condition, Receive.Normalize(), Yield.Normalize(), Source);
			else if (ForbiddenBindings != null)
				g = new CoroutineInstanceType(ForbiddenBindings, Receive.Normalize(), Yield.Normalize(), Source);
			else
				g = new CoroutineInstanceType(Receive.Normalize(), Yield.Normalize(), Source);

			if (g.Yield == ConcreteType.Void && g.Receive == ConcreteType.Void)
				return ConcreteType.Void;
			else
				return g;
		}


		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is CoroutineInstanceType objGenerator)
			{
				return Receive.Equals(objGenerator.Receive) && Yield.Equals(objGenerator.Yield);
			}

			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Receive.GetHashCode() ^ Yield.GetHashCode();
		}

		public virtual CoroutineInstanceType Clone()
		{
			return new CoroutineInstanceType(ForbiddenBindings, Receive, Yield, Source, CanRestore);
		}
	}


	//public class LabeledCoroutineType : GeneratorType
	//{
	//	public string Name { get; }
	//	public bool IsInfinite { get; }

	//	public GeneratorType OriginalType { get; }

	//	public LabeledCoroutineType(PaperType receive, PaperType yield, string name = null, bool isInfinite = false) : base(yield, receive)
	//	{
	//		Name = name;
	//		IsInfinite = isInfinite;

	//		if (IsInfinite)
	//			OriginalType = new GeneratorType(receive, yield);

	//	}

	//	public LabeledCoroutineType(Dictionary<SequenceType, List<SequenceType>> forbiddenBindings, PaperType receive, PaperType yield, string name = null, bool isInfinite = false) : base(forbiddenBindings, receive, yield)
	//	{
	//		Name = name;
	//		IsInfinite = isInfinite;

	//		if (IsInfinite)
	//			OriginalType = new GeneratorType(receive, yield);
	//	}

	//	/// <summary>
	//	/// If it can yield, return the new type. Otherwise return null.
	//	/// </summary>
	//	/// <param name="constants"></param>
	//	/// <param name="g"></param>
	//	/// <param name="yieldedType"></param>
	//	/// <returns></returns>
	//	public override LabeledCoroutineType RunYield(List<string> constants, ref PaperType yieldedType)
	//	{
	//		if (Receive != ConcreteType.Void)
	//			return null;


	//		if (Yield.GetVariables(constants).Count == 0)
	//		{
	//			PaperType remaining = null;
	//			if (Yield.Pop(ref yieldedType, ref remaining))
	//				return new GeneratorType(remaining, Receive);

	//		}

	//		return null;
	//	}

	//	public override bool Equals(object obj)
	//	{
	//		if (obj is GeneratorType objGenerator)
	//		{
	//			return Receive.Equals(objGenerator.Receive) && Yield.Equals(objGenerator.Yield);
	//		}

	//		return false;
	//	}

	//	public override int GetHashCode()
	//	{
	//		return Name.GetHashCode();
	//	}

	//	public LabeledCoroutineType Clone()
	//	{
	//		return new LabeledCoroutineType(ForbiddenBindings, Receive, Yield, Name, IsInfinite);
	//	}
	//}


	public class CoroutineDefinitionType
	{
		public Condition Condition { get; }

		public PaperType Yield { get; }

		public PaperType Receive { get; }

		public Dictionary<SequenceType, List<SequenceType>> ForbiddenBindings { get; } = new Dictionary<SequenceType, List<SequenceType>>();



		public CoroutineDefinitionType(PaperType receive, PaperType @yield, Condition condition = null)
		{
			Receive = receive;
			Yield = yield;
			Condition = condition;
		}

		public CoroutineInstanceType Start()
		{
			return new CoroutineInstanceType(Receive, Yield, null);
		}

		public override string ToString()
		{
			return $"~>({Receive}; {Yield})";
		}

		// override object.Equals
		public override bool Equals(object obj)
		{
			CoroutineDefinitionType y = obj as CoroutineDefinitionType;
			if (y == null)
				return false;

			return Yield.Equals(y.Yield) && Receive.Equals(y.Receive);
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Yield.GetHashCode() << 2 + Receive.GetHashCode();
		}
	}
}
