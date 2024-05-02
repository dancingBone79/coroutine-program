using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GeneratorCalculation;
using Xunit;

namespace RequirementAnalysis.Tests
{
	public class CocomeTest
	{
		[Fact]
		public static void MakeNewSale()
		{
			string content;
			var assembly = typeof(CocomeTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("cocome.remodel"));

			using (var stream = typeof(CocomeTest).GetTypeInfo().Assembly.GetManifestResourceStream(file))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					content = reader.ReadToEnd();
				}
			}

			var inheritance = REModelStart.GetObjectInheritance(content);
			var generators = REModelStart.GetAllGenerators(content, inheritance);

			string[] interestedCoroutines =
			{
				"CoCoMESystem::openStore",
				"CoCoMESystem::openCashDesk",
				"ProcessSaleService::makeNewSale",
				"ProcessSaleService::enterItem",
				"ManageStoreCRUDService::createStore",
				"ManageCashDeskCRUDService::createCashDesk",
				"ManageItemCRUDService::createItem",

				"ProcessSaleService::makeCashPayment",
				//"ProcessSaleService::makeCardPayment",

			};
			string[] lowPriorityCoroutines =
			{
				"ManageItemCRUDService::deleteItem",
				"ManageStoreCRUDService::deleteStore",
				"ManageCashDeskCRUDService::deleteCashDesk",
			};



			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType(from b in bindings select b.Key))));
			coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines, g.Name) != -1));



			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);
		}


		[Fact]
		public static void PlaceOrder()
		{
			string content;
			var assembly = typeof(CocomeTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("cocome.remodel"));

			using (var stream = typeof(CocomeTest).GetTypeInfo().Assembly.GetManifestResourceStream(file))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					content = reader.ReadToEnd();
				}
			}

			var inheritance = REModelStart.GetObjectInheritance(content);
			var generators = REModelStart.GetAllGenerators(content, inheritance);

			string[] interestedCoroutines =
			{
				"ManageItemCRUDService::createItem",
				"CoCoMEOrderProducts::makeNewOrder",
				"CoCoMEOrderProducts::orderItem",
				"CoCoMEOrderProducts::placeOrder",
				"CoCoMESystem::receiveOrderedProduct",
			};
			string[] lowPriorityCoroutines =
			{
				"ManageItemCRUDService::deleteItem",
			};


			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType(from b in bindings select b.Key))));
			coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines, g.Name) != -1));



			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);
		}

		[Fact]
		public static void TestCompositionOrder()
		{
			string content;
			var assembly = typeof(CocomeTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("cocome.remodel"));

			using (var stream = typeof(CocomeTest).GetTypeInfo().Assembly.GetManifestResourceStream(file))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					content = reader.ReadToEnd();
				}
			}

			string[] interestedCoroutines =
			{
				"CoCoMESystem::openStore",
				"CoCoMESystem::openCashDesk",
				"ProcessSaleService::makeNewSale",
				"ProcessSaleService::enterItem",
				"ManageStoreCRUDService::createStore",
				"ManageCashDeskCRUDService::createCashDesk",
				"ManageItemCRUDService::createItem",

				"ProcessSaleService::makeCashPayment",
				//"ProcessSaleService::makeCardPayment",

			};
			string[] lowPriorityCoroutines =
			{
				"ManageItemCRUDService::deleteItem",
				"ManageStoreCRUDService::deleteStore",
				"ManageCashDeskCRUDService::deleteCashDesk",
			};

			var inheritance = REModelStart.GetObjectInheritance(content);
			var generators = REModelStart.GetAllGenerators(content, inheritance);

			List<string> realFunctionOrder = new List<string>();
			CoroutineInstanceType result = REModelStart.Compose(content, realFunctionOrder, generators, interestedCoroutines, lowPriorityCoroutines);


			Assert.Equal(ConcreteType.Void, result.Receive);
			Assert.Equal((ConcreteType)"CurrentStore", ((SequenceType)result.Yield).Types[0]);
			Assert.Equal((ConcreteType)"CurrentCashDesk", ((SequenceType)result.Yield).Types[1]);
			Assert.Equal((ConcreteType)"Sale", ((SequenceType)result.Yield).Types[2]);
			Assert.Equal((ConcreteType)"CashPayment", ((SequenceType)result.Yield).Types[3]);
			Assert.Equal((ConcreteType)"SalesLineItem", ((SequenceType)result.Yield).Types[4]);
			Assert.Equal((ConcreteType)"CurrentSaleLine", ((SequenceType)result.Yield).Types[5]);


			Assert.Equal("ManageStoreCRUDService::createStore", realFunctionOrder[0]);
			Assert.Equal("CoCoMESystem::openStore", realFunctionOrder[1]);
			Assert.Equal("ManageCashDeskCRUDService::createCashDesk", realFunctionOrder[2]);
			Assert.Equal("CoCoMESystem::openCashDesk", realFunctionOrder[3]);
			Assert.Equal("ManageItemCRUDService::createItem", realFunctionOrder[4]);
			Assert.Equal("ProcessSaleService::makeNewSale", realFunctionOrder[5]);
			Assert.Equal("ProcessSaleService::enterItem", realFunctionOrder[6]);
			Assert.Equal("ProcessSaleService::makeCashPayment", realFunctionOrder[7]);

		}
	}
}
