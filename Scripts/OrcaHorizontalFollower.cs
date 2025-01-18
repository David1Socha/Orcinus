using Godot;
using System;

public class OrcaHorizontalFollower : Node2D
{
    private float _initialYPosition;

    public override void _Ready()
    {
        base._Ready();

        _initialYPosition = Position.y;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Position = new Vector2(Position.x, _initialYPosition);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        Position = new Vector2(Position.x, _initialYPosition);
    }
}
