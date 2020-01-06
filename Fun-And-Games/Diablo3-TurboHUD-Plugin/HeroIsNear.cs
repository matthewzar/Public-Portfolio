using Turbo.Plugins.Default;
using System.Linq;

namespace Turbo.Plugins.RNN
{
    public class HeroIsNear : BasePlugin, IInGameTopPainter
    {
		public IBrush BorderBrush { get; set; }
		public IBrush BackgroundBrush1 { get; set; }
		public IBrush BackgroundBrush2 { get; set; }
		
		public HeroClass Heroclass { get; set; } 
		public float Distance { get; set; }
		
		public float WidthRectangle { get; set; }
		public float HeightRectangle { get; set; }
		public float Offset { get; set; }
		
		public float xPos { get; set; } 
		public float yPos { get; set; } 
		
		public HeroIsNear()
		{
			Enabled = true;			
		}
		
		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 30001;
			
			Heroclass = HeroClass.Wizard; //HeroClass.Barbarian , HeroClass.Crusader , HeroClass.DemonHunter, HeroClass.Monk , HeroClass.WitchDoctor, HeroClass.Wizard, HeroClass.Necromancer
			Distance = 50f;

			WidthRectangle =  (Hud.Window.Size.Width * 0.015f) ;
			HeightRectangle =  (Hud.Window.Size.Height * 0.01f) ;			
			Offset = - (Hud.Window.Size.Height / 6) ;
			
			xPos = (Hud.Window.Size.Width - WidthRectangle) / 2 ;
			yPos =  (Hud.Window.Size.Height - HeightRectangle) / 2 + Offset;
						
			BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 1);
			BackgroundBrush1 = Hud.Render.CreateBrush(255, 0, 250, 0, 0);
			BackgroundBrush2 = Hud.Render.CreateBrush(255, 255, 0, 0, 0);
		}
		public void PaintTopInGame(ClipState clipState) 
		{
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame || Hud.Game.IsInTown) { return; }
			bool near = Hud.Game.Players.Where(p => !p.IsMe && p.HasValidActor && (p.HeroClassDefinition.HeroClass == Heroclass) && (p.CentralXyDistanceToMe <= Distance)).Any();
			(near?BackgroundBrush1:BackgroundBrush2).DrawRectangle(xPos, yPos, WidthRectangle, HeightRectangle);
			BorderBrush.DrawRectangle(xPos , yPos, WidthRectangle, HeightRectangle);
        }

    }
}