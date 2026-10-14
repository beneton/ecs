using System;
using System.IO;
using System.Linq;
using Beneton.ECS.Core.CodeGen;
using NUnit.Framework;

namespace Beneton.ECS.Core.Tests
{
	/// <summary>
	/// Runs the component id generator's scanner on sample source files in a temporary folder.
	/// The samples write STRUCT instead of the keyword, replaced when the file is written,
	/// so the real generator scanning this test file doesn't pick the samples up as components.
	/// </summary>
	public class ComponentSourceScannerTests
	{
		private string _folder;

		[SetUp]
		public void CreateFolder()
		{
			_folder = Path.Combine(Path.GetTempPath(), "EcsScannerTests_" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_folder);
		}

		[TearDown]
		public void DeleteFolder()
		{
			Directory.Delete(_folder, true);
		}

		private void WriteSource(string fileName, string code)
		{
			File.WriteAllText(Path.Combine(_folder, fileName), code.Replace("STRUCT", "struct"));
		}

		private string[] FindComponents()
		{
			return ComponentSourceScanner.FindComponents(_folder)
				.Select(c => string.IsNullOrEmpty(c.Namespace) ? c.Name : $"{c.Namespace}.{c.Name}")
				.OrderBy(name => name, StringComparer.Ordinal)
				.ToArray();
		}

		[Test]
		public void FindsEverySupportedWayOfNamingTheInterface()
		{
			WriteSource("ShortName.cs", @"
using Beneton.ECS.Core;
namespace Game { public partial STRUCT ShortName : IComponent { } }");

			WriteSource("FullName.cs", @"
namespace Game { public partial STRUCT FullName : Beneton.ECS.Core.IComponent { } }");

			WriteSource("GlobalName.cs", @"
namespace Game { public partial STRUCT GlobalName : global::Beneton.ECS.Core.IComponent { } }");

			WriteSource("Aliased.cs", @"
using Core = Beneton.ECS.Core;
namespace Game { public partial STRUCT Aliased : Core.IComponent { } }");

			WriteSource("Inside.cs", @"
namespace Beneton.ECS.Core.Sub { public partial STRUCT Inside : IComponent { } }");

			WriteSource("Singleton.cs", @"
using System;
using Beneton.ECS.Core;
namespace Game
{
	public partial STRUCT Singleton : IEquatable<Singleton>, ISingletonComponent
	{
		public bool Equals(Singleton other) => true;
	}
}");

			CollectionAssert.AreEqual(
				new[]
				{
					"Beneton.ECS.Core.Sub.Inside",
					"Game.Aliased",
					"Game.FullName",
					"Game.GlobalName",
					"Game.ShortName",
					"Game.Singleton"
				},
				FindComponents());
		}

		[Test]
		public void IgnoresLookalikes()
		{
			WriteSource("Getter.cs", @"
using Beneton.ECS.Core;
namespace Game { public STRUCT Getter : IComponentGetter { } }");

			WriteSource("NoUsing.cs", @"
namespace Game { public STRUCT NoUsing : IComponent { } }");

			WriteSource("StaticUsing.cs", @"
using static Beneton.ECS.Core.Entity;
namespace Game { public STRUCT StaticUsing : IComponent { } }");

			WriteSource("OtherAlias.cs", @"
using Other = Some.Other;
namespace Game { public STRUCT OtherAlias : Other.IComponent { } }");

			WriteSource("NotAStruct.cs", @"
using Beneton.ECS.Core;
namespace Game { public class NotAStruct : IComponent { public int TypeId => 0; } }");

			CollectionAssert.IsEmpty(FindComponents());
		}

		[Test]
		public void IgnoresComments_AndCommentMarkersInsideStrings()
		{
			WriteSource("Comments.cs", @"
using Beneton.ECS.Core;
namespace Game
{
	// public partial STRUCT LineComment : IComponent { }
	/* public partial STRUCT BlockComment : IComponent { } */
	/*
	public partial STRUCT MultiLineComment : IComponent { }
	*/
	public static class Url { public const string Value = ""http://example.com""; } public partial STRUCT AfterUrl : IComponent { }
	public static class Opener { public const string Value = ""/*""; public const string Path = @""C:\temp\""; }
	public partial STRUCT AfterOpener : IComponent { }
}");

			CollectionAssert.AreEqual(new[] { "Game.AfterOpener", "Game.AfterUrl" }, FindComponents());
		}
	}
}
