// Celeste.DisperseImage
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class ShrisperseImage : Entity
{
	private class Particle
	{
		public Vector2 Position;

		public Vector2 Direction;

		public float Speed;

		public float Sin;

		public float Alpha;

		public float Percent;

		public float Duration;

		public MTexture Image;
	}

	private List<Particle> particles = new List<Particle>();

	private Vector2 scale;

	public Color color;

	public ShrisperseImage(Vector2 position, Vector2 direction, Vector2 origin, Vector2 scale, MTexture texture, Color col)
	{
		Position = position;
		this.scale = new Vector2(Math.Abs(scale.X), Math.Abs(scale.Y));
		float num = direction.Angle();
		color = col;
		for (int i = 0; i < texture.Width; i++)
		{
			for (int j = 0; j < texture.Height; j++)
			{
				particles.Add(new Particle
				{
					Position = position + scale * (new Vector2(i, j) - origin),
					Direction = Calc.AngleToVector(num + Calc.Random.Range(-3f, 3f), 1f),
					Sin = Calc.Random.NextFloat((float)Math.PI * 2f),
					Speed = Calc.Random.Range(0f, 4f),
					Alpha = 1f,
					Percent = 0f,
					Duration = Calc.Random.Range(0.5f, 0.8f),
					Image = new MTexture(texture, i, j, 1, 1)
				});
			}
		}
	}

	public override void Update()
	{
		bool flag = false;
		foreach (Particle particle in particles)
		{
			particle.Percent += Engine.DeltaTime / particle.Duration;
			particle.Position += particle.Direction * particle.Speed * Engine.DeltaTime;
			particle.Position += (float)Math.Sin(particle.Sin) * particle.Direction.Perpendicular() * particle.Percent * 4f * Engine.DeltaTime;
			particle.Speed += Engine.DeltaTime * (4f + particle.Percent * 80f);
			particle.Sin += Engine.DeltaTime * 4f;
			if (particle.Percent < 1f)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			RemoveSelf();
		}
	}

	public override void Render()
	{
		foreach (Particle particle in particles)
		{
			particle.Image.Draw(particle.Position, Vector2.Zero, color * (1f - particle.Percent), scale);
		}
	}
}
