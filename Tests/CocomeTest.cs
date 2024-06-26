﻿using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;

namespace GeneratorCalculation.Tests
{
	public class CocomeTests
	{
		[Fact]
		public void RunMakeNewSaleUseCase()
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("openStore", new CoroutineInstanceType(new SequenceType("Store"), new SequenceType("Store", "CurrentStore"))));
			coroutines.Add(new Generator("openCashDesk", new CoroutineInstanceType(new SequenceType("CashDesk", "CurrentStore"), new SequenceType("CashDesk", "CurrentStore", "CurrentCashDesk"))));
			coroutines.Add(new Generator("makeNewSale", new CoroutineInstanceType(new SequenceType("CurrentCashDesk"), new SequenceType("CurrentCashDesk", "Sale", "CurrentSale"))));
			coroutines.Add(new Generator("enterItem", new CoroutineInstanceType(new SequenceType("CurrentSale", "Item"), new SequenceType("CurrentSale", "Item", "SalesLineItem", "CurrentSaleLine"))));
			coroutines.Add(new Generator("createStore", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("Store"))));
			coroutines.Add(new Generator("createCashDesk", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("CashDesk"))));
			coroutines.Add(new Generator("createItem", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("Item"))));


			var result = new Solver().SolveWithBindings(coroutines);
			Assert.Equal(ConcreteType.Void, result.Receive);
		}

		[Fact]
		public void DeleteItemAtTheEnd()
		{
			var bindings = new Dictionary<PaperVariable, PaperWord>();
			bindings.Add("openStore", new CoroutineInstanceType(new SequenceType("Store"), new SequenceType("Store", "CurrentStore")));
			bindings.Add("openCashDesk", new CoroutineInstanceType(new SequenceType("CashDesk", "CurrentStore"), new SequenceType("CashDesk", "CurrentStore", "CurrentCashDesk")));
			bindings.Add("makeNewSale", new CoroutineInstanceType(new SequenceType("CurrentCashDesk"), new SequenceType("CurrentCashDesk", "Sale", "CurrentSale")));
			bindings.Add("enterItem", new CoroutineInstanceType(new SequenceType("CurrentSale", "Item"), new SequenceType("CurrentSale", "Item", "SalesLineItem", "CurrentSaleLine")));
			bindings.Add("createStore", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("Store")));
			bindings.Add("createCashDesk", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("CashDesk")));
			bindings.Add("createItem", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("Item")));

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType((PaperVariable)"openStore", (PaperVariable)"openCashDesk", (PaperVariable)"makeNewSale", (PaperVariable)"enterItem", (PaperVariable)"createStore", (PaperVariable)"createCashDesk", (PaperVariable)"createItem"))));
			coroutines.Add(new Generator("deleteItem", new CoroutineInstanceType(new SequenceType("Item"), ConcreteType.Void)));


			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Assert.Equal(ConcreteType.Void, result.Receive);
			Assert.DoesNotContain((ConcreteType)"Item", ((SequenceType)result.Yield).Types);
		}

	}
}
