using System;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Beneton.ECS.Core.Tests
{
	/// <summary>
	/// Gives every test an empty scene, and helpers to build a World with inline systems and baked GameObjects.
	/// Unity's test runner fails any test that logs an unexpected error, so every test also checks
	/// that the ECS didn't report misuse. Expected errors are declared with LogAssert.Expect.
	/// </summary>
	public abstract class EcsTestBase
	{
		protected const float Dt = 0.016f;

		protected World World;

		[SetUp]
		public void SetUpScene()
		{
			// Drops the World's ECS-Inspector-Helper and every GameObject a previous test created
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			World = null;
		}

		[TearDown]
		public void TearDownScene()
		{
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			World = null;
		}

		protected World CreateWorld(bool considerInactiveGameObjects = false)
		{
			World = new World(considerInactiveGameObjects);
			return World;
		}

		protected LambdaSystem AddSystem(
			LambdaSystem.UpdateAction onUpdate,
			Action<IArchetypeProvider> onCreate = null)
		{
			return World.AddSystem(new LambdaSystem(onUpdate, onCreate));
		}

		protected LambdaSystem AddSystemLate(
			LambdaSystem.UpdateAction onUpdate,
			Action<IArchetypeProvider> onCreate = null)
		{
			return World.AddSystemLate(new LambdaSystem(onUpdate, onCreate));
		}

		/// <summary>
		/// Creates a GameObject with a Baker for each component asked for. Null means no Baker for it.
		/// The Bakers live in Beneton.ECS.Core.Tests.Support: AddComponent returns null for a
		/// MonoBehaviour compiled into an Editor-only assembly.
		/// </summary>
		protected static GameObject CreateBaked(
			string name,
			int? a = null,
			int? b = null,
			bool tag = false,
			Transform parent = null)
		{
			var gameObject = new GameObject(name);
			if (parent != null)
			{
				gameObject.transform.SetParent(parent);
			}

			if (a.HasValue)
			{
				gameObject.AddComponent<TestABaker>().Value = a.Value;
			}

			if (b.HasValue)
			{
				gameObject.AddComponent<TestBBaker>().Value = b.Value;
			}

			if (tag)
			{
				gameObject.AddComponent<TestTagBaker>();
			}

			return gameObject;
		}

		protected Entity EntityOf(GameObject gameObject)
		{
			Assert.IsTrue(World.TryGetEntity(gameObject, out var entity), $"{gameObject.name} is not an entity");
			return entity;
		}

		protected void RunFrame()
		{
			World.Update(Dt);
			World.LateUpdate(Dt);
		}
	}
}
