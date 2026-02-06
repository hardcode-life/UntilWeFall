using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UntilWeFall
{
	public abstract class GameStateBase : IGameState
	{
		protected GameContext CTX {get;}
		protected Action<GameStateID> ChangeState {get;}
		//protected SpriteBatch SB => CTX.SpriteBatch;

		/*
			IGameState
			↑
			GameStateBase
			↑
			Creation / Loading / Playing / StartMenu

			/╲/\╭( ͡°͡° ͜ʖ ͡°͡°)╮/\╱\
		*/

		protected GameStateBase(GameContext ctx, Action<GameStateID> changeState ){
			CTX = ctx ?? throw new ArgumentNullException(nameof(ctx));
			ChangeState = changeState ?? throw new ArgumentNullException(nameof(changeState));
		}

		public virtual void Enter()
		{
			
		}
		public virtual void Exit()
		{
			
		}
		public virtual void Update(GameTime gameTime)
		{
			
		}
		public virtual void Draw(GameTime gameTime)
		{
			
		}
	}
}