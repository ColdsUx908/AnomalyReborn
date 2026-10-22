using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using System;
namespace Transoceanic.DataStructures.UI
{
    public class ModUIPanel : UIPanel
    {
        protected bool Resizeable;
        protected bool Resizing;
        private readonly int _MinResizeWidth;
        private readonly int _MinResizeHeight;

        protected bool Draggable;
        protected bool Dragging;

        internal Vector2 Offset;

        protected event ElementEvent OnRecalculate;
        protected event ElementEvent OnDrag;
        protected event ElementEvent OnResize;

        internal Asset<Texture2D> ResizeIndicator;

        public ModUIPanel(bool draggable = true, bool resizeable = false, int minResizeWidth = 160, int minResizeHeight = 90)
        {
            Draggable = draggable;
            Resizeable = resizeable;
            OnLeftMouseDown += DragStart;
            OnLeftMouseUp += DragEnd;
            _MinResizeWidth = minResizeWidth;
            _MinResizeHeight = minResizeHeight;
            ResizeIndicator = ModContent.Request<Texture2D>("Transoceanic/DataStructures/UI/Textures/Resize");
        }

        public override void Recalculate()
        {
            base.Recalculate();
            OnRecalculate?.Invoke(this);
        }

        public Rectangle ResizeRectangle
        {
            get
            {
                CalculatedStyle innerDimensions = GetInnerDimensions();
                return new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + (int)PaddingRight, 12 + (int)PaddingBottom);
            }
        }

        // 可拖动/调整大小界面
        private void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            //CalculatedStyle innerDimensions = GetInnerDimensions();
            if (evt.Target != this)
                return;
            if (Resizeable && ResizeRectangle.Contains(evt.MousePosition.ToPoint()))
            {
                CalculatedStyle dimensions = GetOuterDimensions();
                Offset = evt.MousePosition - new Vector2(dimensions.X + dimensions.Width, dimensions.Y + dimensions.Height);
                Resizing = true;
            }
            else if (Draggable)
            {
                Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
                Dragging = true;
            }
        }

        // 可拖动/调整大小界面
        private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            Dragging = false;
            Resizing = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (Resizeable)
            {
                CalculatedStyle dimensions = GetInnerDimensions();
                Vector2 position = dimensions.ToRectangle().BottomRight() - new Vector2(4f);
                Texture2D texture = ResizeIndicator.Value;

                spriteBatch.Draw(texture, position, BorderColor);

                if (ResizeRectangle.Contains(UserInterface.ActiveInstance.MousePosition.ToPoint()) || Resizing)
                {
                    Main.cursorOverride = CursorOverrideID.GamepadDefaultCursor;
                    Main.cursorColor = Color.SkyBlue;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!Main.mouseLeft) { Dragging = false; Resizing = false; }
            base.Update(gameTime);

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            CalculatedStyle dimensions = GetOuterDimensions();

            if (Dragging)
            {
                Left.Set(UserInterface.ActiveInstance.MousePosition.X - Offset.X, 0f);
                Top.Set(UserInterface.ActiveInstance.MousePosition.Y - Offset.Y, 0f);
                Recalculate();
                OnDrag?.Invoke(this);
            }
            if (Resizing)
            {
                Width.Pixels = UserInterface.ActiveInstance.MousePosition.X - dimensions.X - Offset.X;
                Height.Pixels = UserInterface.ActiveInstance.MousePosition.Y - dimensions.Y - Offset.Y;
                // 限制
                if (Width.Pixels <= _MinResizeWidth)
                {
                    Width.Pixels = _MinResizeWidth;
                }
                if (Height.Pixels <= _MinResizeHeight)
                {
                    Height.Pixels = _MinResizeHeight;
                }
                OnResize?.Invoke(this);
                Recalculate();
            }
        }
    }

}

