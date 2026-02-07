using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UntilWeFall
{
	public sealed class Playing : GameStateBase
	{
		private readonly GameContext _ctx;
    		private readonly Action<GameStateID> _changeState;

		public Playing(GameContext ctx, Action<GameStateID> changeState)
			: base(ctx, changeState)
		{

		}

		public void Enter()
		{
			
		}

		public void Exit()
		{
			
		}

		public void Update(GameTime gameTime) 
		{	
			var kb = Keyboard.GetState();
			var mouse = Mouse.GetState();
		}

		public void Draw(GameTime gameTime)
		{
			var _spriteBatch = _ctx.SpriteBatch;
		}
	}
}