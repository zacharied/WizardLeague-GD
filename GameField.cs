using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using wizardballz;

public partial class GameField : Node3D
{
	public Node3D[] SpellCastSpawns = new Node3D[WizardBallz.PlayerCount];
	public Area3D[] GoalZones = new Area3D[WizardBallz.PlayerCount];
	public Vector3[] TurretPositions = new Vector3[WizardBallz.PlayerCount];
	
	public override void _Ready()
	{
		GenerateStaticBodies();
		
		for (var i = 0; i < SpellCastSpawns.Length; i++) {
			SpellCastSpawns[i] = GetNode<Node3D>($"GAME/SpellSpawn{i + 1}");
			GoalZones[i] = GetNode<Area3D>($"GAME/GoalZone{i + 1}");
			var gate = GetNode<Node3D>($"GATES/Gate{i + 1}");
			var aabb = gate.GetChildrenOfType<MeshInstance3D>().Single().GetAabb().Size * gate.Scale;
			TurretPositions[i] = gate.GlobalPosition with { Y = gate.GlobalPosition.Y + aabb.Y };
		}
	}

	public override void _Process(double delta)
	{
	}

	private void GenerateStaticBodies()
	{
		var stopwatch = Stopwatch.StartNew();
		
		var shapes = new Dictionary<Mesh, ConvexPolygonShape3D>();
		var bodyCount = 0;
		
		foreach (var node in GetChildren().Where(n => n.IsInGroup("generate_static_bodies"))) {
			foreach (var child in node.RecurseChildren()) {
				var childContents = child.GetChildren();
				if (childContents.Any(n => n is MeshInstance3D) && !childContents.Any(n => n is PhysicsBody3D)) {
					// This node only has a mesh instance, no body
					// Create a static body with the same shape as the mesh
					if (child.IsInGroup("generate_static_bodies_ignore")) {
						continue;
					}
					
					var body = new StaticBody3D() { CollisionLayer = 1 << 1 };
					var meshInstance = child.GetChildOfType<MeshInstance3D>();
					if (!shapes.ContainsKey(meshInstance.Mesh)) {
						shapes[meshInstance.Mesh] = meshInstance.Mesh.CreateConvexShape();
					}
					var collider = new CollisionShape3D()
						{ Shape = shapes[meshInstance.Mesh] };
					body.AddChild(collider);
					child.AddChild(body);
					
					bodyCount++;
				}
			}
		}
		
		stopwatch.Stop();
		
		GD.Print($"Generated {bodyCount} static bodies ({shapes.Count} unique meshes) in {stopwatch.ElapsedMilliseconds}ms");
	}
}
