﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using DiffSyntax.Antlr;
using System.Linq;
using GeneratorCalculation;

namespace RequirementAnalysis
{
	public class REModelStart
	{
		static string ExtractParenthesesContent(string input)
		{
			// 提取括号及其内容的函数
			// 查找第一个左括号的索引
			int leftParenthesesIndex = input.IndexOf('(');
			if (leftParenthesesIndex == -1)
			{
				throw new Exception("Left parentheses not found.");
			}

			// 查找第一个右括号的索引
			int rightParenthesesIndex = input.IndexOf(')');
			if (rightParenthesesIndex == -1)
			{
				throw new Exception("Right parentheses not found.");
			}

			// 提取括号及其内容
			string content = input.Substring(leftParenthesesIndex, rightParenthesesIndex - leftParenthesesIndex + 1);
			return content;
		}
		static void Main(string[] args)
		{

			// Step 1: Load the file content into a string.
			string path = @"C:\Users\p2215981\Desktop\Liu.Lixue\coroutine-program\RequirementAnalysisTests\cocome.remodel";

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

			List<Generator> generators = FindTypes(path);

			foreach (var g in generators)
				Console.WriteLine($"{g.Name}:\t{g.Type}");

			Console.WriteLine("\nNow, let's compose interested coroutines.");
			CoroutineInstanceType result = Compose(path, generators, interestedCoroutines, lowPriorityCoroutines);
			Console.WriteLine(result);
		}

		public static List<Generator> FindTypes(string remodelPath)
		{
			string content = File.ReadAllText(remodelPath);


			var inheritance = GetObjectInheritance(content);
			return GetAllGenerators(content, inheritance);
		}

		public static CoroutineInstanceType Compose(string filePath, List<Generator> generators, string[] interestedCoroutines = null, string[] lowPriorityCoroutines = null)
		{
			List<Generator> filtered;
			if (interestedCoroutines != null)
				filtered = generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1).ToList();
			else
				filtered = generators;

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in filtered)
				bindings.Add(g.Name, g.Type);

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType(from b in bindings select b.Key))));
			if (lowPriorityCoroutines != null)
				coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines, g.Name) != -1));

			List<CoroutineInstanceType> compositionOrder = new List<CoroutineInstanceType>();
			var solver = new Solver();
			solver.YieldEvent += (object sender, CompositionEventArgs e) =>
			{
				compositionOrder.Add(e.ProcessedCoroutine);
			};
			solver.ResumeEvent += (object sender, CompositionEventArgs e) =>
			{
				compositionOrder.Add(e.ProcessedCoroutine);
			};

			var result = solver.SolveWithBindings(coroutines, bindings);


			//store function names
			List<string> modifiedValues = new List<string>();

			Console.WriteLine("------Composition order:\n" + string.Join(" ---->\n", compositionOrder.Select(g =>
			{
				string p;
				if (g is CoroutineInstanceType cg)
				{
					p = string.IsNullOrEmpty(cg.Source?.Name) ? g.ToString() : cg.Source.Name;
				}
				else
				{
					p = g.ToString();
				}

				if (p.Split("::").Length == 2)
				{
					// 将 p 的值写入到文件中，且去重
					if (!modifiedValues.Contains(p))
					{
						string modifiedValue = p;
						modifiedValues.Add(modifiedValue);
					}
				}
				return p;
			})));

			foreach (var value in modifiedValues)
			{
				Console.WriteLine(value);

				string[] lines = File.ReadAllLines(filePath);

				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].Contains(value))
					{
						Console.WriteLine($"Field '{value}' found in line {i + 1}: {lines[i]}");

						string content = ExtractParenthesesContent(lines[i]);
						Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^" + value + content);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns>subclass : superclass</returns>
		public static Dictionary<string, string> GetObjectInheritance(string content)
		{
			var result = new Dictionary<string, string>();
			foreach (Match m in Regex.Matches(content, @"Actor\s+(\w+)\s+extends\s+(\w+)"))
			{
				result.Add(m.Groups[1].Value, m.Groups[2].Value);
			}

			return result;
		}

		public static List<Generator> GetAllGenerators(string content, Dictionary<string, string> inheritance)
		{
			Dictionary<string, ServiceBlock> serviceDefinitions = CollectProperties(content).ToDictionary(d => d.Name);

			List<Generator> generators = new List<Generator>();
			int startPosition = 0;
			// Step 2: Find the specific section that you want to parse.
			string marker = "Contract ";
			while (true)
			{
				int contractIndex = content.IndexOf(marker, startPosition);
				if (contractIndex < 0)
					break;

				int sectionEndIndex = content.IndexOf("}", contractIndex);
				if (sectionEndIndex < 0)
				{
					Console.WriteLine("Section end not found.");
					return generators;
				}

				string sectionContent = content.Substring(contractIndex, sectionEndIndex - contractIndex + 1);

				// Step 3: Parse the section using the Antlr4 parser.
				var g = ContractAnalyzer.GetGenerator(serviceDefinitions, sectionContent, inheritance);
				if (g != null)
					generators.Add(g);

				startPosition = sectionEndIndex;
			}

			return generators;
		}


		static List<ServiceBlock> CollectProperties(string code)
		{
			string servicePattern = @"Service\s+(\w+)\s*\{(.+?)\}";
			string propertyPattern = @"\[TempProperty\](.+)\n";

			List<ServiceBlock> serviceDefinitions = new List<ServiceBlock>();
			while (code.Length > 0)
			{
				Match m = Regex.Match(code, servicePattern, RegexOptions.Singleline);

				if (!m.Success)
					break;

				ServiceBlock service = new ServiceBlock();
				serviceDefinitions.Add(service);
				service.Name = m.Groups[1].Value;

				string serviceBlock = m.Groups[2].Value;

				Match propertyMatch = Regex.Match(serviceBlock, propertyPattern, RegexOptions.Singleline);

				if (propertyMatch.Success)
				{

					string propertyBlock = propertyMatch.Groups[1].Value;

					foreach (Match match in Regex.Matches(propertyBlock, @"(\w+)\s*:\s*(\w+)"))
					{
						service.Properties[match.Groups[1].Value] = match.Groups[2].Value;
						//Console.WriteLine($"{match.Groups[1].Value}: {match.Groups[2].Value}");
					}
				}

				code = code.Substring(m.Index + m.Length);
			}

			return serviceDefinitions;
		}


	}

}
