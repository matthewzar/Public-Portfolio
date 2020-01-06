using Turbo.Plugins.Default;
using System.Linq;
using Microsoft.Win32;

namespace Turbo.Plugins.User.HeroIsNearNew
{
    public class HeroIsNearNew : BasePlugin, IInGameTopPainter
    {
		const string ROOT_KEY = "ABC";
		const string SUB_KEY = "Key1";

		const string REGISTRY_ENTRY_NAME = "NewField";
		const string IN_TOWN_FLAG = "InTown";
		const string UPDATE_FIELD = "LastUpdate";
		
		public HeroClass Heroclass { get; set; } 
		public float Distance { get; set; }

		public HeroIsNearNew()
		{
			Enabled = true;

			Heroclass = HeroClass.Wizard; //HeroClass.Barbarian , HeroClass.Crusader , HeroClass.DemonHunter, HeroClass.Monk , HeroClass.WitchDoctor, HeroClass.Wizard, HeroClass.Necromancer
			Distance = 50f;

			var rkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(ROOT_KEY).CreateSubKey(SUB_KEY);
			rkey.SetValue(REGISTRY_ENTRY_NAME, false);
			rkey.SetValue(UPDATE_FIELD, System.DateTime.Now.ToString());
            rkey.Close();	
		}
		
		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 30001;
		}

		public void PaintTopInGame(ClipState clipState) 
		{
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame) return;

			//Removed the early-return check for in-town, because in the original, being in town would mean no
			//rectangle gets drawn. But in our version, the registry key will persist regardless (where there used to 
			//be a temporary rectangle).
						
			bool near = Hud.Game.Players.Where(p => !p.IsMe 
													&& p.HasValidActor 
													&& p.HeroClassDefinition.HeroClass == Heroclass
													&& p.CentralXyDistanceToMe <= Distance).Any();
			
			RegistryKey rkey = null;
			try
			{
				rkey = Registry.CurrentUser.OpenSubKey(ROOT_KEY, true).OpenSubKey(SUB_KEY, true);  
				rkey.SetValue(REGISTRY_ENTRY_NAME, near);
				rkey.SetValue(IN_TOWN_FLAG, Hud.Game.IsInTown);
				rkey.SetValue(UPDATE_FIELD, System.DateTime.Now.ToString());
			}
			finally
			{
				//Even if something went wrong, try to close the registry key between uses.
				rkey?.Close();  
			}
        }
    }
}