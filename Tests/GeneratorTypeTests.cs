using Xunit;
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



		[Fact]
		public void TestRemoveCondition()
		{
			// to test on the condition: a should be a user, if not, the condition cannot receieve, and reply null
			// in this test, the pending is 'Book', not a user, so the result should return null.
			// if the test failed, means that, the unit test did not check the state of 'condition' .

			InheritanceCondition co = new InheritanceCondition();
			co.Subclass = (PaperVariable)"t";
			co.Superclass = (ConcreteType)"Y";


			var g = new CoroutineInstanceType(condition: co, (PaperVariable)"b", (ConcreteType)"X");


			Assert.Null(g.Condition);

			//ConcreteType pending = new ConcreteType("Z");
			//CoroutineInstanceType newGenerator;
			//// aim: cannot receive
			//Dictionary<PaperVariable, PaperWord> conditions = g.RunReceive(pending, out newGenerator);
			//
			//Assert.Null(conditions);
		}



		[Fact]
		public void TestRemoveCondition2()
		{
			// to test on the condition: a should be a user, if not, the condition cannot receieve, and reply null
			// in this test, the pending is 'Book', not a user, so the result should return null.
			// if the test failed, means that, the unit test did not check the state of 'condition' .

			InheritanceCondition co = new InheritanceCondition();
			co.Subclass = (PaperVariable)"Socket";
			co.Superclass = (ConcreteType)"T999";

			var g = new CoroutineInstanceType(condition: co, new SequenceType((ConcreteType)"T758", (PaperVariable)"Socket"), (ConcreteType)"Haha");

			Assert.NotNull(g.Condition);
		}


		[Fact]
		public void TestRemoveConditionVariableConcreteSameName()
		{
			// to test on the condition: a should be a user, if not, the condition cannot receieve, and reply null
			// in this test, the pending is 'Book', not a user, so the result should return null.
			// if the test failed, means that, the unit test did not check the state of 'condition' .

			InheritanceCondition co = new InheritanceCondition();
			co.Subclass = (PaperVariable)"Socket";
			co.Superclass = (ConcreteType)"T999";

			var g = new CoroutineInstanceType(condition: co, new SequenceType((ConcreteType)"T758", (ConcreteType)"Socket"), (ConcreteType)"Haha");

			Assert.Null(g.Condition);
		}


		[Fact]
		public void TestNotYieldVariables()
		{
			// to test on the condition: a should be a user, if not, the condition cannot receieve, and reply null
			// in this test, the pending is 'Book', not a user, so the result should return null.
			// if the test failed, means that, the unit test did not check the state of 'condition' .

			var g = new CoroutineInstanceType(new SequenceType((ConcreteType)"T758", (ConcreteType)"Socket"), (PaperVariable)"Haha");

			ConcreteType pending = new ConcreteType("T758");

			CoroutineInstanceType newGenerator;
			Dictionary<PaperVariable, PaperWord> conditions = g.RunReceive(pending, out newGenerator);
			Assert.NotNull(conditions);

			conditions = newGenerator.RunReceive(new ConcreteType("Socket"), out newGenerator);
			Assert.NotNull(conditions);

			PaperType yieldedType = null;
			newGenerator=newGenerator.RunYield(new Dictionary<PaperVariable, PaperWord>(), ref yieldedType);
			Assert.Null(newGenerator);
		}

	}

}