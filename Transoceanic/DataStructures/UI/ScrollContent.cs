using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Transoceanic.DataStructures.UI
{
    public class ScrollContent : UIElement
    {
        public override void Recalculate()
        {
            //Left.Set(0, 0);
            //Width = Parent.Width;
            base.Recalculate();
        }
        public override void OnInitialize()
        {
            MaxHeight = Height;
            //if(Parent != null)
            //    Width = Parent.Width;
            base.OnInitialize();
        }
        public virtual void OnSetVisible()
        {

        }
        public virtual void OnSetInvisible()
        {

        }
        public virtual void Rearrange()
        {
            MaxHeight = Height;
        }
    }
}
