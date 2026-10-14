using System.Runtime.CompilerServices;

// The component id generator's scanner is internal; its tests live in their own assembly
// so the main test assembly only sees the public API, exactly like game code does.
[assembly: InternalsVisibleTo("Beneton.ECS.Core.Tests.CodeGen")]
